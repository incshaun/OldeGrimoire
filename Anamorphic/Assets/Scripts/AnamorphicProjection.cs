using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using TMPro;

public class AnamorphicProjection : MonoBehaviour {
  
  [Tooltip ("This is the object with the FaceProperties component, that connects to the face tracker from AR Foundation")]
  public FaceProperties faceSource;
  
  [Tooltip ("Text element for status and debugging information")]
  public TextMeshProUGUI anamorphStatusText;
  
  void Start()
  {
    faceSource.faceHandler += drawFace;
  }
  
  // See the unity example on the camera's perspective matrix.
  static Matrix4x4 PerspectiveOffCenter(float left, float right, float bottom, float top, float near, float far)
  {
    float x = 2.0F * near / (right - left);
    float y = 2.0F * near / (top - bottom);
    float a = (right + left) / (right - left);
    float b = (top + bottom) / (top - bottom);
    float c = -(far + near) / (far - near);
    float d = -(2.0F * far * near) / (far - near);
    float e = -1.0F;
    Matrix4x4 m = new Matrix4x4();
    m[0, 0] = x; m[0, 1] = 0; m[0, 2] = a; m[0, 3] = 0;
    m[1, 0] = 0; m[1, 1] = y; m[1, 2] = b; m[1, 3] = 0;
    m[2, 0] = 0; m[2, 1] = 0; m[2, 2] = c; m[2, 3] = d;
    m[3, 0] = 0; m[3, 1] = 0; m[3, 2] = e; m[3, 3] = 0;
    return m;
  }
  
  public void drawFace (ARFace face)
  {
//     anamorphStatusText.text = transform.position.ToString ("F4") + " " + transform.forward.ToString ("F4");     
    
    float px = -face.transform.position.x;
    float py = face.transform.position.y;
    float pz = -face.transform.position.z;
    transform.position = new Vector3 (px, py, pz);
    // It doesnt' matter which way the viewer is looking. The
    // display will still show what is visible in the direction
    // of the display.
    transform.forward = Vector3.forward;
    transform.up = Vector3.up;
    
    // Size of the display device.
    float sizex = 0.1f;
    float sizey = 0.05f;
    
    float near = 0.05f;
    float far = 10.0f;
    
    // calculate position of display screen projected onto the near clipping plane.
    float left = ((-px - sizex) * (near / -pz));
    float right = ((-px + sizex) * (near / -pz));
    float bottom = ((-py - sizey) * (near / -pz));
    float top = ((-py + sizey) * (near / -pz));
    
    Matrix4x4 anamorphicProjection = new Matrix4x4 ();
    
    anamorphicProjection = PerspectiveOffCenter (left, right, 
                                                bottom, top,
                                                near, far);
    
    GL.invertCulling = false;
    GetComponent <Camera> ().nearClipPlane = near;
    GetComponent <Camera> ().farClipPlane = far;
    GetComponent <Camera> ().projectionMatrix = anamorphicProjection;
  }
}
