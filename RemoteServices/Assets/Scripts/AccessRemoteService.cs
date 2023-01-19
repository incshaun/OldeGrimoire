using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Text; 
using System.Net;
using System.Net.Sockets;

using TMPro;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;

public class AccessRemoteService : MonoBehaviour
{
    public TMP_InputField inputTextField;
    public TMP_Text outputTextField;
    
    public string serverName = "192.168.1.150";
    public int serverPort = 8800;

    private class ServiceConnection
    {
      public TcpClient client = null;
      public SemaphoreSlim connectionMutex = null;  
      // This field is cosmetic only, to indicate when the server backlog starts to grow.
      public bool locked = false;
      
      public void shutdown ()
      {
        if (client != null)
        {
            client.Close();
            client = null;
        }
      }
      
      public ServiceConnection ()
      {
          client = null;
          connectionMutex = new SemaphoreSlim (1);
          locked = false;
      }
    }
    
    //private int blockSize = 1024;
//     private TcpClient client = null;

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

    public static int hostByteToUInt (byte [] b)
    {
        if (BitConverter.IsLittleEndian)
        {
            //Debug.Log ("Lit en" + b[0] + " " + b.Length);
            Array.Reverse(b);
        }
        int i = BitConverter.ToInt32(b, 0);
        return i;
    }

    public static async Task<byte []> readAmount (NetworkStream stream, int amount)
    {
        try
        {
            byte[] buffer = new byte[amount];
            int received = 0;
            while (received < amount)
            {
                int amountReceived = await stream.ReadAsync(buffer, received, amount - received);
                received += amountReceived;
            }
            return buffer;
        }
        catch (Exception)
        {
            Debug.Log("Error reading data");
            return null;
        }
    }

    public static async Task<int> readInt (NetworkStream stream)
    {
        byte[] buffer = await readAmount(stream, 4);
        if ((buffer != null) && (buffer.Length == 4))
        {
            return hostByteToUInt(buffer);
        }
        else
        {
            throw new SystemException("Unable to read an int");
        }
    }

    void OnApplicationQuit()
    {
        if (speechService != null)
        {
            speechService.shutdown ();
        }
    }

    private async Task<byte []> sendAndReceive (ServiceConnection service, byte [] data)
    {
        // Messages on a single service are queued, so multiple threads sending a request
        // and waiting for a response have to do this sequentially. Separate services have
        // their own connections so can run in parallel but increase server overhead.
        // Mutex lock at the start and end ensure this. Any early leaving must unlock the
        // mutex.
        
        if (service.locked)
        {
            Debug.Log ("Warning. Will have to block on sendAndReceive. Potential server overload situation.");
        }
        await service.connectionMutex.WaitAsync ();
        service.locked = true;
        
        byte[] result = null;

        try
        {
            if (service.client == null)
            {
                IPAddress[] addresslist = await Dns.GetHostAddressesAsync(serverName);
                //Debug.Log("Addresses: " + addresslist);
                try
                {
                    service.client = new TcpClient();
                    await service.client.ConnectAsync(addresslist[0], serverPort);
                    Debug.Log("Connected");
                }
                catch (SocketException)
                {
                    Debug.Log("Could not connect: check that the server is actually running.");
                    service.client = null;
                }
            }

            if ((service.client != null) && (service.client.Connected))
            {
                //Debug.Log("Sending");
                NetworkStream stream = null;
                try
                {
                    stream = service.client.GetStream();
                }
                catch (InvalidOperationException)
                {
                    Debug.Log("Could not get stream: check that the server is actually running.");
                    stream = null;
                    service.client = null;
                }

                if (stream != null)
                {
                    // Send a header with the length of the next block in the stream.
                    byte [] bint = networkUIntToByte((uint)data.Length);
                    await stream.WriteAsync(bint, 0, bint.Length);

                    await stream.WriteAsync(data, 0, data.Length);
                    //Debug.Log("Sent");
                    int length = await readInt(stream);
//                     Debug.Log ("Expect: " + length);
                    result = await readAmount(stream, length);
                    //int amountReceived = await stream.ReadAsync (result);
                    //Debug.Log ("Received: " + amountReceived);
                }
            }
        }
        catch (Exception e)
        {
            Debug.Log("Exception in sendAndReceive: " + e);
            service.client = null;
            result = null;
        }

        service.locked = false;
        service.connectionMutex.Release ();
        
        return result;
    }

    // Provide a way of populating the output text, with a set number of lines.
    private const int numberOutputLines = 4;
    private string[] outputLines = new string [numberOutputLines];
    private int currentOutputLine = 0;
   private void addOutputLine (string line)
   {
        // Control characters interfere with string appending. https://stackoverflow.com/questions/15259275/removing-hidden-characters-from-within-strings/15259355#15259355
        line = new string(line.Where(c => !char.IsControl(c)).ToArray());

        outputLines[currentOutputLine] = line;
        currentOutputLine = (currentOutputLine + 1) % numberOutputLines;
        string result = "";
        for (int i = 0; i < numberOutputLines; i++)
        {
            result += outputLines[(currentOutputLine + i) % numberOutputLines];
        }
        outputTextField.text = result;
    }

    //private IEnumerator recordAudio()
    //{
    //    // Set the microphone recording. Service requires 16 kHz sampling.
    //    audioClip = Microphone.Start(null, false, recordDuration, 16000);
    //    yield return new WaitForSeconds(recordDuration);
    //    Microphone.End(null);

    //    doPlayback();
    //    Debug.Log("Channels " + audioClip.channels + " Samples " + audioClip.samples + "Rate " + audioClip.frequency);
    //}

    //public void doRecord ()
    //{
    //    StartCoroutine(recordAudio());
    //}

    //public void doPlayback ()
    //{
    //    // Play the recording back, to validate it was recorded correctly.
    //    AudioSource audioSource = GetComponent<AudioSource>();
    //    if ((audioClip != null) && (audioSource != null))
    //    {
    //        audioSource.clip = audioClip;
    //        audioSource.Play();
    //    }
    //}

    public void transmitToServer()
    {
        //doTransmitText();
        //doTransmitAudio();
        //doTransmitAudioStream();
    }

    private async Task doTransmitText()
    {
        Debug.Log("Transmitting data: " + inputTextField.text);

//         // The actual interactions with the remote server will use byte arrays,
//         // to allow any form of data to interchanged. This method converts to/from
//         // text for demonstration/testing purposes.
//         byte[] data = Encoding.ASCII.GetBytes(inputTextField.text);
//         byte[] result = await sendAndReceive(data);
// 
//         string resultString = Encoding.ASCII.GetString(result);
//         outputTextField.text = resultString;
// 
//         Debug.Log("Received result: " + outputTextField.text);
    }

    //private async Task doTransmitAudio ()
    //{
    //    if (audioClip != null)
    //    {
    //        try
    //        {
    //            var samples = new float[audioClip.samples];
    //            audioClip.GetData(samples, 0);
    //            Debug.Log("Transmitting audio: " + samples.Length);
    //            byte[] data = new byte[2 * samples.Length];
    //            int rescaleFactor = 32767; //to convert float to Int16
    //            for (int i = 0; i < samples.Length; i++)
    //            {
    //                short v = (short)(samples[i] * rescaleFactor);
    //                // Wav byte order is opposite network.
    //                byte[] b = BitConverter.GetBytes(v);
    //                if (!BitConverter.IsLittleEndian)
    //                {
    //                    Array.Reverse(b);
    //                }
    //                b.CopyTo(data, i * 2);
    //            }
    //            Debug.Log("Transmitting data: " + data.Length);

    //            // The actual interactions with the remote server will use byte arrays,
    //            // to allow any form of data to interchanged. This method converts to/from
    //            // text for demonstration/testing purposes.
    //            byte[] result = await sendAndReceive(data);

    //            string resultString = Encoding.ASCII.GetString(result);
    //            outputTextField.text = resultString;

    //            Debug.Log("Received result: " + outputTextField.text);
    //        }
    //        catch (Exception e)
    //        {
    //            Debug.Log("Something bad: " + e);
    //        }
    //    }
    //}

    private AudioClip audioClip = null;
    private bool audioRecording = false;
    private int lastRecordPosition;
    private int recordBufferLength;
    private int recordDuration = 10; // seconds. Should be longer than the transmissionDuration. Any extra adds to latency, but improves resiliance to delays.
    private int transmissionDuration = 5; // seconds.
    private int recordRate = 16000; // samples per second.
    private int transmissionThreshold;
    private void doTransmitAudioStream()
    {
        audioClip = Microphone.Start(Microphone.devices[0], true, recordDuration, recordRate);
        Debug.Log("Starting microphone: " + Microphone.devices[0] + " " + audioClip);
        lastRecordPosition = 0;
        recordBufferLength = recordDuration * recordRate;
        transmissionThreshold = transmissionDuration * recordRate;
        audioRecording = true;
    }

//     // Since requests to the server are sequentially over a single stream,
//     // we have to wait until the current request is complete before sending
//     // the next, or the stream will get messed up with interleaved requests.
//     private Task runningTask = null;
    private void transmitAsRequired (ServiceConnection speechService)
    {
//         if (runningTask != null)
//         {
//             await Task.WhenAny (runningTask);
//             runningTask = null;
//         }
//         
        try
        {
            //Debug.Log("Recording at position: " + Microphone.GetPosition(Microphone.devices[0]));
            int currentPosition = Microphone.GetPosition(Microphone.devices[0]);
            if ((currentPosition + recordBufferLength - lastRecordPosition) % recordBufferLength >= transmissionThreshold)
            {
                //            Debug.Log("Transmitting: " + transmissionThreshold + " " + lastRecordPosition + " -> " + currentPosition);
                lastRecordPosition = (lastRecordPosition + transmissionThreshold) % recordBufferLength;

                var samples = new float[transmissionThreshold];
                audioClip.GetData(samples, lastRecordPosition);
                //            Debug.Log("Transmitting audio: " + samples.Length);
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
                //            Debug.Log("Transmitting data: " + data.Length);

                // The actual interactions with the remote server will use byte arrays,
                // to allow any form of data to interchanged. This method converts to/from
                // text for demonstration/testing purposes.
                _ = sendAndUpdate(speechService, data);
            }
        }
        catch (Exception e)
        {
            Debug.Log("Something bad: " + e);
        }
    }

    private ServiceConnection speechService = null; // run a single connection for speech services.
    public void speechRecognitionService()
    {
        if (speechService == null)
        {
            speechService = new ServiceConnection ();
        }
        
        doTransmitAudioStream();
    }

    private async Task sendAndUpdate(ServiceConnection service, byte [] data)
    {
        try
        {
            byte[] result = await sendAndReceive(service, data);

            if ((result != null) && (result.Length > 0))
            {
                string resultString = Encoding.ASCII.GetString(result);
                addOutputLine(resultString);

                Debug.Log("Received result: " + resultString);
            }
        }
        catch (Exception e)
        {
            Debug.Log("Send and update bad: " + e);
        }
    }

    private void Update()
    {
        //Debug.Log("Main thread running");
        if (audioRecording)
        {
            transmitAsRequired(speechService);
        }
    }
}
