using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Net;
using System.IO;

public class MoveWithButtons : MonoBehaviour
{

    public Text statusText;

    public MeshFilter mapObject;
    public Material mapMaterial;

    public GameObject marker;
    public GameObject mapPlane;

    private float longitude = 0.0f;
    private float latitude = 0.0f;
    private int zoom = 0;

    private static bool TrustCertificate(object sender, X509Certificate x509Certificate, X509Chain x509Chain, SslPolicyErrors sslPolicyErrors)
    {
        // All Certificates are accepted. Not good
        // practice, but outside scope of this
        // example.
        return true;
    }

    void Start()
    {
        ServicePointManager.ServerCertificateValidationCallback = TrustCertificate;
        updateMapView();
    }

    private void getTileCoordinates(float longitude, float latitude, int zoom, out int x, out int y)
    {
        x = (int)(Mathf.Floor((longitude + 180.0f) / 360.0f * Mathf.Pow(2.0f, zoom)));
        y = (int)(Mathf.Floor((1.0f - Mathf.Log(Mathf.Tan(latitude * Mathf.PI / 180.0f) + 1.0f / Mathf.Cos(latitude * Mathf.PI / 180.0f)) / Mathf.PI) / 2.0f * Mathf.Pow(2.0f, zoom)));
    }

    private void getGeoCoordinates(int x, int y, int zoom, out float longitude, out float latitude)
    {
        float n = Mathf.PI - 2.0f * Mathf.PI * y / Mathf.Pow(2.0f, zoom);

        longitude = x / Mathf.Pow(2.0f, zoom) * 360.0f - 180.0f;
        latitude = 180.0f / Mathf.PI * Mathf.Atan(0.5f * (Mathf.Exp(n) - Mathf.Exp(-n)));
    }

    private float[] convertElevationTexture(Texture2D tex, out float minheight, out float maxheight)
    {
        float[] htexdata = new float[tex.width * tex.height];
        bool setlimits = false;
        minheight = 0.0f;
        maxheight = 0.0f;

        for (int y = 0; y < tex.height; y++)
        {
            for (int x = 0; x < tex.width; x++)
            {
                Color a = tex.GetPixel(x, y);
                float height = ((a.r * 255.0f * 256.0f) + (a.g * 255.0f) + (a.b * 255.0f / 256.0f)) - 32768.0f;
                htexdata[y * tex.width + x] = height;

                if ((!setlimits) || (height < minheight))
                {
                    minheight = height;
                }
                if ((!setlimits) || (height > maxheight))
                {
                    maxheight = height;
                }
                setlimits = true;
            }
        }
        return htexdata;
    }

    private void updateMesh(Texture2D tex, int mwidth, int mheight, float heightrange)
    {
        float minheight;
        float maxheight;
        float[] heighttex = convertElevationTexture(tex, out minheight, out maxheight);

        Vector3[] vertices = new Vector3[(mwidth + 1) * (mheight + 1)];
        Vector2[] uvs = new Vector2[(mwidth + 1) * (mheight + 1)];
        int[] triangles = new int[6 * mwidth * mheight];

        int triangleIndex = 0;
        for (int y = 0; y < mheight + 1; y++)
        {
            for (int x = 0; x < mwidth + 1; x++)
            {
                float xc = (float)x / mwidth;
                float zc = (float)y / mheight;
                float yc = 0.0f;
                //yc = heighttex[(int)(zc * (tex.height - 1)) * tex.width + (int)(xc * (tex.width - 1))];
                //if (yc < 0.0f) yc = 0.0f;
                //yc = heightrange * (yc - minheight) / (maxheight - minheight);
                vertices[y * (mwidth + 1) + x] = new Vector3(xc - 0.5f, yc, zc - 0.5f);
                uvs[y * (mwidth + 1) + x] = new Vector2(xc, zc);

                // Skip the last row/col
                if ((x != mwidth) && (y != mheight))
                {
                    int topLeft = y * (mwidth + 1) + x;
                    int topRight = topLeft + 1;
                    int bottomLeft = topLeft + mwidth + 1;
                    int bottomRight = bottomLeft + 1;

                    triangles[triangleIndex++] = topRight;
                    triangles[triangleIndex++] = topLeft;
                    triangles[triangleIndex++] = bottomLeft;
                    triangles[triangleIndex++] = topRight;
                    triangles[triangleIndex++] = bottomLeft;
                    triangles[triangleIndex++] = bottomRight;
                }
            }
        }

        Mesh m = new Mesh();
        m.vertices = vertices;
        m.uv = uvs;
        m.triangles = triangles;
        m.RecalculateNormals();
        mapObject.mesh = m;
    }

    private void updateTexture(int x, int y, int z)
    {
        // Elevation tiles, see: https://www.mapzen.com/blog/terrain-tile-service/
        string url = "https://s3.amazonaws.com/elevation-tiles-prod/terrarium/" + z + "/" + x + "/" + y + ".png";
        Debug.Log("Retrieving: " + url);
        WebRequest www = WebRequest.Create(url);
        ((HttpWebRequest)www).UserAgent = "TerrainMaps";

        var response = www.GetResponse();

        Texture2D tex = new Texture2D(2, 2);
        // Retrieve a large number of bytes - should be more than in a tile texture.
        ImageConversion.LoadImage(tex, new BinaryReader(response.GetResponseStream()).ReadBytes(1000000));
        updateMesh(tex, 128, 128, 0.1f);
        mapMaterial.mainTexture = tex;
    }

    private void updateMapView()
    {
        int x;
        int y;
        getTileCoordinates(longitude, latitude, zoom, out x, out y);
        updateTexture(x, y, zoom);

        // Place a marker at the current position on the tile.
        float cornerLatA;
        float cornerLongA;
        float cornerLatB;
        float cornerLongB;
        getGeoCoordinates(x, y, zoom, out cornerLongA, out cornerLatA);
        getGeoCoordinates(x + 1, y + 1, zoom, out cornerLongB, out cornerLatB);
        // interpolate current coordinates relative to the coordinates of the
        // tile corners. Assumes the plane coordinates run from (-5,-5) to (5,5).
        float r = 10.0f * ((-(longitude - cornerLongA) / (cornerLongB - cornerLongA))) + 5.0f;
        float d = 10.0f * ((-(latitude - cornerLatA) / (cornerLatB - cornerLatA))) + 5.0f;
        marker.transform.position = mapPlane.transform.position - mapPlane.transform.forward * d + mapPlane.transform.right * r;

        statusText.text = longitude + "," + latitude + "(" + zoom + ") " + "[" + x + "," + y + "]";
    }

    public void onButtonEvent(float dx, float dy, int dz)
    {
        zoom += dz;
        // Calculate step so that it takes a few button presses 
        // to move across a tile at that level.
        float step = 1.0f * 1.0f / Mathf.Pow(2.0f, zoom);
        longitude += 360.0f * dx * step;
        latitude += 180.0f * dy * step;

        updateMapView();
    }
    public void leftButton() { onButtonEvent(-1.0f, 0.0f, 0); }
    public void rightButton() { onButtonEvent(1.0f, 0.0f, 0); }
    public void upButton() { onButtonEvent(0.0f, 1.0f, 0); }
    public void downButton() { onButtonEvent(0.0f, -1.0f, 0); }
    public void inButton() { onButtonEvent(0.0f, 0.0f, 1); }
    public void outButton() { onButtonEvent(0.0f, 0.0f, -1); }

}
