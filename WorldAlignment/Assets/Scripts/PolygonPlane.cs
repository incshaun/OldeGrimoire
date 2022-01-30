using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PolygonPlane : MonoBehaviour
{
  private Transform origin;
  private Transform forward;
  private Transform right;
  
  private void updateMesh ()
  {
    Vector3[] vertices = new Vector3[3];
    Vector2[] uvs = new Vector2[3];
    int[] triangles = new int[3];
      
    vertices[0] = transform.InverseTransformPoint (origin.position);
    vertices[1] = transform.InverseTransformPoint (right.position);
    vertices[2] = transform.InverseTransformPoint (forward.position);
    
    uvs[0] = new Vector2 (0, 0);
    uvs[1] = new Vector2 (1, 0);
    uvs[2] = new Vector2 (0, 1);
    
    triangles[0] = 0;
    triangles[1] = 1;
    triangles[2] = 2;
    
    Mesh m = GetComponent <MeshFilter> ().mesh;
    m.Clear ();
    m.vertices = vertices;
    m.uv = uvs;
    m.triangles = triangles;
    m.RecalculateNormals();
  }
  
  public void setCorners (GameObject o, GameObject f, GameObject r)
  {
      origin = o.transform;
      forward = f.transform;
      right = r.transform;
      
      transform.rotation = Quaternion.identity;
      transform.position = Vector3.zero;
      updateMesh ();
  }
  
  public void getCorners (out GameObject o, out GameObject f, out GameObject r)
  {
      o = origin.gameObject;
      f = forward.gameObject;
      r = right.gameObject;
  }
  
  void Update ()
  {
      // FIXME: only needs to be done, when corner markers send an update event.
      updateMesh ();
  }
}
