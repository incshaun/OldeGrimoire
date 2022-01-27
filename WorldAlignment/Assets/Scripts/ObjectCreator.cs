using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectCreator : MonoBehaviour
{
    public GameObject objectTemplate;
    public Transform sceneRoot;
    
    public void createObject ()
    {
        GameObject o = Instantiate (objectTemplate, transform.position, transform.rotation);
        o.transform.SetParent (sceneRoot);
    }
}
