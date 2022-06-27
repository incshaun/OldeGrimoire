using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using System;

public class SharedLocationServer
{
  // The master list of all objects maintained by the server.
  private List<LocationData> serverDatabase = new List<LocationData>();
  // Since the server runs on multiple threads, all access to the database must be locked by a mutex before use.
  private Mutex serverDatabaseMutex = new Mutex();
  
  // The list of all connections that are operational, so that properties of an individual connection can be identifed.
  private List<ConnectionInfo> connections = new List<ConnectionInfo>();
  // The list of connections is shared between threads, so must be locked by a mutex before use.
  private Mutex connectionsMutex = new Mutex();
  
  // Details an individual connection from a client.
  private class ConnectionInfo
  {
    // The connected socket to a client.
    public TcpClient socket;
    // A mutex to ensure only one thread is reading/writing a socket at any time.
    // This ensures that messages from different processes don't get intermingled.
    public Mutex sendMutex = new Mutex();
    public Mutex receiveMutex = new Mutex();
    // List of identifiers for objects associated with this connection.
    public List<int> myObjects = new List<int>();
  }
  
  // https://stackoverflow.com/questions/6803073/get-local-ip-address
  public static string getLocalIPAddress()
  {
    var host = Dns.GetHostEntry(Dns.GetHostName());
    foreach (var ip in host.AddressList)
    {
      if (ip.AddressFamily == AddressFamily.InterNetwork)
      {
        return ip.ToString();
      }
    }
    throw new Exception("No network adapters with an IPv4 address in the system!");
  }
  
  private void removeConnection(ConnectionInfo ci)
  {
    foreach (int id in ci.myObjects)
    {
      SharedLocation.markObject(serverDatabase, serverDatabaseMutex, id, LocationType.Defunct);
    }
    connectionsMutex.WaitOne();
    connections.Remove(ci);
    connectionsMutex.ReleaseMutex();
  }
  
  // Send a complete copy of the server database to all of the clients.
  // This is not particularly efficient and could be improved by sending
  // updates only where they are required (such as when an object changes
  // location).
  private void sendUpdatesToAllClients()
  {
    // Make a copy of the connections list so we don't
    // have to lock it for an extended period.
    List<ConnectionInfo> connectionsCopy;
    
    connectionsMutex.WaitOne();
    connectionsCopy = new List<ConnectionInfo>(connections);
    connectionsMutex.ReleaseMutex();
    
    foreach (ConnectionInfo ci in connectionsCopy)
    {
      // Send a copy of the database to this client.
      serverDatabaseMutex.WaitOne();
      
      try
      {
        foreach (LocationData ld in serverDatabase)
        {
          ld.status = MessageStatus.Update;
          
          SharedLocation.sendMessage(ld, ci.socket.GetStream(), ci.sendMutex);
        }
      }
      catch (Exception e)
      {
        Debug.Log("Server update Failed : " + e + " " + e.Message);
        
      }
      
      serverDatabaseMutex.ReleaseMutex();
    }
  }
  
  // Handle all interactions with a single client. This function
  // runs on its own thread to avoid holding up the rest of the application.
  // It responds to messages from the client, and sends updates.
  private void handleServerMessages(ConnectionInfo ci)
  {
    NetworkStream stream = ci.socket.GetStream();
    while (true)
    {
      try
      {
        LocationData ld = SharedLocation.receiveMessage(stream, ci.receiveMutex);
        // add to database.
        SharedLocation.addLocation(serverDatabase, serverDatabaseMutex, ld);
        if (!ci.myObjects.Contains(ld.identifier))
        {
          ci.myObjects.Add(ld.identifier);
        }
        
        if (ld.locType == LocationType.Participant)
        {
          // reply back to client.
          ld.status = MessageStatus.IdSet;
          SharedLocation.sendMessage(ld, stream, ci.sendMutex);
        }
        
        sendUpdatesToAllClients();
        
      }
      catch (Exception e)
      {
        Debug.Log("Communication Failed : " + e + " " + e.Message);
        stream.Close();
        // remove connection.
        removeConnection(ci);
        
        break;
      }
    }
  }
  
  // The activity of the server, run in its own thread.
  // Listens for connections, and then creates an
  // additional thread to handle communications on each
  // new connection.
  private void server()
  {
    TcpListener server = null;
    
    try
    {
      server = new TcpListener(IPAddress.Any, SharedLocation.port);
      // Report server in case anyone wants to see its address.
      SharedLocation.serverName = getLocalIPAddress();
      
      server.Start();
      
      while (true)
      {
        // Wait for a new client. This will block until
        // a client connects.
        TcpClient client = server.AcceptTcpClient();
        
        // Create a record of the new connection received.
        ConnectionInfo ci = new ConnectionInfo();
        ci.socket = client;
        
        // Add it to a list of connections. This is also
        // manipulated in other threads so must be locked
        // before being changed.
        connectionsMutex.WaitOne();
        connections.Add(ci);
        connectionsMutex.ReleaseMutex();
        
        // Create a thread for this client to handle client
        // specific communications.
        Thread t = new Thread(() => handleServerMessages(ci));
        t.Start();
      }
    }
    catch (Exception e)
    {
      Debug.Log("Server Exception: " + e.Message);
    }
    
    server.Stop();
  }
  
  public SharedLocationServer()
  {
    Thread t = new Thread(server);
    t.Start();
  }
}
