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
  
  public bool project;
  
  public bool simulate = true;
  public Vector3 simCamPosition;
  
  public GameObject display;
  
  // Current projecting from:
  //   camera position: xc, yc, zc
  // onto screen defined by plane: ax + by + cd + d = 0
  // projecting point xp, yp, zp
  
  // Ray from camera through point is: (x, y, z) = (xc, yc, zc) + t [(xp, yp, zp) - (xc, yc, zc)]
  // Intersects plane at:
  //   a (xc + t (xp - xc)) + b (yc + t (yp - yc)) + c (zc + t (zp - zc)) + d = 0
  //   t = n / m
  //     where n = (a xc + b yc + c zc + d)
  //     and   m = a (xc - xp) + b (yc - yp) + c (zc - zp)
  //             = a xc + b yc + c zc - (a xp + b yp + c zp)
  //           m - n = -a xp - b yp - c zp - d
  
  // Screen coordinates:
  //   xs = xc + t [xp - xc]  (and for y and z)
  //      = 1/m [m xc + n (xp - xc)]
  //      = 1/m [(a xc + b yc + c zc - (a xp + b yp + c zp)) xc + (a xc + b yc + c zc + d) (xp - xc)]
  //      = 1/m [(a xc^2 + b yc xc + c zc xc - (a xc xp + b xc yp + c xc zp)) + (a xc xp + b yc xp + c zc xp + d xp) - (a xc^2 + b yc xc + c zc xc + d xc )]
  //      = 1/m [-d xc + (c zc + b yc + d) xp - b xc yp - c xc zp)]
  
  //   ys = yc + t [yp - yc]
  //      = 1/m [m yc + n (yp - yc)]
  //      = 1/m [(m - n) yc + n yp]
  //      = 1/m [(m - n) yc + n yp]
  //      = 1/m [ -d yc -a yc xp + (a xc + c zc + d) yp - c yc zp]
  
  //   xs = [ (b yc + c zc + d)  -b xc             -c xc             -d xc                ] [xp]
  //   ys = [ -a yc              (a xc + c zc + d) -c yc             -d yc                ] [yp]
  //   zs = [ -a zc              -b zc             (a xc + b yc + d) -d zc                ] [zp]
  //   ws = [ -a                 -b                -c                (a xc + b yc + c zc) ] [wp]
  
  // a = 0, b = 0, c = 1, d = 0
  
  void Start()
  {
    faceSource.faceHandler += drawFace;
  }
  
  
  void oldStart () {
    Camera cam = GetComponent <Camera> ();
    
    Matrix4x4 old = cam.projectionMatrix;
    Debug.Log ("Old: " + old);
    
    Vector3 camPos = cam.transform.position;
    float xc = camPos.x;
    float yc = camPos.y;
    float zc = camPos.z;
    xc = 0;
    yc = 0;
    zc = -1;
    //  cam.transform.position = new Vector3 (0, 0, 0);
    //  cam.transform.rotation = Quaternion.identity;
    Vector3 normal = display.transform.up;
    Vector3 dispPos = display.transform.position;
    float a = normal.x;
    float b = normal.y;
    float c = normal.z;
    float d = -(a * dispPos.x + b * dispPos.y + c * dispPos.z);
    
    print ("C: " + xc + "," + yc + "," + zc + "   Plane " + a + " : " + b + " : " + c + " : " + d);
    
    float near = 0.1f;
    float far = 10.0f;
    
    Matrix4x4 anamorphicProjection = new Matrix4x4 ();
    anamorphicProjection[0, 0] = b * yc + c * zc + d;
    anamorphicProjection[0, 1] = -b * xc;
    anamorphicProjection[0, 2] = -c * xc;
    anamorphicProjection[0, 3] = -d * xc;
    
    anamorphicProjection[1, 0] = -a * yc;
    anamorphicProjection[1, 1] = a * xc + c * zc + d;
    anamorphicProjection[1, 2] = -c * yc;
    anamorphicProjection[1, 3] = -d * yc;
    
    anamorphicProjection[2, 0] = -a * zc;
    anamorphicProjection[2, 1] = -b * zc;
    anamorphicProjection[2, 2] = a * xc + b * yc + d - far - near;
    anamorphicProjection[2, 3] = -d * zc;
    
    anamorphicProjection[3, 0] = -a;
    anamorphicProjection[3, 1] = -b;
    anamorphicProjection[3, 2] = -c * far * near;
    anamorphicProjection[3, 3] = a * xc + b * yc + c * zc;
    
    cam.projectionMatrix = anamorphicProjection;
    
    Debug.Log ("New: " + cam.projectionMatrix);
  }
  static Matrix4x4 PerspectiveOffCenter(float left, float right, float bottom, float top, float near, float far)
    {
      Debug.Log ("LR " + left + " " + right);
        float x = 2.0F * near / (right - left);
        float y = 2.0F * near / (top - bottom);
        float a = (right + left) / (right - left);
        float b = (top + bottom) / (top - bottom);
        float c = -(far + near) / (far - near);
        float d = -(2.0F * far * near) / (far - near);
        float e = -1.0F;
        Matrix4x4 m = new Matrix4x4();
        m[0, 0] = x;
        m[0, 1] = 0;
        m[0, 2] = a;
        m[0, 3] = 0;
        m[1, 0] = 0;
        m[1, 1] = y;
        m[1, 2] = b;
        m[1, 3] = 0;
        m[2, 0] = 0;
        m[2, 1] = 0;
        m[2, 2] = c;
        m[2, 3] = d;
        m[3, 0] = 0;
        m[3, 1] = 0;
        m[3, 2] = e;
        m[3, 3] = 0;
        return m;
    }
    
  public void Update ()
  {
    if (simulate)
    {
      drawFace (null);
    }
  }
    
  public void drawFace (ARFace face)
  {
    if (face != null)
    {
      // The face comes in with an unexpected transformation; possibly due to a mirror
      // because of the use of the front facing camera. 
      transform.position = new Vector3 (face.transform.position.x, face.transform.position.y, -face.transform.position.z);
      transform.rotation = new Quaternion (-face.transform.rotation.x, face.transform.rotation.y, -face.transform.rotation.z, face.transform.rotation.w);
    }
 
//     transform.position = new Vector3 (0.02f, -0.005f, -0.35f);
    
    if (simulate)
    {
      float px = simCamPosition.x;
      float py = simCamPosition.y;
      float pz = simCamPosition.z;
    transform.position = new Vector3 (px, py, pz);
    transform.forward = -Vector3.forward;
    transform.up = Vector3.up;

    // Size of the display device.
    float sizex = 0.1f;
    float sizey = 0.05f;

    // OpenGL projection matrix.
    // Camera at origin, looking down -z axis.
    // near plane at -near.
    float near = 0.05f;
    float far = 10.0f;
    
    // display screen at pz on the z axis, centered at -px, -py
    // calculate position of display screen projected onto the near clipping plane.
    float left = ((-px - sizex) * (near / -pz));
    float right = ((-px + sizex) * (near / -pz));
    float bottom = ((-py - sizey) * (near / -pz));
    float top = ((-py + sizey) * (near / -pz));
    
        Matrix4x4 anamorphicProjection = new Matrix4x4 ();

    anamorphicProjection = PerspectiveOffCenter (left, right, 
                                                 bottom, top,
                                                 near, far);
    // 0.02 -0.005 -0.35
    // 3.47 0 0 0 
    // 0 6.1 0 0
    // 0 0 -1 -0.02 0
    // 0 0 -1 0
//    anamorphicProjection = GetComponent <Camera> ().projectionMatrix;
//     anamorphStatusText.text = "M: " + anamorphicProjection[0, 0] + ", " + 
//      + anamorphicProjection[0, 1] + ", " + 
//      + anamorphicProjection[0, 2] + ", " + 
//      + anamorphicProjection[0, 3] + ",,,, " + 
//      + anamorphicProjection[1, 0] + ", " + 
//      + anamorphicProjection[1, 1] + ", " + 
//      + anamorphicProjection[1, 2] + ", " + 
//      + anamorphicProjection[1, 3] + ",,,, " + 
//      + anamorphicProjection[2, 0] + ", " + 
//      + anamorphicProjection[2, 1] + ", " + 
//      + anamorphicProjection[2, 2] + ", " + 
//      + anamorphicProjection[2, 3] + ",,,, " + 
//      + anamorphicProjection[3, 0] + ", " + 
//      + anamorphicProjection[3, 1] + ", " + 
//      + anamorphicProjection[3, 2] + ", " + 
//      + anamorphicProjection[3, 3] + ", " 
//     ;
    
    GetComponent <Camera> ().nearClipPlane = near;
    GetComponent <Camera> ().farClipPlane = far;
    GetComponent <Camera> ().projectionMatrix = anamorphicProjection;
    
    }
    
    // We now make some assumptions. 
    // 1. The phone (display) is fixed at the origin, facing down the +ve z axis.
    // 2. The view point is the face, whose position and orientation is as just defined.
    // 3. We have an object of interest in the space between view point and display. We now
    // can calculate the projection of this object onto the display.
    if (project)
    {
    anamorphStatusText.text = transform.position.ToString ("F4") + " " + transform.forward.ToString ("F4");     
//     Vector3 camPos = transform.position;
//     float xc = camPos.x;
//     float yc = camPos.y;
//     float zc = camPos.z;

      float px = -face.transform.position.x;
      float py = face.transform.position.y;
      float pz = -face.transform.position.z;
    transform.position = new Vector3 (px, py, pz);
    transform.forward = Vector3.forward;
    transform.up = Vector3.up;

//     px = -px;
//     py = -py;
    // Size of the display device.
    float sizex = 0.1f;
    float sizey = 0.05f;

    // OpenGL projection matrix.
    // Camera at origin, looking down -z axis.
    // near plane at -near.
    float near = 0.05f;
    float far = 10.0f;
    
    // display screen at pz on the z axis, centered at -px, -py
    // calculate position of display screen projected onto the near clipping plane.
    float left = ((-px - sizex) * (near / -pz));
    float right = ((-px + sizex) * (near / -pz));
    float bottom = ((-py - sizey) * (near / -pz));
    float top = ((-py + sizey) * (near / -pz));
    
        Matrix4x4 anamorphicProjection = new Matrix4x4 ();

    anamorphicProjection = PerspectiveOffCenter (left, right, 
                                                 bottom, top,
                                                 near, far);

    GetComponent <Camera> ().nearClipPlane = near;
    GetComponent <Camera> ().farClipPlane = far;
/*    
    Vector3 normal = Vector3.forward;
    Vector3 dispPos = Vector3.zero;
    float a = normal.x;
    float b = normal.y;
    float c = normal.z;
    float d = -(a * dispPos.x + b * dispPos.y + c * dispPos.z);
    
    anamorphStatusText.text += "C: " + xc + "," + yc + "," + zc + "   Plane " + a + " : " + b + " : " + c + " : " + d;
    
    float near = 0.1f;
    float far = 10.0f;
    
    Matrix4x4 anamorphicProjection = new Matrix4x4 ();
    anamorphicProjection[0, 0] = b * yc + c * zc + d;
    anamorphicProjection[0, 1] = -b * xc;
    anamorphicProjection[0, 2] = -c * xc;
    anamorphicProjection[0, 3] = -d * xc;
    
    anamorphicProjection[1, 0] = -a * yc;
    anamorphicProjection[1, 1] = a * xc + c * zc + d;
    anamorphicProjection[1, 2] = -c * yc;
    anamorphicProjection[1, 3] = -d * yc;
    
    anamorphicProjection[2, 0] = -a * zc;
    anamorphicProjection[2, 1] = -b * zc;
    anamorphicProjection[2, 2] = a * xc + b * yc + d - far - near;
    anamorphicProjection[2, 3] = -d * zc;
    
    anamorphicProjection[3, 0] = -a;
    anamorphicProjection[3, 1] = -b;
    anamorphicProjection[3, 2] = -c * far * near;
    anamorphicProjection[3, 3] = a * xc + b * yc + c * zc;

    float sizex = 0.1f;
    float sizey = 0.05f;
//     anamorphicProjection = PerspectiveOffCenter (transform.position.x - sizex, transform.position.x + sizex, 
//                                                  transform.position.y - sizey, transform.position.y + sizey,
//                                                  0.05f, 10.0f);
    anamorphicProjection = PerspectiveOffCenter (-sizex, sizex, 
                                                 -sizey, sizey,
                                                 0.05f, 10.0f);
    // 0.02 -0.005 -0.35
    // 3.47 0 0 0 
    // 0 6.1 0 0
    // 0 0 -1 -0.02 0
    // 0 0 -1 0
//    anamorphicProjection = GetComponent <Camera> ().projectionMatrix;
    anamorphStatusText.text += "M: " + anamorphicProjection[0, 0] + ", " + 
     + anamorphicProjection[0, 1] + ", " + 
     + anamorphicProjection[0, 2] + ", " + 
     + anamorphicProjection[0, 3] + ",,,, " + 
     + anamorphicProjection[1, 0] + ", " + 
     + anamorphicProjection[1, 1] + ", " + 
     + anamorphicProjection[1, 2] + ", " + 
     + anamorphicProjection[1, 3] + ",,,, " + 
     + anamorphicProjection[2, 0] + ", " + 
     + anamorphicProjection[2, 1] + ", " + 
     + anamorphicProjection[2, 2] + ", " + 
     + anamorphicProjection[2, 3] + ",,,, " + 
     + anamorphicProjection[3, 0] + ", " + 
     + anamorphicProjection[3, 1] + ", " + 
     + anamorphicProjection[3, 2] + ", " + 
     + anamorphicProjection[3, 3] + ", " 
    ;*/
    
    GetComponent <Camera> ().projectionMatrix = anamorphicProjection;
      
    }
    
    
    
  }
  
}
