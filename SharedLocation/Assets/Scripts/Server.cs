using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using System.Threading;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;
using System;
//using System.Diagnostics;

public class Server : MonoBehaviour
{
    private static bool TrustCertificate (object sender, X509Certificate x509Certificate, X509Chain x509Chain, SslPolicyErrors sslPolicyErrors)
      {
        // All Certificates are accepted. Not good
        // practice, but outside scope of this
        // example.
        return true ;
      }

    // Make certificate:
    // openssl req -x509 -newkey rsa:4096 -keyout key.pem -out cert.pem -days 365
    // openssl pkcs12 -export -out certificate.pfx -inkey key.pem -in cert.pem 
    // Copy certificate.pfx to project.
    private void server()
    {
//X509Certificate2 cert = new X509Certificate2("certificate.pfx", "", X509KeyStorageFlags.UserKeySet | X509KeyStorageFlags.Exportable | X509KeyStorageFlags.PersistKeySet);

        X509Certificate2 cert = new X509Certificate2("certificate.pfx", "abc123", X509KeyStorageFlags.DefaultKeySet);
        X509Store store = new X509Store(StoreLocation.LocalMachine);
store.Open(OpenFlags.ReadWrite);
if (!store.Certificates.Contains(cert))
{
    store.Add(cert);
}
store.Close();

/*        ProcessStartInfo psi = new ProcessStartInfo();            
psi.FileName = "netsh";
psi.Arguments = "http add sslcert ipport=0.0.0.0:8080 certhash=" + cert.Thumbprint;
Process proc = Process.Start(psi);                
proc.WaitForExit();
psi.Arguments = "http add sslcert ipport=[::]:8080 certhash=" + cert.Thumbprint;
proc = Process.Start(psi);
proc.WaitForExit();*/

                HttpListener listener = new HttpListener();
        listener.Prefixes.Add("https://+:8080/");
        listener.Start();

        UnityEngine.Debug.Log("Listening...");
        HttpListenerContext context = null;
        try
         {
        UnityEngine.Debug.Log("Listening...0A");
        context = listener.GetContext();
                    UnityEngine.Debug.Log("Listening...0B");

        }
        catch (Exception e)
        {
            UnityEngine.Debug.Log ("EWrror " + e);
        }
        UnityEngine.Debug.Log("Listening...1");
        HttpListenerRequest request = context.Request;
        UnityEngine.Debug.Log("Listening...2");
        // Obtain a response object.
        HttpListenerResponse response = context.Response;
        // Construct a response.
        string responseString = "<HTML><BODY> Hello world!</BODY></HTML>";
        byte[] buffer = System.Text.Encoding.UTF8.GetBytes(responseString);
        // Get a response stream and write the response to it.
        response.ContentLength64 = buffer.Length;
        System.IO.Stream output = response.OutputStream;
        output.Write(buffer, 0, buffer.Length);
        // You must close the output stream.
        output.Close();
        listener.Stop();
    }

    private void client()
    {
        string url = "https://127.0.0.1:8080/";
        UnityEngine.Debug.Log("Retrieving: " + url);
        WebRequest www = WebRequest.Create(url);

        var response = www.GetResponse();
        UnityEngine.Debug.Log("Got response: " + response);

        // Obtain a 'Stream' object associated with the response object.
        Stream ReceiveStream = response.GetResponseStream();

        StreamReader readStream = new StreamReader(ReceiveStream);
        UnityEngine.Debug.Log("\nResponse stream received");
        char[] read = new char[256];

        // Read 256 charcters at a time.    
        int count = readStream.Read(read, 0, 256);
        UnityEngine.Debug.Log("HTML...\r\n");

        while (count > 0)
        {
            // Dump the 256 characters on a string and display the string onto the console.
            string str = new string(read, 0, count);
            UnityEngine.Debug.Log(str);
            count = readStream.Read(read, 0, 256);
        }

        UnityEngine.Debug.Log("");
        // Release the resources of stream object.
        readStream.Close();

        // Release the resources of response object.
        response.Close();
    }

    void Start()
    {
        ServicePointManager.ServerCertificateValidationCallback = TrustCertificate;

        Thread t = new Thread(server);
        t.Start();

        System.Threading.Thread.Sleep(3000);
        t = new Thread(client);
        t.Start();
    }
}
