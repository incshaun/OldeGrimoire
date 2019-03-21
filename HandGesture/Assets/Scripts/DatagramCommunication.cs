using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net.Sockets;
using System.Net;
using System;

public class DatagramCommunication
{

    private int port = 8082;

    private UdpClient udpClient;

    private HandDetails lastHand;
    private bool controlValid = false;

    private IPEndPoint remoteep;

    public DatagramCommunication()
    {
        udpClient = new UdpClient();

        udpClient.Client.Bind(new IPEndPoint(IPAddress.Any, port));
        IPAddress multicastaddress = IPAddress.Parse("239.0.0.222");
        udpClient.JoinMulticastGroup(multicastaddress);
        remoteep = new IPEndPoint(multicastaddress, port);

        udpClient.BeginReceive(new AsyncCallback(udpReceive), null);
    }

    // Retrieve the most recently received hand details.
    public HandDetails receiveHandDetails()
    {
        if (controlValid)
        {
            controlValid = false;
            return lastHand;
        }
        return null;
    }

    // Wait for a new message to arrive, and set that as the last message received.
    private void udpReceive(IAsyncResult res)
    {
        IPEndPoint from = new IPEndPoint(0, 0);
        byte[] buffer = udpClient.Receive(ref from);

        HandDetails details = HandDetails.deserialize(buffer);
        lastHand = details;
        controlValid = true;

        udpClient.BeginReceive(new AsyncCallback(udpReceive), null);
    }

    // Broadcast the hand parameters.
    public void sendHandDetails(string hostname, HandDetails details)
    {
        if (hostname != null)
        {
            remoteep = new IPEndPoint(IPAddress.Parse(hostname), port);
        }

        byte[] data = details.serialize();
        udpClient.Send(data, data.Length, remoteep);

    }
}
