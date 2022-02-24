using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestMeshAlign : MonoBehaviour
{
    public MeshFilter pointCloudSource;
    
    public GameObject cloudTemplate;
    
    public int pointCloudSize = 100;
    
    private List <Vector3> extractRandomCloud (int n, bool scramble)
    {
        List <Vector3> p = new List <Vector3> ();

        Mesh m = pointCloudSource.GetComponent <MeshFilter> ().mesh;
        Vector3 [] v = m.vertices;
        int vlen = v.Length;
        
        Matrix4x4 randTrans = Matrix4x4.identity;
        if (scramble)
        {
            Vector3 t = Random.insideUnitSphere;
            float angle = Random.Range (0.0f, 360.0f);
            Vector3 direction = Random.insideUnitSphere;
            randTrans.SetTRS (t, Quaternion.AngleAxis (angle, direction), Vector3.one);
            Debug.Log ("Transformation: " + t.ToString ("F4") + "   " + angle.ToString ("F4") + " - " + direction.ToString ("F4") + "\n\n" + randTrans.ToString ("F4"));
        }
        
        for (int i = 0; i < n; i++)
        {
            p.Add (randTrans.MultiplyPoint (v[Random.Range (0, vlen)]));
        }
        
        return p;
    }
    
    private void displayCloud (List <Vector3> p)
    {
        Vector3 [] vertices = p.ToArray ();
        
        GameObject g = Instantiate (cloudTemplate);
        Mesh m = g.GetComponent <MeshFilter> ().mesh;
        m.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        m.vertices = vertices;
        m.triangles = new int [0];
        m.RecalculateNormals();
    }
    
    void Start()
    {
        List <Vector3> p1 = extractRandomCloud (pointCloudSize, false);
        List <Vector3> p2 = extractRandomCloud (pointCloudSize, true);
        
        displayCloud (p1);
        displayCloud (p2);
        
        FastGlobalRegistration fgr = new FastGlobalRegistration ();
        fgr.AddFeature (p1);
        fgr.AddFeature (p2);
        fgr.NormalizePoints();
        fgr.AdvancedMatching();
        fgr.OptimizePairwise(true);

        List <Vector3> p3 = fgr.GetTransformedPoints (p2);
        displayCloud (p3);
    }
}
