using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

// Make sure all objects that need to be persisted have a persistable component.
public class PersistScene : MonoBehaviour
{
    [Tooltip ("A root object for the part of the scene which contains the persistables")]
    public Transform persistablesRoot;
    public GameObject template;
    private string persistFilename = "persist.txt";
    
    public class PersistStructure
    {
        [SerializeField]
        public int ID;
        [SerializeField]
        public Vector3 position;
        [SerializeField]
        public Quaternion rotation;
        [SerializeField]
        public Vector3 scale;
    }
    
    public void persist ()
    {
        StreamWriter file = new StreamWriter (Application.persistentDataPath + "/" + persistFilename, false);
        Persistable [] obj = persistablesRoot.GetComponentsInChildren <Persistable> ();
        Debug.Log ("Pe " + obj.Length + " " + persistablesRoot.gameObject.name);
        
        PersistStructure pdata = new PersistStructure ();
        foreach (Persistable p in obj)
        {
            GameObject g = p.gameObject;
            pdata.ID = g.GetInstanceID ();
            // Local transforms are used, to ignore scene root changes.
            // This will need to be done explicitly if hierarchies of objects under the scene root are used.
            pdata.position = g.transform.localPosition;
            pdata.rotation = g.transform.localRotation;
            pdata.scale = g.transform.localScale;
            Debug.Log (p.gameObject.name);
//            file.WriteLine (g.GetInstanceID () + " " + g.transform.position);
            file.WriteLine (JsonUtility.ToJson (pdata));
        }
        file.Close ();
    }
    
    public void unpersist ()
    {
        try
        {
            StreamReader file = new StreamReader (Application.persistentDataPath + "/" + persistFilename);
            string line;
            PersistStructure pdata;
            while ((line = file.ReadLine ()) != null)
            {
                Debug.Log ("P read " + line);
                pdata = JsonUtility.FromJson <PersistStructure> (line);
                // FIXME: check for existing object with the same ID, so unpersist can be called without duplicating.
                GameObject g = Instantiate (template);
                g.transform.SetParent (persistablesRoot);
                g.transform.localPosition = pdata.position;
                g.transform.localRotation = pdata.rotation;
                g.transform.localScale = pdata.scale;
            }
            file.Close ();
        }
        catch (System.Exception)
        {
            // file does not exist yet.
        }
    }    
}
