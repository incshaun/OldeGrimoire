using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Net;
using System.IO;

public class MoveWithButtons : MonoBehaviour {

  public Text statusText;
  
  public Material mapMaterial;
  
  public GameObject marker;
  public GameObject mapPlane;
  
  private float longitude = 0.0f;
  private float latitude = 0.0f;
  private int zoom = 0;
  
  private static bool TrustCertificate (object sender, X509Certificate x509Certificate, X509Chain x509Chain, SslPolicyErrors sslPolicyErrors)
  {
    // All Certificates are accepted. Not good
    // practice, but outside scope of this
    // example.
    return true ;
  }

  void Start ()
  {
    ServicePointManager.ServerCertificateValidationCallback = TrustCertificate;
    updateMapView ();
  }
  
  private void getTileCoordinates (float longitude, float latitude, int zoom, out int x, out int y)
  {
    x = (int) (Mathf.Floor ((longitude + 180.0f) / 360.0f * Mathf.Pow (2.0f, zoom)));
    y = (int) (Mathf.Floor ((1.0f - Mathf.Log (Mathf.Tan (latitude * Mathf.PI / 180.0f) + 1.0f / Mathf.Cos (latitude * Mathf.PI / 180.0f)) / Mathf.PI) / 2.0f * Mathf.Pow (2.0f, zoom)));
  }
  
  private void getGeoCoordinates (int x, int y, int zoom, out float longitude, out float latitude) 
  {
    float n = Mathf.PI - 2.0f * Mathf.PI * y / Mathf.Pow (2.0f, zoom);
    
    longitude = x / Mathf.Pow (2.0f, zoom) * 360.0f - 180.0f;
    latitude = 180.0f / Mathf.PI * Mathf.Atan (0.5f * (Mathf.Exp (n) - Mathf.Exp (-n)));
  }

  private void updateTexture (int x, int y, int z)
  {
    string url = "https://a.tile.openstreetmap.org/" + z + "/" + x + "/" + y + ".png";
    // Similar process with another map service. 
    // Avoid use without considering terms of service.
    // string url = "https://mt.google.com/vt/lyrs=m&x=" + x + "&y=" + y + "&z=" + z;
    Debug.Log ("Retrieving: " + url);
    WebRequest www = WebRequest.Create (url);
    
    var response = www.GetResponse ();
    
    Texture2D tex = new Texture2D (2, 2);
    // Retrieve a large number of bytes - should be more than in a tile texture.
    ImageConversion.LoadImage (tex, new BinaryReader (response.GetResponseStream ()).ReadBytes (1000000)); 
    mapMaterial.mainTexture = tex;
  }

  private void updateMapView ()
  {
    int x;
    int y;
    getTileCoordinates (longitude, latitude, zoom, out x, out y);
    updateTexture (x, y, zoom);

    // Place a marker at the current position on the tile.
    float cornerLatA;
    float cornerLongA;
    float cornerLatB;
    float cornerLongB;
    getGeoCoordinates (x, y, zoom, out cornerLongA, out cornerLatA);
    getGeoCoordinates (x + 1, y + 1, zoom, out cornerLongB, out cornerLatB);
    // interpolate current coordinates relative to the coordinates of the
    // tile corners. Assumes the plane coordinates run from (-5,-5) to (5,5).
    float r = 10.0f * ((-(longitude - cornerLongA) / (cornerLongB - cornerLongA))) + 5.0f;
    float d = 10.0f * ((-(latitude - cornerLatA) / (cornerLatB - cornerLatA))) + 5.0f;
    marker.transform.position = mapPlane.transform.position - mapPlane.transform.forward * d + mapPlane.transform.right * r;
    
    statusText.text = longitude + "," + latitude + "(" + zoom + ") " + "[" + x + "," + y + "]";    
  }
  
  public void onButtonEvent (float dx, float dy, int dz)
  {
    zoom += dz;
    // Calculate step so that it takes a few button presses 
    // to move across a tile at that level.
    float step = 0.3f * 1.0f / Mathf.Pow (2.0f, zoom);
    longitude += 360.0f * dx * step;
    latitude += 180.0f * dy * step;
    
    updateMapView ();
  }
  public void leftButton () { onButtonEvent (-1.0f, 0.0f, 0); }
  public void rightButton () { onButtonEvent (1.0f, 0.0f, 0); }
  public void upButton () { onButtonEvent (0.0f, 1.0f, 0); }
  public void downButton () { onButtonEvent (0.0f, -1.0f, 0); }
  public void inButton () { onButtonEvent (0.0f, 0.0f, 1); }
  public void outButton () { onButtonEvent (0.0f, 0.0f, -1); }
  
}
