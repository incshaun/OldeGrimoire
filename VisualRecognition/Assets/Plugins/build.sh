ANDROID_NDK=~/android-sdks/ndk-bundle/
NDK_TARGET=24
ARCH="armeabi-v7a"
CONFIG="android-armeabi"
ARCH_PREFIX="arm-linux-androideabi-"
DST_PREFIX=$ARCH
COMPILER=4.9
TOOLCHAIN_PREFIX="arm-linux-androideabi-"
PLATFORM_PREFIX="arch-arm"
TMP_DIR=$ANDROID_NDK/toolchains/$TOOLCHAIN_PREFIX$COMPILER/prebuilt/
HOST_PLATFORM=$(ls $TMP_DIR)
export ANDROID_NDK_HOME=$ANDROID_NDK

ORG_PATH=$PATH
export CROSS_SYSROOT=$ANDROID_NDK/sysroot
export ANDROID_DEV=$ANDROID_NDK/platforms/android-$NDK_TARGET/$PLATFORM_PREFIX/usr
DST_DIR=$BUILD_DST/$DST_PREFIX
export ANDROID_NDK_HOME=${ANDROID_NDK}

export CROSS_COMPILE="armv7a-linux-androideabi24-clang"
export PATH=$ANDROID_NDK/toolchains/llvm/prebuilt/$HOST_PLATFORM/bin/:$ORG_PATH

${CROSS_COMPILE} -isystem $ANDROID_NDK/sysroot/usr/include/arm-linux-androideabi -shared -DANDROID -I$ANDROID_NDK/toolchains/llvm/prebuilt/linux-x86_64/sysroot/usr/include/c++/v1/ -L../../Standard\ Assets/OpenCV-android-sdk/sdk/native/libs/armeabi-v7a/ -fPIC -Wl,-soname,libVisualRecognition.so -o Android/libVisualRecognition.so VisualRecognition.cpp -I. -I../../Standard\ Assets/OpenCV-android-sdk/sdk/native/jni/include -Wl,--whole-archive -lopencv_java4 -Wl,--no-whole-archive  

