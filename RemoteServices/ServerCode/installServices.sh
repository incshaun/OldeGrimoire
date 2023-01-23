#! /bin/bash -l
# 
# Invoke with: source installServices.sh

conda deactivate
conda env remove --name remoteservices
rm -rf remoteservices

mkdir remoteservices
cd remoteservices

conda create --name remoteservices python=3.7 -y
conda activate remoteservices

# Add whisper
pip install -U openai-whisper


# Add tortoise-tts
git clone https://github.com/neonbjb/tortoise-tts.git
cd tortoise-tts
python -m pip install -r ./requirements.txt
python setup.py install

  mkdir models
  cd models
  wget https://huggingface.co/jbetker/tortoise-tts-v2/resolve/main/.models/autoregressive.pth
  wget https://huggingface.co/jbetker/tortoise-tts-v2/resolve/main/.models/classifier.pth
  wget https://huggingface.co/jbetker/tortoise-tts-v2/resolve/main/.models/clvp2.pth
  wget https://huggingface.co/jbetker/tortoise-tts-v2/resolve/main/.models/cvvp.pth
  wget https://huggingface.co/jbetker/tortoise-tts-v2/resolve/main/.models/diffusion_decoder.pth
  wget https://huggingface.co/jbetker/tortoise-tts-v2/resolve/main/.models/vocoder.pth
  wget https://huggingface.co/jbetker/tortoise-tts-v2/resolve/main/.models/rlg_auto.pth
  wget https://huggingface.co/jbetker/tortoise-tts-v2/resolve/main/.models/rlg_diffuser.pth
  cd ..

cd ..

# fix for inconsistent dependencies
pip -y uninstall ffmpeg-python
pip install ffmpeg-python

# Add BLIP
git clone https://github.com/salesforce/BLIP.git
cd BLIP
  pip install -r requirements.txt
  pip install pillow
  wget -c https://storage.googleapis.com/sfr-vision-language-research/BLIP/models/model_base_capfilt_large.pth
  wget -c https://storage.googleapis.com/sfr-vision-language-research/BLIP/models/model_base_vqa_capfilt_large.pth
  touch __init__.py
cd ..

cd ..

echo "You may need to manually install ffmpeg for your system for whisper"
