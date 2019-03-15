using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net.Sockets;
using System.Net;
using System;

public class DatagramCommunication {

  private int port = 8081;
  
  private UdpClient udpClient;
  
  private ControllerDetails lastController;
  private bool controlValid = false;
  
  public DatagramCommunication ()
  {
    udpClient = new UdpClient ();
    udpClient.Client.Bind (new IPEndPoint (IPAddress.Any, port));
    
    udpClient.BeginReceive (new AsyncCallback (udpReceive), null);
  }
  
  // Retrieve the most recently received controller details.
  public ControllerDetails receiveControllerDetails ()
  {
    if (controlValid)
    {
      controlValid = false;
      return lastController;
    }
    return null;
  }
  
  // Wait for a new message to arrive, and set that as the last message received.
  private void udpReceive (IAsyncResult res)
  {
    IPEndPoint from = new IPEndPoint(0, 0);
    byte [] buffer = udpClient.Receive(ref from);
    
    ControllerDetails details = ControllerDetails.deserialize (buffer);
    lastController = details;
    controlValid = true;
    
    udpClient.BeginReceive (new AsyncCallback (udpReceive), null);
  }

  // Broadcast the controller parameters.
  public void sendControllerDetails (ControllerDetails details)
  {
    byte [] data = details.serialize ();
    udpClient.Send(data, data.Length, "255.255.255.255", port);
    
  }  
}
