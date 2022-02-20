using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShowVertices : MonoBehaviour
{
    void OnDrawGizmosSelected()
    {
       Mesh m = GetComponent <MeshFilter> ().mesh;
       Vector3 [] v = m.vertices;
       int vlen = v.Length;

       for (int i = 0; i < vlen; i++)
       {
         Gizmos.color = Color.yellow;
         Gizmos.DrawSphere (transform.TransformPoint (v[i]), 0.01f);
       }
    }
}
