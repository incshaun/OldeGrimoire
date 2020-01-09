#! /bin/sh

if [ ! -d "tensorflow" ]; then
  echo "Cloning Tensorflow Lite repository"
  (
    svn export https://github.com/tensorflow/tensorflow.git/trunk/tensorflow
  )
fi

if [ ! -f "tensorflow/lite/tools/make/downloads/flatbuffers/include/flatbuffers/flatbuffers.h" ]; then
  echo "Download Tensorflow Lite dependencies"
  (
    ./tensorflow/lite/tools/make/download_dependencies.sh 
    
    # Unneeded tool, that has its own main.
    rm ./tensorflow/lite/experimental/ruy/tune_tool.cc
    
    # Also doesn't seem to be used, and requires the fft library.
    rm ./tensorflow/lite/kernels/rfft2d.cc

    # adsl just gets used for one function, and causes link issues.
    rm -r tensorflow/lite/tools/make/downloads/absl
    cat tensorflow/lite/kernels/lstm.cc | sed -z 's/#include "absl\/memory\/memory.h"//g; s/*output = absl::make_unique<int32_t\[\]>(row)\;/*output = std::unique_ptr<int32_t\[\]> (new int32_t[row])\;/g'> tensorflow/lite/kernels/lstm.ccc; mv tensorflow/lite/kernels/lstm.ccc tensorflow/lite/kernels/lstm.cc
    
    # fix error in makefile
    cat tensorflow/lite/tools/make/Makefile | sed -z 's/$(wildcard tensorflow\/lite\/experimental\/resource_variable\/\*.cc)/$(wildcard tensorflow\/lite\/experimental\/resource\/\*.cc)/g'> tensorflow/lite/tools/make/Makefile.a; mv tensorflow/lite/tools/make/Makefile.a tensorflow/lite/tools/make/Makefile
    
  )
fi

# Cleanup some untidy dependencies on third party elements.

if [ ! -d "third_party/fft2d" ]; then
  echo "Retrieving FFT2D"
  (
    mkdir -p third_party
    mkdir -p third_party/fft2d
    cd third_party/fft2d
    # Doesn't seem to actually use this file.
    rm -f fft.h
    echo "#ifndef FFT2D_FFT_H__" >> fft.h
    echo "#define FFT2D_FFT_H__" >> fft.h

    echo "#ifdef __cplusplus" >> fft.h
    echo "extern \"C\" {" >> fft.h
    echo "#endif" >> fft.h

    echo "inline void rdft(int, int, double *, int *, double *) {}" >> fft.h

    echo "#ifdef __cplusplus" >> fft.h
    echo "}" >> fft.h
    echo "#endif" >> fft.h

    echo "#endif  // FFT2D_FFT_H__" >> fft.h

    echo "extern void rdft2d(int, int, int, double **, double *, int *, double *);" > fft2d.h
  )
fi

rm -f tensorflow/lite/tools/make/gen/linux_x86_64/lib/libtensorflow-lite.a
if [ ! -f "tensorflow/lite/tools/make/gen/linux_x86_64/lib/libtensorflow-lite.a" ]; then
  echo "Building Tensorflow Lite for x86_64 platform"
  (
    mkdir -p third_party
    mkdir -p third_party/eigen3
    ln -s `pwd`/tensorflow/lite/tools/make/downloads/eigen/Eigen third_party/eigen3/
    ln -s `pwd`/tensorflow/lite/tools/make/downloads/eigen/unsupported third_party/eigen3/
    
    
    make -j 4 BUILD_WITH_NNAPI=false -C `pwd` -f tensorflow/lite/tools/make/Makefile lib
    
    rm -f Assets/Plugins/x86_64/libposeinterface.so
  )
fi

if [ -f "Assets/Scripts/poseinterface.cc" ]; then
  if [ ! -f "poseinterfacetest" ]; then
    echo "Building test application. Note this needs an image in pose.ppm, and the posenet_mobilenet_v1_100_257x257_multi_kpt_stripped.tflite model file to run."
    (
      g++ -DTESTPOSEINTERFACE Assets/Scripts/poseinterface.cc -o poseinterfacetest tensorflow/lite/tools/make/gen/linux_x86_64/lib/libtensorflow-lite.a -I . -I tensorflow/lite/tools/make/downloads/flatbuffers/include/ -lpthread -lGL
    )
  fi

  rm -f Assets/Plugins/x86_64/libposeinterface.so
  if [ ! -f "Assets/Plugins/x86_64/libposeinterface.so" ]; then
    echo "Building unity interface library, x86_64 version."
    (
      mkdir -p Assets
      mkdir -p Assets/Plugins/
      mkdir -p Assets/Plugins/x86_64
      g++ -fPIC Assets/Scripts/poseinterface.cc -o Assets/Plugins/x86_64/libposeinterface.so -shared -Wl,--whole-archive  tensorflow/lite/tools/make/gen/linux_x86_64/lib/libtensorflow-lite.a -I . -I tensorflow/lite/tools/make/downloads/flatbuffers/include/  -lpthread -Wl,--no-whole-archive
    )
  fi
  
fi
