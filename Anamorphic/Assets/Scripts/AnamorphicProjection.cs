using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnamorphicProjection : MonoBehaviour {

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
  
  // a = 0, b = 0, c = 1, d = -sz
  
	void Start () {
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
	
}
