﻿// using UnityEngine;
// using System.Collections;
// using Windows.Kinect;
// using System.IO;
// using UnityEditor;
// 
// public class ObjectScan : MonoBehaviour
// {
//     private KinectSensor sensor;
//     private CoordinateMapper mapper;
// 
//     private Mesh mesh;
//     private Vector3[] vertices;
//     private Vector2[] uv;
//     private int[] triangles;
//     
//     // Size of Unity mesh is limited, so down sample to fit this limit.
//     private const int _DownsampleSize = 2;
//     private const double _DepthScale = 0.12f;
//     
//     public ColorSourceManager colorManager;
//     public DepthSourceManager depthManager;
// 
//     private float timeDelay = -1.0f;
//     private bool timerStarted = false;
// 
//     public float timeBeforeCapture = 0.1f;
// 
//     void Start()
//     {
//         sensor = KinectSensor.GetDefault();
//         if (sensor != null)
//         {
//             mapper = sensor.CoordinateMapper;
//             var frameDesc = sensor.DepthFrameSource.FrameDescription;
// 
//             CreateMesh(frameDesc.Width / _DownsampleSize, frameDesc.Height / _DownsampleSize);
// 
//             if (sensor.IsOpen)
//             {
//                 sensor.Open();
//             }
//         }
//     }
// 
//     void CreateMesh(int width, int height)
//     {
//         mesh = new Mesh();
//         GetComponent<MeshFilter>().mesh = mesh;
// 
//         vertices = new Vector3[width * height];
//         uv = new Vector2[width * height];
//         triangles = new int[6 * ((width - 1) * (height - 1))];
// 
//         int triangleIndex = 0;
//         for (int y = 0; y < height; y++)
//         {
//             for (int x = 0; x < width; x++)
//             {
//                 int index = (y * width) + x;
// 
//                 vertices[index] = new Vector3(x, -y, 0);
//                 uv[index] = new Vector2(((float)x / (float)width), ((float)y / (float)height));
// 
//                 // Skip the last row/col
//                 if (x != (width - 1) && y != (height - 1))
//                 {
//                     int topLeft = index;
//                     int topRight = topLeft + 1;
//                     int bottomLeft = topLeft + width;
//                     int bottomRight = bottomLeft + 1;
// 
//                     triangles[triangleIndex++] = topLeft;
//                     triangles[triangleIndex++] = topRight;
//                     triangles[triangleIndex++] = bottomLeft;
//                     triangles[triangleIndex++] = bottomLeft;
//                     triangles[triangleIndex++] = topRight;
//                     triangles[triangleIndex++] = bottomRight;
//                 }
//             }
//         }
// 
//         mesh.vertices = vertices;
//         mesh.uv = uv;
//         mesh.triangles = triangles;
//         mesh.RecalculateNormals();
//     }
// 
//     void Update()
//     {
//         if (sensor == null)
//         {
//             return;
//         }
//         gameObject.GetComponent<MeshRenderer> ().material.mainTexture = colorManager.GetColorTexture();
//         RefreshData(depthManager.GetData(), colorManager.ColorWidth, colorManager.ColorHeight);
// 
//         if (Input.GetAxis("Fire1") > 0.0f)
//         {
//             timeDelay = timeBeforeCapture;
//             timerStarted = true;
//         }
//         if (timeDelay > 0.0f)
//         {
//             timeDelay -= Time.deltaTime;
//         }
// 
//         if (timerStarted && (timeDelay < 0.0f))
//         {
//             timerStarted = false;
// 
//             // For continuous capture. Recommend timeBeforeCapture of at least 1 s.
//             //timeDelay = timeBeforeCapture;
//             //timerStarted = true;
// 
//             int count = 0;
//             string fn = "";
//             do
//             {
//                 fn = "scan" + count.ToString("D4");
//                 count++;
//             }
//             while (File.Exists(fn + ".obj"));
//             Debug.Log("Exporting: " + fn);
//             exportObj(fn);
//             EditorApplication.Beep();
//         }
//     }
//     
//     private void RefreshData(ushort[] depthData, int colorWidth, int colorHeight)
//     {
//         var frameDesc = sensor.DepthFrameSource.FrameDescription;
//         
//         ColorSpacePoint[] colorSpace = new ColorSpacePoint[depthData.Length];
//         mapper.MapDepthFrameToColorSpace(depthData, colorSpace);
//         
//         for (int y = 0; y < frameDesc.Height - (_DownsampleSize - 1); y += _DownsampleSize)
//         {
//             for (int x = 0; x < frameDesc.Width - (_DownsampleSize - 1); x += _DownsampleSize)
//             {
//                 int indexX = x / _DownsampleSize;
//                 int indexY = y / _DownsampleSize;
//                 int width = frameDesc.Width / _DownsampleSize;
//                 int smallIndex = indexY * width + indexX;
//                 
//                 double avg = GetAvg(depthData, x, y, frameDesc.Width, frameDesc.Height);
//                 
//                 avg = avg * _DepthScale;
//                 
//                 vertices[smallIndex].z = (float)avg;
//                 
//                 // Update UV mapping with CDRP
//                 var colorSpacePoint = colorSpace[(y * frameDesc.Width) + x];
//                 uv[smallIndex] = new Vector2(colorSpacePoint.X / colorWidth, colorSpacePoint.Y / colorHeight);
//             }
//         }
//         
//         mesh.vertices = vertices;
//         mesh.uv = uv;
//         mesh.triangles = triangles;
//         mesh.RecalculateNormals();
//     }
//     
//     private double GetAvg(ushort[] depthData, int x, int y, int width, int height)
//     {
//         double sum = 0.0;
//         int count = 0;
//         for (int y1 = y; y1 < y + _DownsampleSize; y1++)
//         {
//             for (int x1 = x; x1 < x + _DownsampleSize; x1++)
//             {
//                 if ((x < width) && (y < height))
//                 {
//                     int fullIndex = (y1 * width) + x1;
// 
//                     if (depthData[fullIndex] == 0)
//                     {
//                         sum += 4500;
//                     }
//                     else
//                     {
//                         sum += depthData[fullIndex];
//                     }
//                     count++;
//                 }
//             }
//         }
// 
//         return sum / count;
//     }
// 
//     void exportObj(string filenameBase)
//     {
//         Debug.Log("Saving scan");
// 
//         // Write obj file
//         using (System.IO.StreamWriter file = new System.IO.StreamWriter(filenameBase + ".obj"))
//         {
//             file.WriteLine("mtllib " + filenameBase + ".mtl");
//             file.WriteLine("o mesh");
//             foreach (Vector3 v in vertices)
//             {
//                 file.WriteLine("v " + v.x + " " + v.y + " " + v.z);
//             }
//             foreach (Vector2 v in uv)
//             {
//                 file.WriteLine("vt " + v.x + " " + v.y);
//             }
//             foreach (Vector3 v in mesh.normals)
//             {
//                 file.WriteLine("vn " + v.x + " " + v.y + " " + v.z);
//             }
//             file.WriteLine("usemtl meshMaterial");
//             file.WriteLine("s off");
//             for (int i = 0; i < triangles.Length; i += 3)
//             {
//                 file.WriteLine("f " + (1 + triangles[i + 0]) + "/" + (1 + triangles[i + 0]) + "/" + (1 + triangles[i + 0]) + " " +
//                                       (1 + triangles[i + 1]) + "/" + (1 + triangles[i + 1]) + "/" + (1 + triangles[i + 1]) + " " +
//                                       (1 + triangles[i + 2]) + "/" + (1 + triangles[i + 2]) + "/" + (1 + triangles[i + 2]));
//             }
//         }
// 
//         // Write mtl file
//         using (System.IO.StreamWriter file = new System.IO.StreamWriter(filenameBase + ".mtl"))
//         {
//             file.WriteLine("newmtl meshMaterial");
//             file.WriteLine("o mesh");
//             file.WriteLine("Ns 96.078431");
//             file.WriteLine("Ka 1.000000 1.000000 1.000000");
//             file.WriteLine("Kd 0.640000 0.640000 0.640000");
//             file.WriteLine("Ks 0.500000 0.500000 0.500000");
//             file.WriteLine("Ke 0.000000 0.000000 0.000000");
//             file.WriteLine("Ni 1.000000");
//             file.WriteLine("d 1.000000");
//             file.WriteLine("illum 2");
//             file.WriteLine("map_Kd " + filenameBase + ".jpg");
//         }
//         File.WriteAllBytes (filenameBase + ".jpg", ImageConversion.EncodeToJPG (colorManager.GetColorTexture()));
//     }
// }
