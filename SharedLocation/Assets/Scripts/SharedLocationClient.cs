using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System;
using System.Threading;

public class SharedLocationClient
{
    // The connection to the server.
    private TcpClient clientSocket = null;
    // Mutex to ensure sending/receiving on the client socket are done one message at a time.
    private Mutex clientSendMutex = new Mutex();
    private Mutex clientReceiveMutex = new Mutex();

    // An identifier, to be supplied by the server, identifying this client.
    // -1 corresponds to no identifier yet assigned.
    private int clientIdentifier = -1;

    // 
    private List<LocationData> clientDatabase = new List<LocationData>();
    private Mutex clientDatabaseMutex = new Mutex();

    // The client actions which run in their own thread. This
    // handles the processing of any messages which are sent by 
    // the server.
    public void handleClient(TcpClient clientSocket)
    {
        NetworkStream stream = clientSocket.GetStream();

        while (true)
        {
            try
            {
                LocationData ld = SharedLocation.receiveMessage(stream, clientReceiveMutex);

                Debug.Log("Client Received " + ld);

                switch (ld.status)
                {
                    case MessageStatus.IdSet:
                        clientIdentifier = ld.identifier;
                        break;
                    case MessageStatus.Update:
                        SharedLocation.addLocation(clientDatabase, clientDatabaseMutex, ld);
                        SharedLocation.updateRequired = true;
                        break;
                    default:
                        Debug.Log("Unexpected message type from server: " + ld.status);
                        break;
                }

            }
            catch (Exception e)
            {
                Debug.Log("Client Communication Failed : " + e + " " + e.Message);
                stream.Close();

                break;
            }
        }
    }

    public SharedLocationClient(string host)
    {
        clientIdentifier = -1;
        try
        {
            Debug.Log("Connecting to: " + host);
            clientSocket = new TcpClient(host, SharedLocation.port);
        }
        catch (Exception e)
        {
            Debug.Log("Client Exception: " + e.Message);
        }
        Thread t = new Thread(() => handleClient(clientSocket));
        t.Start();

    }

    // Send the client's position to the server.
    public void sendPosition(float lati, float longi)
    {
        if (clientSocket == null)
            return;

        LocationData ld = new LocationData();
        ld.status = MessageStatus.Add;
        ld.locType = LocationType.Participant;
        ld.identifier = clientIdentifier;
        ld.latitude = lati;
        ld.longitude = longi;

        SharedLocation.sendMessage (ld, clientSocket.GetStream(), clientSendMutex);
    }

    // Add a new marker on the server at the given position.
    public void sendMarker(float lati, float longi)
    {
        if (clientSocket == null)
            return;

        LocationData ld = new LocationData();
        ld.status = MessageStatus.Add;
        ld.locType = LocationType.Waypoint;
        ld.identifier = -1;
        ld.latitude = lati;
        ld.longitude = longi;

        SharedLocation.sendMessage (ld, clientSocket.GetStream(), clientSendMutex);
    }

    // Convert the client database into a scene element, under the object boardState.
    // Each element is created from the pieceTemplate, set to different colours according
    // to its role.
    // Needs to be run from the Unity main thread.
    public void updateScene (GameObject boardState, GameObject pieceTemplate)
    {
        foreach (Transform child in boardState.transform)
        {
            GameObject.Destroy(child.gameObject);
        }

        clientDatabaseMutex.WaitOne();

        foreach (LocationData ld in clientDatabase)
        {
            GameObject g = GameObject.Instantiate(pieceTemplate);
            g.transform.position = new Vector3(ld.longitude, ld.latitude, 0);
            Color c = new Color();
            switch (ld.locType)
            {
                case LocationType.Participant: c = new Color(1, 0, 0); break;
                case LocationType.Waypoint: c = new Color(0, 1, 0); break;
                default: c = new Color(0, 0.5f, 0.5f); break;
            }
            g.GetComponent<MeshRenderer>().material.color = c;
            g.transform.SetParent(boardState.transform);
        }
        clientDatabaseMutex.ReleaseMutex();
    }

}
