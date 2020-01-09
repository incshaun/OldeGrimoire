// Compile with:
// g++ ta.cc -o ta tensorflow/tensorflow/lite/tools/make/gen/linux_x86_64/lib/libtensorflow-lite.a -I tensorflow/ -I tensorflow/tensorflow/lite/tools/make/downloads/flatbuffers/include/ -lpthread

// Disable absl in the Makefile

// Knock out a couple of other unused functions; might need to clean this up later.

//g++ -fPIC ta.cc -o libta.so -shared -Wl,--whole-archive  tensorflow/tensorflow/lite/tools/make/gen/linux_x86_64/lib/libtensorflow-lite.a -I tensorflow/ -I tensorflow/tensorflow/lite/tools/make/downloads/flatbuffers/include/  -lpthread -Wl,--no-whole-archive; cp libta.so PoseTest/Assets/Plugins/x86_64


#include <stdio.h>

#include <tensorflow/lite/model.h>
#include <tensorflow/lite/core/api/error_reporter.h>
#include <tensorflow/lite/kernels/register.h>

#ifdef ANDROID
#include <GLES3/gl31.h>
#else
#include <GL/gl.h>
#endif

extern "C" 
{
  void initPose ();
  int computePose (int texture, int w, int h, float * results);
}

// For debugging, read the input image from a ppm file. Generic function
// from external sources.
unsigned char * readPPMfile (const char* filename, int *wp, int *hp) 
{
  FILE* input;
  int w, h, max;
  int i, j, k;
  char rgb [3];
  unsigned char* pixels;
  char buffer[200];
  
  if ((input = fopen (filename, "r")) == NULL) 
  {
    fprintf (stderr, "Cannot open file %s \n", filename);
    exit (1);
  }
  
  /* read a line of input */
  fgets (buffer, 200, input);
  if (strncmp (buffer, "P6", 2) != 0) 
  {
    fprintf (stderr, "%s is not a binary PPM file \n", filename);
    exit (1);
  }
  
  /* get second line, ignoring comments */
  do 
  {
    fgets (buffer, 200, input);
  }
  while (strncmp (buffer, "#", 1) == 0);
  
  if (sscanf (buffer, "%d %d", &w, &h) != 2) 
  {
    fprintf (stderr, "can't read sizes! \n");
    exit (1);
  }
  
  /* third line, ignoring comments */
  do 
  {
    fgets (buffer, 200, input);
  }
  while (strncmp (buffer, "#", 1) == 0);
  
  if (sscanf (buffer, "%d", &max) != 1) 
  {
    fprintf (stderr, "what about max size? \n");
    exit (1);
  }
  
  pixels = (unsigned char*) malloc (w * h * 3 * sizeof(unsigned char));
  for (i = 0; i < h; i++) 
  {
    for (j = 0; j < w; j++) 
    {
      fread (rgb, sizeof(char), 3, input);
      for (k = 0; k < 3; k++)
      {
        *(pixels + (i) * w * 3 + j * 3 + k) = (unsigned char) rgb[k];
      }
    }
  }
  
  *wp = w;
  *hp = h;
  return pixels;
}

// Write the image file, useful for showing output. Generic function
// from external sources.
void writePPM (unsigned char * data, int w, int h)
{
  int i, j;
  FILE *fp = fopen ("posedetection.ppm", "wb");
  (void) fprintf (fp, "P6\n%d %d\n255\n", w, h);
  for (j = 0; j < h; ++j)
  {
    for (i = 0; i < w; ++i)
    {
      static unsigned char color[3];
      color[0] = data[3 * ((j * w) + i) + 0];
      color[1] = data[3 * ((j * w) + i) + 1];
      color[2] = data[3 * ((j * w) + i) + 2];
      (void) fwrite(color, 1, 3, fp);
    }
  }
  (void) fclose(fp);
}

// Make a mark on the image data. Useful for plotting output.
void plot (unsigned char * data, int w, int h, int x, int y)
{
  int size = 5;
  for (int i = -size; i < size; i++)
  {
    for (int j = -size; j < size; j++)
    {
      if ((x + i >= 0) && (x + i < w) && (y + j >= 0) && (y + j < h))
      {
        data[3 * (((y + j) * w) + (x + i)) + 0] = 255;
        data[3 * (((y + j) * w) + (x + i)) + 1] = 0;
        data[3 * (((y + j) * w) + (x + i)) + 2] = 0;
      }
    }
  }
}

std::unique_ptr<tflite::FlatBufferModel> model;
tflite::ops::builtin::BuiltinOpResolver resolver;
std::unique_ptr<tflite::Interpreter> interpreter;

// Load the reusable elements of the process, such as model.
void initPose ()
{
  model = tflite::FlatBufferModel::BuildFromFile ("posenet_mobilenet_v1_100_257x257_multi_kpt_stripped.tflite"); //"multi_person_mobilenet_v1_075_float.tflite");
  tflite::InterpreterBuilder(*model, resolver)(&interpreter);

#ifdef TESTPOSEINTERFACE
  printf ("Inputs %d Outputs %d\n", interpreter->inputs ().size (), interpreter->outputs ().size ());
#endif
  
  interpreter->AllocateTensors();
}

// Analyse the image provided and return the pose data.
void callPose (unsigned char * data, int width, int height, float * results)
{
  // Find the dimensions of the input image required by this model.
  TfLiteIntArray* dims = interpreter->tensor(interpreter->inputs()[0])->dims;
  int wanted_height = dims->data[1];
  int wanted_width = dims->data[2];
  int wanted_channels = dims->data[3];
#ifdef TESTPOSEINTERFACE
  printf ("Wanted %d: %d %d %d %d\n", dims->size, dims->data[0],wanted_height, wanted_width, wanted_channels);
#endif

  // Find the dimensions of the output heatmap as well.  
  TfLiteIntArray* output_dims = interpreter->tensor(interpreter->outputs()[0])->dims;
#ifdef TESTPOSEINTERFACE
  printf ("Output dims %d  - %d %d %d %d\n", output_dims->size, output_dims->data[0], output_dims->data[1], output_dims->data[2], output_dims->data[3]);
#endif
  int outheight = output_dims->data[1];
  int outwidth = output_dims->data[2];
  int numKeypoints = output_dims->data[3];

  // Copy the image into the input buffer, rescaling at the same time.  
  float * inputBuffer = interpreter->typed_input_tensor<float>(0);
  float shrinkx = width / ((float) wanted_width);
  float shrinky = height / ((float) wanted_height);
  
  float mean = 128.0;
  float std = 128.0;
  unsigned char pixelValue;
  for (int row  = 0; row < wanted_height; row++)
  {
    for (int col = 0; col < wanted_width; col++) 
    {
      pixelValue = data[3 * (((int) ((wanted_height - 1 - row) * shrinky)) * width + ((int) (col * shrinkx))) + 0];
      inputBuffer[3 * (row * wanted_width + col) + 0] = (((float) (pixelValue - mean)) / std);
      pixelValue = data[3 * (((int) ((wanted_height - 1 - row) * shrinky)) * width + ((int) (col * shrinkx))) + 1];
      inputBuffer[3 * (row * wanted_width + col) + 1] = (((float) (pixelValue - mean)) / std);
      pixelValue = data[3 * (((int) ((wanted_height - 1 - row) * shrinky)) * width + ((int) (col * shrinkx))) + 2];
      inputBuffer[3 * (row * wanted_width + col) + 2] = (((float) (pixelValue - mean)) / std);
    }
  }
  
  // Do the recognition.
  interpreter->Invoke();
  
  // Extract meaning from the output. Combine the most reliable heatmap entry with the offset to determine
  // exact position of each point.
  float * heatmaps = interpreter->typed_output_tensor<float>(0);
  float * offsets = interpreter->typed_output_tensor<float>(1);
  
  TfLiteTensor * heatmaps_tensor = interpreter->tensor (0);
  
  for (int keypoint = 0; keypoint < numKeypoints; keypoint++) 
  {
    float maxVal = heatmaps[numKeypoints * (outwidth * (0) + 0) + keypoint];
    int maxRow = 0;
    int maxCol = 0;
    for (int row = 0; row < outheight; row++) 
    {
      for (int col = 0; col < outwidth; col++)
      {
        float h = heatmaps[numKeypoints * (outwidth * (row) + col) + keypoint]; 
        if (h > maxVal) 
        {
          maxVal = h;
          maxRow = row;
          maxCol = col;
        }
      }
    }
    
    
    float positionX = ((((float) maxCol) / (outwidth-1)) * wanted_width + offsets[2 * numKeypoints * (outwidth * (maxRow) + maxCol) + keypoint + numKeypoints]) * shrinkx;
    float positionY = (wanted_height - (((((float) maxRow) / (outheight-1)) * wanted_height + offsets[2 * numKeypoints * (outwidth * (maxRow) + maxCol) + keypoint]))) * shrinky;
#ifdef TESTPOSEINTERFACE
    printf ("Out %d %d %d %f --   %f %f\n", keypoint, maxCol, maxRow, maxVal, positionX, positionY);
    plot (data, width, height, positionX, positionY);    
#endif    
    
    results[keypoint * 3 + 0] = positionX / width;
    results[keypoint * 3 + 1] = positionY / height;
    results[keypoint * 3 + 2] = maxVal;
  }
  
#ifdef TESTPOSEINTERFACE
  writePPM (data, width, height);
#endif  
}

// Interface between Unity and native code.
int computePose (int texture, int w, int h, float * results)
{
  glEnable (GL_TEXTURE_2D);
  glBindTexture (GL_TEXTURE_2D, texture);
  unsigned char * dd = new unsigned char [w * h * 3];
  glGetTexImage (GL_TEXTURE_2D, 0, GL_RGB, GL_UNSIGNED_BYTE, dd);
  glBindTexture (GL_TEXTURE_2D, 0);
  glDisable (GL_TEXTURE_2D);

  callPose (dd, w, h, results);
  int r = 99;
  r = glGetError ();
  return r;
}

#ifdef TESTPOSEINTERFACE
int main (int argc, char * argv [])
{
  int width;
  int height;
  unsigned char * data = readPPMfile ("pose.ppm", &width, &height);
  printf ("Read %d %d\n", width, height);
  float results [17 * 3];
  initPose ();
  callPose (data, width, height, results);
  for (int i = 0; i < 17; i++)
  {
    printf ("Result %d: (%f, %f)\n", i, results[i * 2 + 0], results[i * 2 + 1]);
  }
}
#endif

