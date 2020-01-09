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
    rm -f ./tensorflow/lite/experimental/ruy/tune_tool.cc
    
    # Also doesn't seem to be used, and requires the fft library.
    rm -f ./tensorflow/lite/kernels/rfft2d.cc

    # adsl just gets used for one function, and causes link issues.
    rm -rf tensorflow/lite/tools/make/downloads/absl
    cat tensorflow/lite/kernels/lstm.cc | sed -z 's/#include "absl\/memory\/memory.h"//g; s/*output = absl::make_unique<int32_t\[\]>(row)\;/*output = std::unique_ptr<int32_t\[\]> (new int32_t[row])\;/g'> tensorflow/lite/kernels/lstm.ccc; mv tensorflow/lite/kernels/lstm.ccc tensorflow/lite/kernels/lstm.cc
    
    # fix error in makefile
    cat tensorflow/lite/tools/make/Makefile | sed -z 's/$(wildcard tensorflow\/lite\/experimental\/resource_variable\/\*.cc)/$(wildcard tensorflow\/lite\/experimental\/resource\/\*.cc)/g'> tensorflow/lite/tools/make/Makefile.a; mv tensorflow/lite/tools/make/Makefile.a tensorflow/lite/tools/make/Makefile

    # missing functions, apparently related to neon and eigen.
    cat tensorflow/lite/tools/make/downloads/eigen/Eigen/src/Core/arch/NEON/Complex.h | sed -z 's/Packet2d eq_swapped = vreinterpretq_f64_u32(vrev64q_u32(vreinterpretq_u32_f64(eq)))/Packet2d eq_swapped = (float64x2_t)(vrev64q_u32((uint32x4_t)(eq)))/g'> tensorflow/lite/tools/make/downloads/eigen/Eigen/src/Core/arch/NEON/Complex.h.a; mv tensorflow/lite/tools/make/downloads/eigen/Eigen/src/Core/arch/NEON/Complex.h.a tensorflow/lite/tools/make/downloads/eigen/Eigen/src/Core/arch/NEON/Complex.h
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

# Force some degree of rebuilding each invocation, to avoid having to check dependencies.
rm -f tensorflow/lite/tools/make/gen/aarch64_armv8-a/lib/libtensorflow-lite.a
rm -f Assets/Plugins/Android/libs/arm64-v8a/libposeinterface.so
rm -f Assets/Plugins/x86_64/libposeinterface.so
rm -f tensorflow/lite/tools/make/gen/linux_x86_64/lib/libtensorflow-lite.a

if [ ! -f "tensorflow/lite/tools/make/gen/linux_x86_64/lib/libtensorflow-lite.a" ]; then
  echo "Building Tensorflow Lite for x86_64 platform"
  (
    mkdir -p third_party
    mkdir -p third_party/eigen3
    ln -s `pwd`/tensorflow/lite/tools/make/downloads/eigen/Eigen third_party/eigen3/
    ln -s `pwd`/tensorflow/lite/tools/make/downloads/eigen/unsupported third_party/eigen3/
    
    
    make -j 4 BUILD_WITH_NNAPI=false -C `pwd` -f tensorflow/lite/tools/make/Makefile lib
  )
fi

if [ ! -f " a" ]; then
  echo "Building Tensorflow Lite for ARM64 platform"
  (
    mkdir -p third_party
    mkdir -p third_party/eigen3
    ln -s `pwd`/tensorflow/lite/tools/make/downloads/eigen/Eigen third_party/eigen3/
    ln -s `pwd`/tensorflow/lite/tools/make/downloads/eigen/unsupported third_party/eigen3/
    
    ANDROID_NDK=~/android-sdks/ndk/16.1.4479499/
    
    make TARGET=aarch64 TARGET_TOOLCHAIN_PREFIX=$ANDROID_NDK/toolchains/aarch64-linux-android-4.9/prebuilt/linux-x86_64/bin/aarch64-linux-android- EXTRA_CXXFLAGS+="-D__ANDROID_API__=26 -DANDROID --sysroot=$ANDROID_NDK/sysroot -isystem $ANDROID_NDK/sysroot/usr/include/aarch64-linux-android  -isystem $ANDROID_NDK/sources/cxx-stl/gnu-libstdc++/4.9/include -isystem $ANDROID_NDK/sources/cxx-stl/gnu-libstdc++/4.9/libs/arm64-v8a/include -isystem $ANDROID_NDK/sources/cxx-stl/gnu-libstdc++/4.9/include/backward -isystem $ANDROID_NDK/sysroot/usr/include -isystem $ANDROID_NDK/sysroot/usr/include/aarch64-linux-android -D_GLIBCXX_USE_C99" EXTRA_CFLAGS="-D__ANDROID_API__=26 -DANDROID --sysroot=$ANDROID_NDK/sysroot -isystem $ANDROID_NDK/sysroot/usr/include/aarch64-linux-android  -isystem $ANDROID_NDK/sources/cxx-stl/gnu-libstdc++/4.9/include -isystem $ANDROID_NDK/sources/cxx-stl/gnu-libstdc++/4.9/libs/arm64-v8a/include -isystem $ANDROID_NDK/sources/cxx-stl/gnu-libstdc++/4.9/include/backward -isystem $ANDROID_NDK/sysroot/usr/include -isystem $ANDROID_NDK/sysroot/usr/include/aarch64-linux-android -std=c11" -C `pwd` -f tensorflow/lite/tools/make/Makefile lib
  )
fi

if [ -f "Assets/Scripts/poseinterface.cc" ]; then
  if [ ! -f "poseinterfacetest" ]; then
    echo "Building test application. Note this needs an image in pose.ppm, and the posenet_mobilenet_v1_100_257x257_multi_kpt_stripped.tflite model file to run."
    (
      g++ -DTESTPOSEINTERFACE Assets/Scripts/poseinterface.cc -o poseinterfacetest tensorflow/lite/tools/make/gen/linux_x86_64/lib/libtensorflow-lite.a -I . -I tensorflow/lite/tools/make/downloads/flatbuffers/include/ -lpthread -lGL
    )
  fi

  if [ ! -f "Assets/Plugins/x86_64/libposeinterface.so" ]; then
    echo "Building unity interface library, x86_64 version."
    (
      mkdir -p Assets
      mkdir -p Assets/Plugins/
      mkdir -p Assets/Plugins/x86_64
      g++ -fPIC Assets/Scripts/poseinterface.cc -o Assets/Plugins/x86_64/libposeinterface.so -shared -Wl,--whole-archive  tensorflow/lite/tools/make/gen/linux_x86_64/lib/libtensorflow-lite.a -I . -I tensorflow/lite/tools/make/downloads/flatbuffers/include/  -lpthread -Wl,--no-whole-archive
    )
  fi
  
  if [ ! -f "Assets/Plugins/Android/libs/arm64-v8a/libposeinterface.so" ]; then
    echo "Building unity interface library, ARM64 version."
    (
      mkdir -p Assets
      mkdir -p Assets/Plugins/
      mkdir -p Assets/Plugins/Android/
      mkdir -p Assets/Plugins/Android/libs/
      mkdir -p Assets/Plugins/Android/libs/arm64-v8a
      
      ANDROID_NDK=~/android-sdks/ndk/16.1.4479499/
      
      $ANDROID_NDK/toolchains/aarch64-linux-android-4.9/prebuilt/linux-x86_64/bin/aarch64-linux-android-g++ -D__ANDROID_API__=26 --sysroot=$ANDROID_NDK/sysroot -isystem $ANDROID_NDK/sysroot/usr/include/aarch64-linux-android   -DANDROID -rdynamic -shared -fPIC Assets/Scripts/poseinterface.cc -c -I . -I tensorflow/lite/tools/make/downloads/flatbuffers/include/ -std=c++14 -isystem $ANDROID_NDK/sources/cxx-stl/gnu-libstdc++/4.9/include -isystem $ANDROID_NDK/sources/cxx-stl/gnu-libstdc++/4.9/libs/arm64-v8a/include -isystem $ANDROID_NDK/sources/cxx-stl/gnu-libstdc++/4.9/include/backward -isystem $ANDROID_NDK/sysroot/usr/include -isystem $ANDROID_NDK/sysroot/usr/include/aarch64-linux-android
      
      $ANDROID_NDK/toolchains/aarch64-linux-android-4.9/prebuilt/linux-x86_64/bin/aarch64-linux-android-g++ -D__ANDROID_API__=26 --sysroot=$ANDROID_NDK/platforms/android-26/arch-arm64/ -isystem $ANDROID_NDK/sysroot/usr/include/aarch64-linux-android -std=c++14  -DANDROID -shared -fPIC -oAssets/Plugins/Android/libs/arm64-v8a/libposeinterface.so -Wl,--whole-archive,-soname,libposeinterface.so poseinterface.o tensorflow/lite/tools/make/gen/aarch64_armv8-a/lib/libtensorflow-lite.a $ANDROID_NDK/sources/cxx-stl/gnu-libstdc++/4.9/libs/arm64-v8a/libgnustl_static.a $ANDROID_NDK/platforms/android-26/arch-arm64/usr/lib/libz.a -lGLESv3 -Wl,--no-whole-archive
    )
  fi
  
fi


