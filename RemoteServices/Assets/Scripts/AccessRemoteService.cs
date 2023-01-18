using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
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

    public static byte[] networkShortToByte(short s)
    {
        byte[] b = BitConverter.GetBytes(s);
        if (BitConverter.IsLittleEndian)
        {
            Array.Reverse(b);
        }
        return b;
    }
    public static byte[] networkUIntToByte(uint i)
    {
        byte[] b = BitConverter.GetBytes(i);
        if (BitConverter.IsLittleEndian)
        {
            Array.Reverse(b);
        }
        return b;
    }

    private async Task<byte []> sendAndReceive (byte [] data)
    {
        if (client == null)
        {
            IPAddress[] addresslist = await Dns.GetHostAddressesAsync (serverName);
            Debug.Log("Addresses: " + addresslist);
            client = new TcpClient();
            await client.ConnectAsync (addresslist[0], serverPort);
            Debug.Log("Connected");
        }

        byte [] result = new byte [blockSize];
        if (client != null)
        {
            Debug.Log("Sending");
            NetworkStream stream = client.GetStream();

            // Send a header with the length of the next block in the stream.
            await stream.WriteAsync(networkUIntToByte((uint) data.Length));

            await stream.WriteAsync (data);
            Debug.Log("Sent");
            int amountReceived = await stream.ReadAsync (result);
            Debug.Log ("Received: " + amountReceived);
        }
        
        return result;
    }

    private int recordDuration = 5;
    private AudioClip audioClip = null;

    private IEnumerator recordAudio()
    {
        // Set the microphone recording. Service requires 16 kHz sampling.
        audioClip = Microphone.Start(null, false, recordDuration, 16000);
        yield return new WaitForSeconds(recordDuration);
        Microphone.End(null);

        doPlayback();
        Debug.Log("Channels " + audioClip.channels + " Samples " + audioClip.samples + "Rate " + audioClip.frequency);
    }

    public void doRecord ()
    {
        StartCoroutine(recordAudio());
    }

    public void doPlayback ()
    {
        // Play the recording back, to validate it was recorded correctly.
        AudioSource audioSource = GetComponent<AudioSource>();
        if ((audioClip != null) && (audioSource != null))
        {
            audioSource.clip = audioClip;
            audioSource.Play();
        }
    }

    public void transmitToServer()
    {
        //doTransmitText();
        doTransmitAudio();
    }

    private async Task doTransmitText()
    {
        Debug.Log("Transmitting data: " + inputTextField.text);

        // The actual interactions with the remote server will use byte arrays,
        // to allow any form of data to interchanged. This method converts to/from
        // text for demonstration/testing purposes.
        byte[] data = Encoding.ASCII.GetBytes(inputTextField.text);
        byte[] result = await sendAndReceive(data);

        string resultString = Encoding.ASCII.GetString(result);
        outputTextField.text = resultString;

        Debug.Log("Received result: " + outputTextField.text);
    }

    private async Task doTransmitAudio ()
    {
        if (audioClip != null)
        {
            try
            {
                var samples = new float[audioClip.samples];
                audioClip.GetData(samples, 0);
                Debug.Log("Transmitting audio: " + samples.Length);
                byte[] data = new byte[2 * samples.Length];
                int rescaleFactor = 32767; //to convert float to Int16
                for (int i = 0; i < samples.Length; i++)
                {
                    short v = (short)(samples[i] * rescaleFactor);
                    // Wav byte order is opposite network.
                    byte[] b = BitConverter.GetBytes(v);
                    if (!BitConverter.IsLittleEndian)
                    {
                        Array.Reverse(b);
                    }
                    b.CopyTo(data, i * 2);
                }
                Debug.Log("Transmitting data: " + data.Length);

                // The actual interactions with the remote server will use byte arrays,
                // to allow any form of data to interchanged. This method converts to/from
                // text for demonstration/testing purposes.
                byte[] result = await sendAndReceive(data);

                string resultString = Encoding.ASCII.GetString(result);
                outputTextField.text = resultString;

                Debug.Log("Received result: " + outputTextField.text);
            }
            catch (Exception e)
            {
                Debug.Log("Something bad: " + e);
            }
        }
    }

    private void Update()
    {
        //Debug.Log("Main thread running");
    }
}
