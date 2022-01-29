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
    Vector3[] vertices = new Vector3[4];
    Vector2[] uvs = new Vector2[4];
    int[] triangles = new int[6];
      
//     origin.position = new Vector3 (-0.3099f, 0.1220f, 0.5417f);
//     right.position = new Vector3 (0.0349f, -0.0862f, 0.4102f);
//     forward.position = new Vector3 (-0.3332f, -0.3426f, 0.5946f);
//     transform.position = new Vector3 (-0.1938f, -0.1087f, 0.4605f);
//     
    vertices[0] = origin.position - transform.position;
    vertices[1] = right.position - transform.position;
    vertices[2] = (right.position + forward.position - origin.position) - transform.position;
    vertices[3] = forward.position - transform.position;
    
//     Debug.Log ("Mesh " + origin.position.ToString ("F4") + " " + right.position.ToString ("F4") + " " + forward.position.ToString ("F4") + " " + transform.position.ToString ("F4") + " " + vertices[0].ToString ("F4") + " " + vertices[1].ToString ("F4") + " " + vertices[2].ToString ("F4") + " " + vertices[3].ToString ("F4"));
    
    uvs[0] = new Vector2 (0, 0);
    uvs[1] = new Vector2 (1, 0);
    uvs[2] = new Vector2 (1, 1);
    uvs[3] = new Vector2 (0, 1);
    
    triangles[0] = 0;
    triangles[1] = 1;
    triangles[2] = 2;
    triangles[3] = 0;
    triangles[4] = 2;
    triangles[5] = 3;
    
    Mesh m = GetComponent <MeshFilter> ().mesh;
    m.Clear ();
    m.vertices = vertices;
    m.uv = uvs;
    m.triangles = triangles;
    m.RecalculateNormals();
//    GetComponent <MeshFilter> ().mesh = m; 
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
  
  void Update ()
  {
      updateMesh ();
  }
}
