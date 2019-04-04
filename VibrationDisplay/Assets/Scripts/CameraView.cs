using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraView : MonoBehaviour
{
    // Copies of the textures.
    private WebCamTexture webcamTex = null;
    private Texture2D edgeTex;
    private Texture2D sourceCopyTex;
    private Texture2D reduceTex;

    private int mipLevel = 5;

    // Accessor methods.
    public Texture getCameraImage()
    {
        return webcamTex;
    }

    public Texture2D getEdgeImage()
    {
        return edgeTex;
    }

    public WebCamTexture getWebcamTex()
    {
        if (webcamTex == null)
        {
            webcamTex = new WebCamTexture();
            webcamTex.Play();
            print(webcamTex.deviceName + " " + webcamTex.width + " " + webcamTex.height);
        }

        return webcamTex;
    }

    // Initialization of the textures.
    void Start()
    {
        // Start live camera feed.
        webcamTex = getWebcamTex();

        // Set the sizes of the other textures based on the camera reference.
        sourceCopyTex = new Texture2D(webcamTex.width, webcamTex.height);
        reduceTex = new Texture2D(webcamTex.width >> mipLevel, webcamTex.height >> mipLevel);
        edgeTex = new Texture2D (reduceTex.width, reduceTex.height);

        GetComponent<MeshRenderer>().sharedMaterial.mainTexture = edgeTex;

        // Update buffers asynchronously and partially to minimize run time overhead.
        StartCoroutine(updateEdgeImage());
    }

    void Update ()
    {
        // Length of pulse replaces lack of amplitude control.

        //// Mode where running a finger over the screen provides some feel for the
        //// edges in the image.
        //Vector3 pos = Input.mousePosition;

        //Ray ray = Camera.main.ScreenPointToRay(pos);

        //RaycastHit hit;
        //float edgeStrength;
        //if (Physics.Raycast (ray, out hit))
        //{
        //  edgeStrength = edgeTex.GetPixelBilinear (hit.textureCoord.x, hit.textureCoord.y).r;
        //  Vibration.Vibrate ((int) (edgeStrength * 10.0f));
        //}

        // Mode where direction camera is aimed controls edge related vibration.
        float edgeStrength;
        edgeStrength = edgeTex.GetPixelBilinear (0.5f, 0.5f).r;
        Vibration.Vibrate ((int) (edgeStrength * 10.0f));
    }

    // Extract the edge image. Do it block by block with yield after
    // each block to avoid significant performance impacts.
    private IEnumerator updateEdgeImage()
    {
        while (true)
        {
            sourceCopyTex.SetPixels(webcamTex.GetPixels ());
            sourceCopyTex.Apply ();
            reduceTex.SetPixels(sourceCopyTex.GetPixels (mipLevel));
            reduceTex.Apply();

            // edge detect.
            TextureFilter.Convolution(reduceTex, edgeTex, TextureFilter.EDGEDETECT_KERNEL_3, 1);
            Color[] cols = edgeTex.GetPixels();
            // convert to grayscale.
            for (int k = 0; k < cols.Length; k++)
            {
                float v = (cols[k].r + cols[k].g + cols[k].b) / 3;
                cols[k].r = v;
                cols[k].g = v;
                cols[k].b = v;
            }

            edgeTex.SetPixels (cols);

            GetComponent<MeshRenderer>().sharedMaterial.mainTexture = edgeTex;
            yield return new WaitForSeconds(0.01f);
        }
    }
}
