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
    [Tooltip("A text field where the server can display its own address.")]
    public Text serverAddress;

    [Tooltip("An input field to enter the address of a server that the client connects to.")]
    public InputField serverInput;

    [Tooltip("A template for the object created to represent a piece at a given location.")]
    public GameObject pieceTemplate;

    [Tooltip("A parent object for any items added by this component. Children are deleted regularly so should be an object created just for this purpose.")]
    public GameObject boardState;

    // The port number that the server uses.
    static public Int32 port = 8080;

    // Server name field written by any servers that are created.
    static public string serverName = null;

    // If this flag is set, then the scene is regenerated from the client info.
    public static bool updateRequired = false;

    // A counter used to assign identifiers. 
    private static int id = 99;

    // Add one to the counter to get a new ID. Using a singleton 
    // pattern to ensure multiple counters don't get created.
    private static int getUniqueID()
    {
        id += 1;
        return id;
    }

    // Wait until a complete message has been received, extract
    // the data, and return the message object.
    static public LocationData receiveMessage(Stream stream, Mutex receiveMutex)
    {
        BinaryFormatter formatter = new BinaryFormatter();
        receiveMutex.WaitOne();
        LocationData ld = (LocationData)formatter.Deserialize(stream);
        receiveMutex.ReleaseMutex();
        return ld;
    }

    // Write the message to the given stream. This needs to be
    // serialized, and the channel locked.
    static public void sendMessage(LocationData message, Stream stream, Mutex sendMutex)
    {
        MemoryStream ms = new MemoryStream();
        new BinaryFormatter().Serialize(ms, message);
        byte[] serializedData = ms.ToArray();

        sendMutex.WaitOne();
        stream.Write(serializedData, 0, serializedData.Length);
        sendMutex.ReleaseMutex();
    }

    // Add a location entry to one of the database. To be thread safe, we also require
    // a mutex to lock the list before updating it.
    static public void addLocation(List<LocationData> list, Mutex mutex, LocationData ld)
    {
        // Ensure exclusive access to the list.
        mutex.WaitOne();
        // Assign identifiers if none are provided (value of -1)
        if (ld.identifier < 0)
        {
            ld.identifier = getUniqueID();
        }
        // Update an existing entry if one is found.
        bool found = false;
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
            list.Add(ld);
        }
        // Allow others to access the list.
        mutex.ReleaseMutex();
    }

    public void sendPosition(float lati, float longi)
    {
        activeClient.sendPosition (lati, longi);
    }

    public void sendMarker(float lati, float longi)
    {
        activeClient.sendMarker (lati, longi);
    }

    // The most recent client created.
    private SharedLocationClient activeClient;

    public void startClient()
    {
        activeClient = new SharedLocationClient (serverInput.text);
    }

    public void startServer ()
    {
        SharedLocationServer s = new SharedLocationServer ();
    }

    void Update()
    {
        // Any interaction with Unity has to be done
        // in the main thread. 

        // Show server name in text box.
        if (serverName != null)
        {
            serverAddress.text = serverName;
        }

        // Update the scene to reflect the
        // elements provided from the server.
        if (updateRequired)
        {
            activeClient.updateScene (boardState, pieceTemplate);
            updateRequired = false;
        }
    }
}
