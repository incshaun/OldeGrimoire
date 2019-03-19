# Simple connection service for webRTC
# Requires python 3.

# Import the support libraries required.
# Concurrency support, to avoid delays handling network messages.
import asyncio
# Encryption for network stream required by browsers.
import ssl
# A network library that interacts with similar libraries supported in browsers.
import websockets

# Keep a list of the clients that have connected so far.
clients = []

# When a new connection comes in, add it to the list of clients.
async def register(websocket):
  if not websocket in clients:
    clients.append (websocket)
    
# Handle communication with one client.    
async def handler (websocket, path):
  print ("Received client.")
  await register(websocket)
  
  while True:
    # Wait for incoming message.
    message = await websocket.recv()
    
    print ("Receieved message", message[:20], "... from client", (1+clients.index (websocket)), "of", len (clients))
    
    # Copy that message to all other clients.
    for client in clients:
      if client != websocket:
        try:
          await client.send (message)
        except Exception as e:
          pass
          # Since we don't remove closed connections, some errors are expected.
          # print ("Problem sending", e)
  
# Start up the server.
  
# create certificate with: openssl req -new -newkey rsa:4096 -nodes -x509 -keyout key.pem -out cert.pem -days 365
#
# Browsers will have issues accessing the site because of the self-signed certificate
# Visit the address: https://[fill in server address here]:5000/
# It should complain about the certificate and allow it to be accepted (under "Advanced"). 
ssl_context = ssl.SSLContext(ssl.PROTOCOL_TLS_SERVER)
ssl_context.load_cert_chain (certfile = 'cert.pem', keyfile = "key.pem")
server = websockets.server.serve (handler, '0.0.0.0', 5000, ssl=ssl_context)

asyncio.get_event_loop().run_until_complete (server)
asyncio.get_event_loop().run_forever()
