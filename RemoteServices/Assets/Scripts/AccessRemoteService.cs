using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text; 
using System.Net;
using System.Net.Sockets;

using TMPro;
using System.Threading.Tasks;

public class AccessRemoteService : MonoBehaviour
{
    public TMP_InputField inputTextField;
    public TMP_Text outputTextField;
    
    public string serverName = "192.168.1.150";
    public int serverPort = 8800;
    
    private int blockSize = 1024;
    private TcpClient client = null;
    
    private async Task<byte []> sendAndReceive (byte [] data)
    {
        if (client == null)
        {
            IPAddress[] addresslist = await Dns.GetHostAddressesAsync (serverName);
            Debug.Log("Addresses: " + addresslist);
            client = new TcpClient();
            await client.ConnectAsync (addresslist[0], serverPort);
        }
        
        byte [] result = new byte [blockSize];
        if (client != null)
        {
            NetworkStream stream = client.GetStream();
            await stream.WriteAsync (data);
            int amountReceived = await stream.ReadAsync (result);
            Debug.Log ("Received: " + amountReceived);
        }
        
        return result;
    }

    public void transmitToServer()
    {
        doTransmit();
    }

    private async Task doTransmit ()
    { 
        Debug.Log ("Transmitting data: " + inputTextField.text);
        
        // The actual interactions with the remote server will use byte arrays,
        // to allow any form of data to interchanged. This method converts to/from
        // text for demonstration/testing purposes.
        byte [] data = Encoding.ASCII.GetBytes (inputTextField.text);
        byte [] result = await sendAndReceive (data);
        
        string resultString = Encoding.ASCII.GetString (result);
        outputTextField.text = resultString;
        
        Debug.Log ("Received result: " + outputTextField.text);
    }

    private void Update()
    {
        Debug.Log("Main thread running");
    }
}
