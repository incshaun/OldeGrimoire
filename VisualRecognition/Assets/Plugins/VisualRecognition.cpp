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
    
    float confidenceThreshold = 0.2;
    printf ("Detnum %d %d %d %d %d %d %d %d\n", detections.size ().width, detections.size ().height, detections.channels (), detections.total (), detections.rows, detections.cols, detections.dims, detections.size [2]);
    for (int i = 0; i < detections.size[2]; i++)
    {
      int pos [4] = { 0, 0, 0, 0 };
      pos[2] = i;
      
      pos[3] = 1; float idx = detections.at <float> (pos);
      pos[3] = 2; float confidence = detections.at <float> (pos);
      pos[3] = 3; float sx = detections.at <float> (pos);
      pos[3] = 4; float sy = detections.at <float> (pos);
      pos[3] = 5; float ex = detections.at <float> (pos);
      pos[3] = 6; float ey = detections.at <float> (pos);
#ifdef ANDROID
    __android_log_print (ANDROID_LOG_ERROR, "Unity", "Det %d %f  %f %f %f %f\n", i, confidence, sx, sy, ex, ey);
#else
      printf ("Det %d %f  %f %f %f %f\n", i, confidence, sx, sy, ex, ey);
#endif
      
      if (confidence > confidenceThreshold)
      {
        std::string label = CLASSES[(int) idx] + " : " + std::to_string (confidence * 100);
        cv::rectangle(image, cv::Point_<float> (sx * w, sy * h), cv::Point_<float> (ex * w, ey * h), cv::Scalar (255, 0, 0), 2);
        cv:putText(image, label, cv::Point_<float> (sx * w, sy * h), cv::FONT_HERSHEY_SIMPLEX, 0.5, cv::Scalar (255, 0, 0), 2);
      }
    }
#ifdef ANDROID
#else
    cv::imshow("Output", image);
    cv::waitKey(0);
#endif    
    
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
