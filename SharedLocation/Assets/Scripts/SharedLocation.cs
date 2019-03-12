using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Net;
using System.Threading;
using System.IO;
using System.Net.Sockets;
using System;
using System.Runtime.Serialization.Formatters.Binary;

public class SharedLocation : MonoBehaviour
{
  public Text serverAddress;
  
  private Int32 port = 8080;
  
  public GameObject pieceTemplate;
  public GameObject boardState;
  
  public enum MessageStatus { Add, IdSet, Update };
  public enum LocationType { Participant, Waypoint, Defunct };
  
  [Serializable]
  public class LocationData
  {
    public MessageStatus status;
    public LocationType locType;
    public int identifier;
    public float latitude;
    public float longitude;
  }
  
  private List <LocationData> serverDatabase = new List <LocationData> ();
  private Mutex serverDatabaseMutex = new Mutex ();

  private List <LocationData> clientDatabase = new List <LocationData> ();
  private Mutex clientDatabaseMutex = new Mutex ();

  private bool updateRequired = false;
  
  private int id = 99;
  
  private int getUniqueID ()
  {
    id += 1;
    return id;
  }
  
  private void addLocation (List <LocationData> list, Mutex mutex, LocationData ld)
  {
    mutex.WaitOne ();
    bool found = false;
    if (ld.identifier < 0)
    {
      ld.identifier = getUniqueID ();
    }
    for (int i = 0; i < list.Count; i++)
    {
      if (list[i].identifier == ld.identifier)
      {
        list[i] = ld;
        found = true;
        break;
      }
    }
    if (!found)
    {
      list.Add (ld);
    }
    mutex.ReleaseMutex ();
  }
  
  private class ConnectionInfo
  {
    public TcpClient socket;
    public Mutex connectionMutex = new Mutex ();
  }
  
  private void sendUpdatesToAllClients ()
  {
    List <ConnectionInfo> connectionsCopy;
    
    connectionsMutex.WaitOne ();
    connectionsCopy = new List <ConnectionInfo> (connections);
    connectionsMutex.ReleaseMutex ();

    foreach (ConnectionInfo ci in connectionsCopy)
    {
      serverDatabaseMutex.WaitOne ();
      
      foreach (LocationData ld in serverDatabase)
      {
        ld.status = MessageStatus.Update;
        MemoryStream ms = new MemoryStream ();
        new BinaryFormatter ().Serialize (ms, ld);
        byte [] serializedData = ms.ToArray ();
          
        ci.connectionMutex.WaitOne ();
        // Send the message to the connected TcpServer. 
        ci.socket.GetStream ().Write (serializedData, 0, serializedData.Length);
        ci.connectionMutex.ReleaseMutex ();
      }
      
      serverDatabaseMutex.ReleaseMutex ();
    }
  }
  
  private void handleServerMessages (ConnectionInfo ci)
  {
    NetworkStream stream = ci.socket.GetStream ();
    while (true)
    {
      try 
      {
        BinaryFormatter formatter = new BinaryFormatter();
        LocationData ld = (LocationData) formatter.Deserialize (stream);
        Debug.Log ("Received " + ld);
        // add to database.
        addLocation (serverDatabase, serverDatabaseMutex, ld);
        
        if (ld.locType == LocationType.Participant)
        {
          // reply back to client.
          ld.status = MessageStatus.IdSet;
          MemoryStream ms = new MemoryStream ();
          new BinaryFormatter ().Serialize (ms, ld);
          byte [] serializedData = ms.ToArray ();
          
          ci.connectionMutex.WaitOne ();
          // Send the message to the connected TcpServer. 
          stream.Write (serializedData, 0, serializedData.Length);
          ci.connectionMutex.ReleaseMutex ();
        }
        
        sendUpdatesToAllClients ();
        
      }
      catch (Exception e)
      {
        Debug.Log ("Communication Failed : " + e + " " + e.Message);
        stream.Close();
        // remove connection.
        connectionsMutex.WaitOne ();
        connections.Remove (ci);
        connectionsMutex.ReleaseMutex ();
        
        break;
      }
    }
  }
  
  private List <ConnectionInfo> connections = new List <ConnectionInfo> ();
  private Mutex connectionsMutex = new Mutex ();
  
  private string serverName = null;
  
  private void server ()
  {
    TcpListener server = null;   
    
    try
    {
      server = new TcpListener (IPAddress.Any, port);
      serverName = ((IPEndPoint) server.LocalEndpoint).Address.ToString();
      
      // Start listening for client requests.
      server.Start();
      
      // Buffer for reading data
      Byte[] bytes = new Byte[256];
      String data = null;
      
      // Enter the listening loop.
      while (true) 
      {
        TcpClient client = server.AcceptTcpClient ();            
        
        ConnectionInfo ci = new ConnectionInfo ();
        ci.socket = client;
        
        connectionsMutex.WaitOne ();
        connections.Add (ci);
        connectionsMutex.ReleaseMutex ();
        
        Thread t = new Thread (() => handleServerMessages (ci));
        t.Start ();
        
        Debug.Log ("Got client");
      }
    }
    catch (Exception e)
    {
      Debug.Log ("Server Exception: " + e.Message);
    }
    
    server.Stop();
  }

  private TcpClient clientSocket = null;
  private int clientIdentifier = -1;
  
  public void handleClient (TcpClient clientSocket)
  {
    NetworkStream stream = clientSocket.GetStream ();

    while (true)
    {
      try 
      {
        BinaryFormatter formatter = new BinaryFormatter();
        LocationData ld = (LocationData) formatter.Deserialize (stream);
        Debug.Log ("Client Received " + ld);
        // add to database.
        //addLocation (serverDatabase, serverDatabaseMutex, ld);

      if (ld.status == MessageStatus.IdSet)
      {
        clientIdentifier = ld.identifier;
        Debug.Log ("Got id");
      }
      if (ld.status == MessageStatus.Update)
      {
        addLocation (clientDatabase, clientDatabaseMutex, ld);
        Debug.Log ("Got update");

        updateRequired = true;
      }
        
      }
      catch (Exception e)
      {
        Debug.Log ("Client Communication Failed : " + e + " " + e.Message);
        stream.Close();
        
        break;
      }
    }
  }
  
  public void sendPosition (float lati, float longi)
  {
    if (clientSocket == null)
      return;
    
    LocationData ld = new LocationData ();
    ld.status = MessageStatus.Add;
    ld.locType = LocationType.Participant;
    ld.identifier = clientIdentifier;
    ld.latitude = lati;
    ld.longitude = longi;
    
    MemoryStream ms = new MemoryStream ();
    new BinaryFormatter ().Serialize (ms, ld);
    byte [] serializedData = ms.ToArray ();
    
    NetworkStream stream = clientSocket.GetStream();
    
    // Send the message to the connected TcpServer. 
    stream.Write (serializedData, 0, serializedData.Length);
    
    Debug.Log ("Sent: " + serializedData.Length);         
    
    
    
  }
  
  public void sendMarker (float lati, float longi)
  {
    if (clientSocket == null)
      return;
    
    LocationData ld = new LocationData ();
    ld.status = MessageStatus.Add;
    ld.locType = LocationType.Waypoint;
    ld.identifier = -1;
    ld.latitude = lati;
    ld.longitude = longi;
    
    MemoryStream ms = new MemoryStream ();
    new BinaryFormatter ().Serialize (ms, ld);
    byte [] serializedData = ms.ToArray ();
    
    NetworkStream stream = clientSocket.GetStream();
    
    // Send the message to the connected TcpServer. 
    stream.Write (serializedData, 0, serializedData.Length);
    
    Debug.Log ("Sent: " + serializedData.Length);         
  }

  public void startServer ()
  {
    Thread t = new Thread (server);
    t.Start ();
  }
  
  public void startClient ()
  {
    clientIdentifier = -1;
    try 
    {
      clientSocket = new TcpClient("127.0.0.1", port);
    } 
    catch (Exception e)
    {
      Debug.Log ("Client Exception: " + e.Message);
    }
    Thread t = new Thread (() => handleClient (clientSocket));
    t.Start ();
  }
  
  void Start()
  {
    //         ServicePointManager.ServerCertificateValidationCallback = TrustCertificate;
    
    
    //     System.Threading.Thread.Sleep(3000);
    //     t = new Thread(client);
    //     t.Start();
    // 
    //     System.Threading.Thread.Sleep(1000);
    //     t = new Thread(client);
    //     t.Start();
  }
  
  private void updateScene ()
  {
     foreach (Transform child in boardState.transform) 
     {
       GameObject.Destroy(child.gameObject);
     }
 
    clientDatabaseMutex.WaitOne ();
    
    foreach (LocationData ld in clientDatabase)
    {
      GameObject g = Instantiate (pieceTemplate);
      g.transform.position = new Vector3 (ld.longitude, ld.latitude, 0);
      Color c = new Color ();
      switch (ld.locType)
      {
        case LocationType.Participant: c = new Color (1, 0, 0); break;
        case LocationType.Waypoint: c = new Color (0, 1, 0); break;
        default: c = new Color (0, 0.5f, 0.5f); break;
      }
      g.GetComponent <MeshRenderer> ().material.color = c;
      g.transform.SetParent (boardState.transform);
      Debug.Log ("Placing : " + ld.longitude + " " + ld.latitude + " " + ld.locType + " " + ld.identifier + " - " + clientDatabase.Count);
    }
    clientDatabaseMutex.ReleaseMutex ();
  }
  
  void Update ()
  {
    if (serverName != null)
    {
      serverAddress.text = serverName; // has to be done on main thread.
    }
    
    if (updateRequired)
    {
      updateScene ();
      updateRequired = false;
    }
  }
}
