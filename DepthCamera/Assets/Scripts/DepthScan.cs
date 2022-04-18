using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Collections;

using TMPro;
using System.IO;

public class DepthScan : MonoBehaviour
{
    public ARCameraManager cameraManager;
    public AROcclusionManager occlusionManager;
    public Material depthMaterial;
    public Material colourMaterial;
    public TextMeshProUGUI statusText;
    public GameObject targetObject;
    
    void Start()
    {
       cameraManager.frameReceived += OnCameraFrameEventReceived; 
    }

    void Update()
    {
        Texture2D envDepth = occlusionManager.environmentDepthTexture;
//         statusText.text = "Got unv " + envDepth;
        depthMaterial.mainTexture = envDepth;
    }

    Texture2D colourTexture;
    Texture2D savedColourTexture;
    int count = 0;
    unsafe void OnCameraFrameEventReceived(ARCameraFrameEventArgs eventArgs)
    {
      statusText.text = "Got image " + count;
      count++;

      // From: https://docs.unity3d.com/Packages/com.unity.xr.arfoundation@4.0/manual/cpu-camera-image.html
      if (!cameraManager.TryAcquireLatestCpuImage(out XRCpuImage image))
            return;

      var conversionParams = new XRCpuImage.ConversionParams {
            inputRect = new RectInt(0, 0, image.width, image.height),
            outputDimensions = new Vector2Int(image.width / 2, image.height / 2),
            outputFormat = TextureFormat.RGBA32,
            transformation = XRCpuImage.Transformation.MirrorY
        };
      int size = image.GetConvertedDataSize(conversionParams);
      var buffer = new NativeArray<byte>(size, Allocator.Temp);
      image.Convert(conversionParams, new System.IntPtr(buffer.GetUnsafePtr()), buffer.Length);
      image.Dispose();
      if (colourTexture != null)
      {
        Destroy (colourTexture);
        colourTexture = null;
      }
      colourTexture = new Texture2D(conversionParams.outputDimensions.x, conversionParams.outputDimensions.y, conversionParams.outputFormat, false);
      colourTexture.LoadRawTextureData(buffer);
      colourTexture.Apply();

      buffer.Dispose();
    }

    public void toggleOcclusion ()
    {
      occlusionManager.enabled = !occlusionManager.enabled;
    }
    
    int scanCount = 0;
    public void saveScan ()
    {
      string fn = "";
      do
      {
        fn = Application.persistentDataPath + "/" + "scan" + scanCount.ToString("D4");
        scanCount++;
      }
      while (File.Exists(fn + ".obj"));
      statusText.text = "Exporting: " + fn;
      
      Texture2D depthTexture = null;
      Texture2D texture = occlusionManager.environmentDepthTexture;
      if (texture != null)
      {
        RenderTexture tmp = RenderTexture.GetTemporary (texture.width, texture.height, 0, RenderTextureFormat.RHalf, RenderTextureReadWrite.Linear);
        Graphics.Blit(texture, tmp);
        RenderTexture previous = RenderTexture.active;
        RenderTexture.active = tmp;
        depthTexture = new Texture2D(texture.width, texture.height, TextureFormat.RHalf, false);
        depthTexture.ReadPixels(new Rect(0, 0, tmp.width, tmp.height), 0, 0);
        depthTexture.Apply();
        Debug.Log ("Pixe " + depthTexture.GetPixel (0, 0) + " " + depthTexture.GetPixel (50, 20));
        RenderTexture.active = previous;
        RenderTexture.ReleaseTemporary(tmp);        
      }
      
      if (savedColourTexture != null)
      {
        Destroy (savedColourTexture);
        savedColourTexture = null;
      }
      savedColourTexture = new Texture2D (colourTexture.width, colourTexture.height, colourTexture.format, false);
      savedColourTexture.SetPixels (colourTexture.GetPixels ());
      savedColourTexture.Apply ();
      colourMaterial.mainTexture = savedColourTexture;
      createMesh (depthTexture);
      exportObj (fn, depthTexture);
      
      Destroy (depthTexture);
    }
    
    private Mesh mesh;
    private Vector3[] vertices;
    private Vector2[] uv;
    private int[] triangles;
    
    void createMesh(Texture2D texture)
    {
        mesh = new Mesh();
        targetObject.GetComponent<MeshFilter>().mesh = mesh;

        int width = 640;
        int height = 360;
        
        if (texture != null)
        {
          width = texture.width;
          height = texture.height;
        }
        
        vertices = new Vector3[width * height];
        uv = new Vector2[width * height];
        triangles = new int[6 * ((width - 1) * (height - 1))];

        int triangleIndex = 0;
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                int index = (y * width) + x;

                float depth = 0.0f;
                if (texture != null)
                {
                  depth = 5.0f * texture.GetPixel (x, y).r;
                }
                vertices[index] = new Vector3(x, -y, depth);
                uv[index] = new Vector2(1.0f - ((float)x / (float)width), ((float)y / (float)height));

                // Skip the last row/col
                if (x != (width - 1) && y != (height - 1))
                {
                    int topLeft = index;
                    int topRight = topLeft + 1;
                    int bottomLeft = topLeft + width;
                    int bottomRight = bottomLeft + 1;

                    triangles[triangleIndex++] = topLeft;
                    triangles[triangleIndex++] = topRight;
                    triangles[triangleIndex++] = bottomLeft;
                    triangles[triangleIndex++] = bottomLeft;
                    triangles[triangleIndex++] = topRight;
                    triangles[triangleIndex++] = bottomRight;
                }
            }
        }

        mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        mesh.vertices = vertices;
        mesh.uv = uv;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
    }
    
    void exportObj(string filenameBase, Texture2D texture)
    {
        Debug.Log("Saving scan");

        // Write obj file
        using (System.IO.StreamWriter file = new System.IO.StreamWriter(filenameBase + ".obj"))
        {
            file.WriteLine("mtllib " + filenameBase + ".mtl");
            file.WriteLine("o mesh");
            foreach (Vector3 v in vertices)
            {
                file.WriteLine("v " + v.x + " " + v.y + " " + v.z);
            }
            foreach (Vector2 v in uv)
            {
                file.WriteLine("vt " + v.x + " " + v.y);
            }
            foreach (Vector3 v in mesh.normals)
            {
                file.WriteLine("vn " + v.x + " " + v.y + " " + v.z);
            }
            file.WriteLine("usemtl meshMaterial");
            file.WriteLine("s off");
            for (int i = 0; i < triangles.Length; i += 3)
            {
                file.WriteLine("f " + (1 + triangles[i + 0]) + "/" + (1 + triangles[i + 0]) + "/" + (1 + triangles[i + 0]) + " " +
                                      (1 + triangles[i + 1]) + "/" + (1 + triangles[i + 1]) + "/" + (1 + triangles[i + 1]) + " " +
                                      (1 + triangles[i + 2]) + "/" + (1 + triangles[i + 2]) + "/" + (1 + triangles[i + 2]));
            }
        }

        // Write mtl file
        using (System.IO.StreamWriter file = new System.IO.StreamWriter(filenameBase + ".mtl"))
        {
            file.WriteLine("newmtl meshMaterial");
            file.WriteLine("o mesh");
            file.WriteLine("Ns 96.078431");
            file.WriteLine("Ka 1.000000 1.000000 1.000000");
            file.WriteLine("Kd 0.640000 0.640000 0.640000");
            file.WriteLine("Ks 0.500000 0.500000 0.500000");
            file.WriteLine("Ke 0.000000 0.000000 0.000000");
            file.WriteLine("Ni 1.000000");
            file.WriteLine("d 1.000000");
            file.WriteLine("illum 2");
            file.WriteLine("map_Kd " + filenameBase + ".png");
        }
        
        if (colourTexture != null)
        {
          byte [] bytes = ImageConversion.EncodeToPNG (colourTexture);
          File.WriteAllBytes (filenameBase + ".png", bytes);
        }
    }
    
}
