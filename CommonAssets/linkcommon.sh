#! /bin/sh

if [ "${PWD##*/}" = "Assets" ]; then
  echo "Linking assets in Textures folder"
  (
  name=`(cd ..; echo "${PWD##*/}") | sed 's/\([^[:blank:]]\)\([[:upper:]]\)/\1\n\2/g'` 
  echo $name
  mkdir Textures
  cd Textures
  ln -s ../../../CommonAssets/iconbackground.png .
  convert ../../../CommonAssets/iconforeground.png -background red -fill white -pointsize 52 -gravity Center -annotate 0 "$name" -fill black -pointsize 50 -gravity Center -annotate 0 "$name" iconforeground.png 
  convert ../../../CommonAssets/icon.png -fill white -pointsize 52 -gravity Center -annotate 0 "$name" -fill black -pointsize 50 -gravity Center -annotate 0 "$name" icon.png 
  )
  echo "Linking assets for skybox"
  (
  mkdir Materials
  cd Materials
  ln -s ../../../CommonAssets/OGSkybox.mat .
  )
  (
  mkdir Shaders
  cd Shaders
  ln -s ../../../CommonAssets/OldeGrimSkybox.shader .
  )
else
  echo "This script is intended to run in the assets folder of a project, to link common assets used across multiple projects"
  exit
fi

