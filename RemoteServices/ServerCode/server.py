import socket
import time
import wave

serverPort = 8800
maxClients = 2
blockSize = 512

def server ():
  
  sock = socket.socket (socket.AF_INET, socket.SOCK_STREAM)
  sock.setsockopt (socket.SOL_SOCKET, socket.SO_REUSEADDR, 1)

  sock.bind (('', serverPort))

  sock.listen (maxClients)

  try:
      while True:
          clientSocket, address = sock.accept ()
          print ("Connection from: ", address)
          handleConnection (clientSocket, address)
  finally:
      sock.close(  )

def saveToWav (filename, data):
  fl = wave.open (filename, 'wb')
  fl.setnchannels(1) 
  fl.setsampwidth(2)
  fl.setframerate(16000)
  fl.writeframesraw (data)
  fl.close()

import whisper

speechModel = None
def invokeSpeechRecognition ():
  global speechModel
  
  if speechModel == None:
    speechModel = whisper.load_model("base")
  result = speechModel.transcribe("my.wav")
  print(result["text"])
  return result["text"]

def readAmount (clientSocket, amount):
  buf = bytearray ()
  while len (buf) < amount:
    readAmount = clientSocket.recv (amount - len (buf))
    buf += readAmount
    print ("Buf len: ", len (buf), amount - len (buf))
  return buf

def readInt (clientSocket):
  buf = readAmount (clientSocket, 4)
  print ("Read", buf, int.from_bytes (buf, byteorder='big', signed=False))
  return int.from_bytes (buf, byteorder='big', signed=False)

def handleConnection (clientSocket, address):    

  while True:
      size = readInt (clientSocket)
      receivedData = readAmount (clientSocket, size)
      print ("Received: ", len (receivedData))
      if not receivedData: 
        break
      
      saveToWav ("my.wav", receivedData)
      #time.sleep (5)
      
      result = str.encode (invokeSpeechRecognition ())
      
      clientSocket.send (result)
      print ("Sent reply")
  clientSocket.close(  )
  print ("Disconnected:", address)

server ()
#invokeSpeechRecognition ()
