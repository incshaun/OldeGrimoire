import socket

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

def handleConnection (clientSocket, address):    

  while True:
      receivedData = clientSocket.recv (blockSize)
      if not receivedData: 
        break
      clientSocket.send (receivedData)
  clientSocket.close(  )
  print ("Disconnected:", address)

server ()
