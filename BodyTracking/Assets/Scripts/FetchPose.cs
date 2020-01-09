using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using System;
using System.Runtime.InteropServices;
using System.Threading;

// Retrieve pose information from the native posenet/tensorflow lite facilities.
// Also includes some visual elements, to show camera feed and monitor retrieved
// values.
public class FetchPose : MonoBehaviour
{
  [DllImport ("poseinterface")]
  unsafe private static extern void initPose ();
  [DllImport ("poseinterface")]
  unsafe private static extern int computePose (IntPtr texture, int w, int h, float * results);
  
  public Material camTexMaterial;
  
  private WebCamTexture webcamTexture;
  
  private int numPointsInPose = 17;
  
  public GameObject pointMarkerTemplate;
  
  private GameObject [] markers;
  
  // Start is called before the first frame update
  void Start()
  {
    webcamTexture = new WebCamTexture ();
    
    camTexMaterial.mainTexture = webcamTexture;
    webcamTexture.Play ();
    
    initPose ();
    
    markers = new GameObject [numPointsInPose];
  }
  
  // Retrieve the pose from the native library. This returns
  // as an array of floats, containing x, y, and confidence
  // values for each point.
  unsafe float [] retrievePose ()
  {
      NativeArray <float> pose = new NativeArray <float> (numPointsInPose * 3, Allocator.Temp);
      int result = computePose (webcamTexture.GetNativeTexturePtr (), webcamTexture.width, webcamTexture.height, (float *) NativeArrayUnsafeUtility.GetUnsafePtr (pose));
      Debug.Log ("Got result " + result + " " + pose[0] + " " + pose[1]);
      return pose.ToArray ();
  }
  
  // Update is called once per frame
  void Update()
  {
    if (Input.GetAxis ("Fire1") > 0)
    {
      float [] pose = retrievePose ();
      
      for (int i = 0; i < numPointsInPose; i++)
      {
        if (markers[i] == null)
        {
          markers[i] = Instantiate (pointMarkerTemplate);
          markers[i].transform.SetParent (this.transform, false);
        }
         
        markers[i].transform.localPosition = new Vector3 (- (10.0f * pose[i * 3 + 0] - 5.0f), 0.0f, - (10.0f * pose[i * 3 + 1] - 5.0f));
        if (pose[i * 3 + 2] < 0.0f)
        {
          markers[i].SetActive (false);
        }
        else
        {
          markers[i].SetActive (true);
        }
        //Debug.Log (pose[i * 3 + 0] + " " + pose[i * 3 + 1] + " " + pose[i * 3 + 2]);
      }
    }
    
  }
}
