using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// Show the view from from an image/texture on a plane
// just in front of the camera. Recommend adding this script to an empty 
// containing any planes in this category.
// Set the plane transform to: rot: 90, 180, 0 and scale to 0.1, 0.1, 0.1
// to achieve a unit size plane facing the z axis. The transform for
// the empty is set by this script.
public class ShowPhysicalCamera : MonoBehaviour {
  
  public Material camTexMaterial;
  public Camera camera;
  
  private WebCamTexture webcamTexture;
  
  void Start () {
    webcamTexture = new WebCamTexture ();
    
    camTexMaterial.mainTexture = webcamTexture;
    webcamTexture.Play ();
  }
  
  void Update ()
  {
    // With thanks: https://answers.unity.com/questions/314049/how-to-make-a-plane-fill-the-field-of-view.html
    float pos = (camera.nearClipPlane + 0.01f);
    
    transform.position = camera.transform.position + camera.transform.forward * pos;
    
    float h = Mathf.Tan (camera.fieldOfView * Mathf.Deg2Rad * 0.5f) * pos * 2.0f;
    
    transform.localScale = new Vector3(h * camera.aspect, h, 1.0f);
  }
}
