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

    enum ServiceType 
    {
        SpeechRecognition = 10,
        SpeechSynthesis = 13,
        ImageIdentification = 19,
    }
    
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
    
    public static void networkShortToByte(short s, byte[] dest, int offset = 0)
    {
        byte[] b = BitConverter.GetBytes(s);
        if (BitConverter.IsLittleEndian)
        {
            Array.Reverse(b);
        }
        Buffer.BlockCopy (b, 0, dest, offset, 2);
    }
    public static void networkUIntToByte(uint i, byte[] dest, int offset = 0)
    {
        byte[] b = BitConverter.GetBytes(i);
        if (BitConverter.IsLittleEndian)
        {
            Array.Reverse(b);
        }
        Buffer.BlockCopy (b, 0, dest, offset, 4);
    }

    public static int hostByteToUInt (byte [] bytes, int offset = 0)
    {
        byte [] b = new byte [4];
        Buffer.BlockCopy (bytes, offset, b, 0, 4);
        
        if (BitConverter.IsLittleEndian)
        {
            //Debug.Log ("Lit en" + b[0] + " " + b.Length);
            Array.Reverse(b);
        }
        int i = BitConverter.ToInt32(b);
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

    private async Task<(byte [], byte [])> sendAndReceive (ServiceConnection service, ServiceType serviceType, byte [] header, byte [] data)
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
        
        byte[] resultHeader = null;
        byte[] resultData = null;

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
                    byte [] bint = new byte [4];
                    
                    // Send a header with the length of the next block in the stream.
                    // 1. Service type
                    networkUIntToByte((uint)serviceType, bint);
                    await stream.WriteAsync(bint, 0, bint.Length);
                    
                    // 2. Header
                    networkUIntToByte((uint)header.Length, bint);
                    await stream.WriteAsync(bint, 0, bint.Length);
                    await stream.WriteAsync(header, 0, header.Length);
                    
                    // 3. Data
                    networkUIntToByte((uint)data.Length, bint);
                    await stream.WriteAsync(bint, 0, bint.Length);
                    await stream.WriteAsync(data, 0, data.Length);
                    
                    // Receive the response, with a header and body.
                    int length;
                    // 1. Header
                    length = await readInt(stream);
                    resultHeader = await readAmount(stream, length);
                    // 2. Data
                    length = await readInt(stream);
                    resultData = await readAmount(stream, length);
                    //int amountReceived = await stream.ReadAsync (result);
                    //Debug.Log ("Received: " + amountReceived);
                }
            }
        }
        catch (Exception e)
        {
            Debug.Log("Exception in sendAndReceive: " + e);
            service.client = null;
            resultHeader = null;
            resultData = null;
        }

        service.locked = false;
        service.connectionMutex.Release ();
        
        return (resultHeader, resultData);
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

    // Speech Synthesis
        
    public void doSpeechSynthesis ()
    {
        byte[] data = Encoding.ASCII.GetBytes(inputTextField.text);
        _ = speechSynthesisService (data);
    }

    private ServiceConnection speechSynthService = null; // run a single connection for speech services.
    public async Task speechSynthesisService(byte [] data)
    {
        if (speechSynthService == null)
        {
            speechSynthService = new ServiceConnection ();
        }

        byte [] header = new byte [0];
        
        byte [] resultHeader;
        byte[] resultData;
        (resultHeader, resultData) = await sendAndReceive(speechSynthService, ServiceType.SpeechSynthesis, header, data);

        // Parse the returned header for audio parameters.
        int channels = hostByteToUInt (resultHeader, 0);        
        int width = hostByteToUInt (resultHeader, 4);        
        int rate = hostByteToUInt (resultHeader, 8);        
        
        Debug.Assert (width == 2);

        float [] vals = new float [resultData.Length / 2];
        float rescaleFactor = 32767.0f; //to convert float to Int16
        for (int i = 0; i < vals.Length; i++)
        {
            byte [] b = new byte [2];
            Buffer.BlockCopy (resultData, i * 2, b, 0, 2);
            if (!BitConverter.IsLittleEndian)
            {
                Array.Reverse(b);
            }
            short v = BitConverter.ToInt16 (b);
            vals[i] = v / rescaleFactor;
        }
        
        AudioClip audio = AudioClip.Create ("speech", vals.Length, channels, rate, false);
        audio.SetData (vals, 0);
       
        AudioSource audioSource = GetComponent<AudioSource>();
        if ((audio != null) && (audioSource != null))
        {
            audioSource.clip = audio;
            audioSource.Play();
        }
        outputTextField.text = "Received " + resultHeader.Length + "+" + resultData.Length + " bytes";

        Debug.Log("Received result: " + outputTextField.text + " - " + channels + " " + width + " " + rate);
    }

    // Speech Recognition
    
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

    private void transmitAsRequired (ServiceConnection speechService)
    {
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
                
                byte [] header = new byte [12];
                networkUIntToByte ((uint) audioClip.channels, header, 0);
                networkUIntToByte (2, header, 4); // sample width in bytes.
                networkUIntToByte ((uint) audioClip.frequency, header, 8);
                
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
                _ = sendAndUpdate(speechService, ServiceType.SpeechRecognition, header, data);
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

    private async Task sendAndUpdate(ServiceConnection service, ServiceType type, byte [] header, byte [] data)
    {
        try
        {
            byte [] resultHeader;
            byte[] resultData;

            (resultHeader, resultData) = await sendAndReceive(service, type, header, data);

            if ((resultData != null) && (resultData.Length > 0))
            {
                string resultString = Encoding.ASCII.GetString(resultData);
                addOutputLine(resultString);

                Debug.Log("Received result: " + resultString);
            }
        }
        catch (Exception e)
        {
            Debug.Log("Send and update bad: " + e);
        }
    }

    // Image Identification
    private WebCamTexture webcamTex = null;
    private bool webcamStarting = false;
    public Material cameraMaterial;
    private ServiceConnection imageIDService = null;
    public void identifyImage ()
    {
        if (webcamTex == null)
        {
          webcamTex = new WebCamTexture();
          webcamTex.Play();
          webcamStarting = true;
        }
        
        if (imageIDService == null)
        {
            imageIDService = new ServiceConnection ();
        }
        
        if ((cameraMaterial != null) && (webcamTex != null))
        {
            cameraMaterial.mainTexture = webcamTex;
        }
        
        if (webcamTex != null)
        {
            StartCoroutine (sendSnapshot ());
        }
    }
    
    private IEnumerator sendSnapshot ()
    {
        if (webcamStarting)
        {
            // wait to ensure camera is running.
            yield return new WaitForSeconds (1.0f);
            webcamStarting = false;
        }
        
        Texture2D tex = new Texture2D (webcamTex.width, webcamTex.height, TextureFormat.RGB24, false);
        // seem to need a flip vertical.
        for (int i = 0; i < webcamTex.height; i++)
        {
          tex.SetPixels(0, webcamTex.height - (i + 1), webcamTex.width, 1, webcamTex.GetPixels (0, i, webcamTex.width, 1));
        }
        tex.Apply ();
        byte [] data = tex.GetRawTextureData ();
        Debug.Log ("Got image: " + data.Length + " " + webcamTex.width + " " + webcamTex.height + " " + tex.width + " " + tex.height + " " + webcamTex.videoVerticallyMirrored);

        byte [] header = new byte [8];
        networkUIntToByte ((uint) tex.width, header, 0);
        networkUIntToByte ((uint) tex.height, header, 4); // sample width in bytes.
        
        _ = sendAndUpdate (imageIDService, ServiceType.ImageIdentification, header, data);        
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
