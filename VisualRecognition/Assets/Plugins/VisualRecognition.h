extern "C"
{
  // Load the recognition data sets, providing the name
  // of the directory they are located in.
  void prepareModel (char * dirname);

  // Perform the recognition step and return the number
  // of candidate matches identified.
  int doRecognise (char * imageData, int width, int height);
  
  // Return the parameters of the ith match from the last
  // doRecognise step.
  void retrieveMatch (int i, int & category, float & confidence, float & sx, float & sy, float & ex, float & ey);
}
