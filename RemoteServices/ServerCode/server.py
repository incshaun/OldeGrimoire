import socket
import time
import wave
import signal
import sys
import threading

serverPort = 8800
maxClients = 2
blockSize = 512

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
  #print(result["text"])
  return result["text"]

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
        size = readInt (clientSocket)
        receivedData = readAmount (clientSocket, size)
  #      print ("Received: ", len (receivedData))
        if not receivedData: 
          break
        
        saveToWav ("my.wav", receivedData)
        #time.sleep (5)
        
        result = str.encode (invokeSpeechRecognition ())
        
        writeInt (clientSocket, len (result))
        writeAmount (clientSocket, result)
        print ("Sent reply", result)
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
