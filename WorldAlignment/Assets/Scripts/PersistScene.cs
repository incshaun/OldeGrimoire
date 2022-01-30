using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

// Make sure all objects that need to be persisted have a persistable component.
public class PersistScene : MonoBehaviour
{
    [Tooltip ("A root object for the part of the scene which contains the persistables")]
    public Transform persistablesRoot;
    public GameObject manipulableTemplate;
    public GameObject polygonPlaceTemplate;
    
    private string persistFilename = "persist.txt";
    
    internal enum PersistTypes { Manipulable, PolygonPlane };
    
    internal class PersistStructure
    {
        [System.NonSerialized]
        public GameObject element;
        
        [SerializeField]
        public int ID;
        [SerializeField]
        public PersistTypes type;
        [SerializeField]
        public Vector3 position;
        [SerializeField]
        public Quaternion rotation;
        [SerializeField]
        public Vector3 scale;
        [SerializeField]
        public string typeJSON;
    }

    internal class PolygonPlanePersistStructure
    {
        [SerializeField]
        public int [] manipulables;
    }
    
    // Return the index of g in pdata.
    private int getIndex (PersistStructure [] pdata, GameObject g)
    {
        for (int i = 0; i < pdata.Length; i++)
        {
            if (pdata[i].element == g) return i;
        }
        Debug.Assert (true, "GetIndex failed - major problem");
        return -1;
    }
    
    public void persist ()
    {
        StreamWriter file = new StreamWriter (Application.persistentDataPath + "/" + persistFilename, false);
        Persistable [] obj = persistablesRoot.GetComponentsInChildren <Persistable> ();
        Debug.Log ("Pe " + obj.Length + " " + persistablesRoot.gameObject.name);
        
        PersistStructure [] pdata = new PersistStructure [obj.Length];
        // initial loop assigns identifiers to all elements.
        for (int i = 0; i < obj.Length; i++)
        {
            pdata[i] = new PersistStructure ();
            Debug.Log ("V " + i + " " + obj[i]);
            pdata[i].element = obj[i].gameObject;
            pdata[i].ID = i;
        }
        
        // second loop populates fields, including cross references.
        for (int i = 0; i < obj.Length; i++)
        {
            GameObject g = pdata[i].element;
            
            if (g.GetComponent <Manipulable> () != null)
            {
                pdata[i].type = PersistTypes.Manipulable;
            }
            if (g.GetComponent <PolygonPlane> () != null)
            {
                pdata[i].type = PersistTypes.PolygonPlane;
                PolygonPlanePersistStructure pp = new PolygonPlanePersistStructure ();
                GameObject v0, v1, v2;
                g.GetComponent <PolygonPlane> ().getCorners (out v0, out v1, out v2);
                pp.manipulables = new int [3];
                pp.manipulables[0] = getIndex (pdata, v0);
                pp.manipulables[1] = getIndex (pdata, v1);
                pp.manipulables[2] = getIndex (pdata, v2);
                pdata[i].typeJSON = JsonUtility.ToJson (pp);
            }
            
            // Local transforms are used, to ignore scene root changes.
            // This will need to be done explicitly if hierarchies of objects under the scene root are used.
            pdata[i].position = g.transform.localPosition;
            pdata[i].rotation = g.transform.localRotation;
            pdata[i].scale = g.transform.localScale;
            Debug.Log (obj[i].gameObject.name);
//            file.WriteLine (g.GetInstanceID () + " " + g.transform.position);
            file.WriteLine (JsonUtility.ToJson (pdata[i]));
        }
        file.Close ();
    }
    
    public void unpersist ()
    {
        // Should we clean the scene first?
        try
        {
            StreamReader file = new StreamReader (Application.persistentDataPath + "/" + persistFilename);
            string line;
            PersistStructure pdata;
            List <GameObject> gobs = new List <GameObject> ();
            List <string> typeJSONs = new List <string> ();
            List <PersistTypes> types = new List <PersistTypes> ();
            
            // First iteration to create objects.
            while ((line = file.ReadLine ()) != null)
            {
                Debug.Log ("P read " + line);
                pdata = JsonUtility.FromJson <PersistStructure> (line);
                GameObject g = null;
                switch (pdata.type)
                {
                    case PersistTypes.Manipulable:
                      g = Instantiate (manipulableTemplate);
                      break;
                    case PersistTypes.PolygonPlane:
                      g = Instantiate (polygonPlaceTemplate);
                      break;
                    default:
                      break;
                }
                g.transform.SetParent (persistablesRoot);
                g.transform.localPosition = pdata.position;
                g.transform.localRotation = pdata.rotation;
                g.transform.localScale = pdata.scale;
                
                gobs.Add (g);
                typeJSONs.Add (pdata.typeJSON);
                types.Add (pdata.type);
            }
            file.Close ();
            
            // Second iteration to fill in cross references and custom fields.
            for (int i = 0; i < gobs.Count; i++)
            {
                switch (types[i])
                {
                    case PersistTypes.Manipulable:
                      break;
                    case PersistTypes.PolygonPlane:
                        PolygonPlanePersistStructure pp = JsonUtility.FromJson <PolygonPlanePersistStructure> (typeJSONs[i]);
                        gobs[i].GetComponent <PolygonPlane> ().setCorners (gobs[pp.manipulables[0]], gobs[pp.manipulables[1]], gobs[pp.manipulables[2]]);
                      break;
                    default:
                      break;
                }
            }
        }
        catch (System.Exception)
        {
            // file does not exist yet.
        }
    }    
}
