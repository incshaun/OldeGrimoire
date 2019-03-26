#include "VisualRecognition.h"
#include <opencv2/opencv.hpp>
#ifdef ANDROID
#include <android/log.h>
#endif

extern "C"
{
  // The recognition model, created during prepareModel.
  cv::dnn::Net net;
  
  // The details of detected matches from the last round
  // of doRecognise.
  cv::Mat detections;
  
  void prepareModel (char * dirname)
  {
    const char * protoFile = "MobileNetSSD_deploy.prototxt.txt";
    const char * modelFile = "MobileNetSSD_deploy.caffemodel";
    #ifdef ANDROID
    __android_log_print (ANDROID_LOG_ERROR, "Unity", "Unity Opening file: %s %s\n", protoFile, dirname);
    #endif     
    net = cv::dnn::readNetFromCaffe (std::string (dirname) + std::string ("/") + std::string (protoFile), std::string (dirname) + std::string ("/") + std::string (modelFile));
    
    std::vector<cv::String> n = net.getLayerNames ();
    for (cv::String s : n)
    {
      #ifdef ANDROID
      __android_log_print (ANDROID_LOG_ERROR, "Unity", "Layer %s\n", s.c_str ());
      #else
      printf ("Layer %s\n", s.c_str ());
      #endif     
    }
  }
  
  int doRecognise (char * imageData, int width, int height)
  {
    std::string CLASSES [] = {"background", "aeroplane", "bicycle", "bird", "boat",
      "bottle", "bus", "car", "cat", "chair", "cow", "diningtable",
      "dog", "horse", "motorbike", "person", "pottedplant", "sheep",
      "sofa", "train", "tvmonitor" };
      
      #ifdef ANDROID
      __android_log_print (ANDROID_LOG_ERROR, "Unity", "Image data %p %d %d\n", imageData, width, height);
      #endif    
      //     const char * imageFile = "example_01.jpg";
      
      //     cv::Mat image = cv::imread(std::string (dirname) + std::string ("/") + std::string (imageFile));
      //     cv::Mat image = cv::imread(imageFile);
      cv::Mat rawimage = cv::Mat (height, width, CV_8UC4, imageData);
      cv::Mat image;
      cv::cvtColor (rawimage, image, cv::COLOR_RGBA2BGR, 3);
      cv::flip (image, image, 0);
      int h = image.rows;
      int w = image.cols;
      //     imwrite ("/storage/emulated/0/Android/data/visualrecognition.olde.grim/files/test2.jpg", image);
      #ifdef ANDROID
      __android_log_print (ANDROID_LOG_ERROR, "Unity", "Image %d %d\n", h, w);
      #else
      printf ("Image %d %d\n", h, w);
      #endif    
      cv::Mat resized;
      cv::resize (image, resized, cv::Size(300, 300));
      #ifdef ANDROID
      __android_log_print (ANDROID_LOG_ERROR, "Unity", "Image2 %d %d\n", image.rows, image.cols);
      #else    
      printf ("Image2 %d %d\n", image.rows, image.cols);
      #endif    
      cv::Mat blob = cv::dnn::blobFromImage (resized, 0.007843, cv::Size(300, 300), cv::Scalar(127.5));
      #ifdef ANDROID
      __android_log_print (ANDROID_LOG_ERROR, "Unity", "Blob %d %d %d\n", blob.size ().height, blob.size ().width, blob.total ());
      #else
      printf ("Blob %d %d %d\n", blob.size ().height, blob.size ().width, blob.total ());
      #endif
      
      net.setInput(blob);
      detections = net.forward();
      
      return detections.size[2];
  }
  
  void retrieveMatch (int i, int & category, float & confidence, float & sx, float & sy, float & ex, float & ey)
  {
    int pos [4] = { 0, 0, 0, 0 };
    pos[2] = i;
    
    pos[3] = 1; category = detections.at <float> (pos);
    pos[3] = 2; confidence = detections.at <float> (pos);
    pos[3] = 3; sx = detections.at <float> (pos);
    pos[3] = 4; sy = detections.at <float> (pos);
    pos[3] = 5; ex = detections.at <float> (pos);
    pos[3] = 6; ey = detections.at <float> (pos);    
  }
}
