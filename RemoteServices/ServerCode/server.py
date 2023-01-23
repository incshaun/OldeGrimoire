import socket
import time
import wave
import signal
import sys
import threading

serverPort = 8800
maxClients = 2
blockSize = 512

SERVICETYPESPEECHRECOGNITION = 10
SERVICETYPESPEECHSYNTHESIS = 13
SERVICETYPESIMAGEIDENTIFICATION = 19

###########################################################################
### Service functions
###########################################################################

###########################################################################
# Speech Recognition
###########################################################################

import whisper

speechModel = None
def invokeSpeechRecognition (header, data):
  global speechModel
  
  if speechModel == None:
    speechModel = whisper.load_model("base")

  channels = int.from_bytes (header[0:4], byteorder='big', signed=False)
  samplewidth = int.from_bytes (header[4:8], byteorder='big', signed=False)
  frequency = int.from_bytes (header[8:12], byteorder='big', signed=False)

  #print ("Audio", channels, samplewidth, frequency)

  saveToWav ("my.wav", data, channels, samplewidth, frequency)
  result = speechModel.transcribe("my.wav")
  print ("Heard", result["text"])
  resultData = str.encode (result["text"])
  #print(result["text"])
  return b'', resultData

# Write to file, originally as a data transfer mechanism but also a useful debugging step
def saveToWav (filename, data, channels, samplewidth, frequency):
  fl = wave.open (filename, 'wb')
  fl.setnchannels(channels) 
  fl.setsampwidth(samplewidth)
  fl.setframerate(frequency)
  fl.writeframesraw (data)
  fl.close()

###########################################################################
# Speech Synthesis
###########################################################################

sys.path.append('remoteservices/tortoise-tts/tortoise/')

import torch
import torchaudio

from api import TextToSpeech, MODELS_DIR
from utils.audio import load_voices

speechSynthModel = None
def invokeSpeechSynthesis (header, data):
  global speechSynthModel
  
  if speechSynthModel == None:
    tts = TextToSpeech(models_dir="remoteservices/tortoise-tts/models")
    voice_samples, conditioning_latents = load_voices(["train_grace"], extra_voice_dirs=["remoteservices/tortoise-tts/tortoise/voices/"])
    speechSynthModel = (tts, voice_samples, conditioning_latents)

  words = data.decode("utf-8") 
  print (words)
  gen, dbg_state = speechSynthModel[0].tts_with_preset(words, k=1, voice_samples=speechSynthModel[1], conditioning_latents=speechSynthModel[2], preset='fast', use_deterministic_seed=259, return_deterministic_state=True, cvvp_amount=0.5)
  torchaudio.save("synth.wav", gen.squeeze(0).cpu(), 24000, encoding="PCM_S", bits_per_sample=16)
  
  fl = wave.open ("synth.wav", 'rb')
  data = fl.readframes (fl.getnframes ())
  header = b''
  header += fl.getnchannels ().to_bytes (4, byteorder='big', signed=False)
  header += fl.getsampwidth ().to_bytes (4, byteorder='big', signed=False)
  header += fl.getframerate ().to_bytes (4, byteorder='big', signed=False)
  print (fl.getnchannels (), fl.getsampwidth (), fl.getframerate ())
  fl.close()
  
  print ("TTS done")
  return header, data

###########################################################################
# Image Identification
###########################################################################

sys.path.insert(0, 'remoteservices/BLIP/')
from PIL import Image
from models.blip import blip_decoder
from models.blip_vqa import blip_vqa

from torchvision import transforms
from torchvision.transforms.functional import InterpolationMode

imageIdModel = None
def invokeImageIdentification (header, data):
  global imageIdModel
  
  image_size = 384
  device = torch.device('cuda' if torch.cuda.is_available() else 'cpu')
  if imageIdModel == None:
    modelsource = "remoteservices/BLIP/model_base_capfilt_large.pth"
    imageIdModel = blip_decoder(pretrained=modelsource, image_size=image_size, vit='base', med_config = 'remoteservices/BLIP/configs/med_config.json')
    imageIdModel.eval()
    imageIdModel = imageIdModel.to(device)

  width = int.from_bytes (header[0:4], byteorder='big', signed=False)
  height = int.from_bytes (header[4:8], byteorder='big', signed=False)
  b = bytes (data)
  rawimage = Image.frombytes ('RGB', (width, height), b)
  #print (width, height, len (data), width * height * 3, rawimage.width, rawimage.height)
  rawimage.save ("imid.png") # save, just for debugging purposes.
  transform = transforms.Compose([
        transforms.Resize((image_size,image_size),interpolation=InterpolationMode.BICUBIC),
        transforms.ToTensor(),
        transforms.Normalize((0.48145466, 0.4578275, 0.40821073), (0.26862954, 0.26130258, 0.27577711))
        ]) 
  image = transform(rawimage).unsqueeze(0).to(device)   
  
  with torch.no_grad():
    # beam search
    caption = imageIdModel.generate(image, sample=False, num_beams=3, max_length=20, min_length=5) 
    # nucleus sampling
    # caption = model.generate(image, sample=True, top_p=0.9, max_length=20, min_length=5) 
    print('caption: ', caption)
  
  ## Bonus - you can spin this off into a separate service, to respond to text questions about the image.
  #questionmodelsource = 'remoteservices/BLIP/model_base_vqa_capfilt_large.pth'
  #model = blip_vqa(pretrained=questionmodelsource, image_size=image_size, vit='base', med_config = 'remoteservices/BLIP/configs/med_config.json')
  #model.eval()
  #model = model.to(device)
      
  #question = 'what type of clothing is the person wearing?'

  #with torch.no_grad():
      #answer = model(image, question, train=False, inference='generate') 
      #print('answer: '+answer[0]) 
  
  return b'', str.encode (caption[0])
  
###########################################################################

services = {
  SERVICETYPESPEECHRECOGNITION : invokeSpeechRecognition,
  SERVICETYPESPEECHSYNTHESIS : invokeSpeechSynthesis,
  SERVICETYPESIMAGEIDENTIFICATION : invokeImageIdentification,
}

activeSockets = []

def signal_handler(signal, frame):
  for sock in activeSockets:
    sock.close ()
  print ("Socket closed ", activeSockets)
  sys.exit (0)

def server ():
  sock = socket.socket (socket.AF_INET, socket.SOCK_STREAM)
  sock.setsockopt (socket.SOL_SOCKET, socket.SO_REUSEADDR, 1)

  sock.bind (('', serverPort))

  sock.listen (maxClients)
  
  activeSockets.append (sock)
  signal.signal(signal.SIGINT, signal_handler)
  
  print ("Server started")
  
  try:
      while True:
          clientSocket, address = sock.accept ()
          print ("Connection from: ", address)
          activeSockets.append (clientSocket)
          
          serviceThread = threading.Thread (target = handleConnection, args = (clientSocket, address), daemon = True)
          serviceThread.start ()
          #handleConnection (clientSocket, address)
  finally:
      sock.close(  )


def readAmount (clientSocket, amount):
  buf = bytearray ()
  while len (buf) < amount:
    try:
      readAmount = clientSocket.recv (amount - len (buf))
      if not readAmount: # socket probably closed.
        raise IOError
      
      buf += readAmount
    except Exception as e:
      print ("Error reading on socket")
      raise e
  #print ("Buf len: ", len (buf), amount - len (buf))
  return buf

def writeAmount (clientSocket, data):
  clientSocket.send (data)

def readInt (clientSocket):
  try:
    buf = readAmount (clientSocket, 4)
  #  print ("Read", buf, int.from_bytes (buf, byteorder='big', signed=False))
    return int.from_bytes (buf, byteorder='big', signed=False)
  except Exception as e:
    raise e

def writeInt (clientSocket, v):
  clientSocket.send (v.to_bytes (4, byteorder='big', signed=False))

def handleConnection (clientSocket, address):    

  try:
    while True:
        service = readInt (clientSocket)
        size = readInt (clientSocket)
        receivedHeader = readAmount (clientSocket, size)
        size = readInt (clientSocket)
        receivedData = readAmount (clientSocket, size)
        print ("Received: ", service, len (receivedHeader), len (receivedData))
        if not receivedData: 
          break

        if service in services.keys ():
          resultHeader, resultData = services[service] (receivedHeader, receivedData)
        else:
          print ("Unknown service: ", service)
          resultHeader = b''
          resultData = b''
        #saveToWav ("my.wav", receivedData)
        
        #result = str.encode (invokeSpeechRecognition ())
        #text = receivedData.decode("utf-8") 
        #result = invokeSpeechSynthesis (text)
        
        writeInt (clientSocket, len (resultHeader))
        writeAmount (clientSocket, resultHeader)
        writeInt (clientSocket, len (resultData))
        writeAmount (clientSocket, resultData)
        print ("Sent reply")
    clientSocket.close(  )
    print ("Disconnected:", address, len (activeSockets))
    activeSockets.remove (clientSocket)
  except Exception as e:
    activeSockets.remove (clientSocket)
    clientSocket.close(  )
    print ("Service exception: ", str (e))
    return

server ()
#invokeSpeechRecognition ()
#invokeSpeechSynthesis ()
