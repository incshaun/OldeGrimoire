using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text; 
using System.Net;
using System.Net.Sockets;

using TMPro;

public class AccessRemoteService : MonoBehaviour
{
    public TMP_InputField inputTextField;
    public TMP_Text outputTextField;
    
    public string serverName = "192.168.1.150";
    public int serverPort = 8800;
    
    private int blockSize = 1024;
    private Socket commSocket = null;
    
    private byte [] sendAndReceive (byte [] data)
    {
        if (commSocket == null)
        {
            IPAddress[] addresslist = Dns.GetHostAddresses (serverName);
            commSocket = new Socket (AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            commSocket.Connect (addresslist[0], serverPort);
        }
        
        byte [] result = new byte [blockSize];
        if (commSocket != null)
        {
            commSocket.Send (data);
            SocketError errorCode;
            int amountReceived = commSocket.Receive (result, 0, blockSize, SocketFlags.None, out errorCode);
            Debug.Log ("Received: " + amountReceived + " " + errorCode);
        }
        
        return result;
    }
    
    public void transmitToServer ()
    {
        Debug.Log ("Transmitting data: " + inputTextField.text);
        
        // The actual interactions with the remote server will use byte arrays,
        // to allow any form of data to interchanged. This method converts to/from
        // text for demonstration/testing purposes.
        byte [] data = Encoding.ASCII.GetBytes (inputTextField.text);
        byte [] result = sendAndReceive (data);
        
        string resultString = Encoding.ASCII.GetString (result);
        outputTextField.text = resultString;
        
        Debug.Log ("Received result: " + outputTextField.text);
    }
}
