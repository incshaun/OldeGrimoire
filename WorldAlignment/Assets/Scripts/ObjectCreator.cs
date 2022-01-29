using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectCreator : MonoBehaviour
{
    public GameObject objectTemplate;
    public Transform sceneRoot;
    
    public GameObject polygonPlaneTemplate;
    
    private GameObject instantiateTemplate (Vector3 offset)
    {
        GameObject o = Instantiate (objectTemplate, transform.position + offset, transform.rotation);
        o.transform.SetParent (sceneRoot);
        return o;
    }
    
    public void createObject ()
    {
        instantiateTemplate (Vector3.zero);
    }
    
    public void createPlane ()
    {
       GameObject o = instantiateTemplate (Vector3.zero);
       GameObject f = instantiateTemplate (new Vector3 (0.0f, 0.0f, 0.1f));
       GameObject r = instantiateTemplate (new Vector3 (0.1f, 0.0f, 0.0f));
       
      GameObject pp = Instantiate (polygonPlaneTemplate, transform.position, transform.rotation);
       pp.GetComponent <PolygonPlane> ().setCorners (o, f, r);
    }
}
