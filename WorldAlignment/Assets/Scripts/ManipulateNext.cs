using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TMPro;

public class ManipulateNext : MonoBehaviour
{
    [Tooltip ("The template for anchor points")]
    public GameObject objectTemplate;
    [Tooltip ("The template for polygon planes")]
    public GameObject polygonPlaneTemplate;
    [Tooltip ("The root object for the scene (for manipulating global coordinates)")]
    public Transform sceneRoot;
    
    [Tooltip ("A label to show state of the controls")]    
    public TextMeshPro label;
    
    private enum ActionMode { None, CreatePoint, Move, Reset, PlacePlane, LinkPoints };
    
    private ActionMode action = ActionMode.None;

    public class Anchor
    {
        public Transform anchor;
        public Transform sceneElement;
    };

    private Transform activeObject = null;
    private Dictionary <Transform, Anchor> anchorPoints = new Dictionary <Transform, Anchor> ();
    
//     private Transform heldObject = null;
//     private Transform heldObjectClone = null;
//     private Transform originalParent = null;
    
    [Tooltip ("Controller input used to trigger selection/deselection")]
    public OVRInput.RawButton button = OVRInput.RawButton.LThumbstickUp;
    
    private GameObject instantiateTemplate (Vector3 offset)
    {
        GameObject o = Instantiate (objectTemplate, transform.position + offset, transform.rotation);
        o.transform.SetParent (sceneRoot);
        return o;
    }
    
    public void createPoint ()
    {
      instantiateTemplate (Vector3.zero);
    }
    
    public void activateNext ()
    {
        action = ActionMode.Move;
        label.text = "Click to select closest object";
    }
    
    public void activateReset ()
    {
        action = ActionMode.Reset;
        label.text = "Click to relocate closest object";
    }
    
    public void placePlane ()
    {
        action = ActionMode.PlacePlane;
        label.text = "Click to create plane";        
    }
    
    Transform [] linkElements = new Transform [3];
    int linkCount = 0;
    public void linkPoints ()
    {
        linkCount = 0;
        action = ActionMode.LinkPoints;
        label.text = "Click on points to make plane";        
    }
    
    public void updateTransformation ()
    {
        if (activeObject != null)
        {
            Vector3 [] local = new Vector3 [anchorPoints.Keys.Count];
            Vector3 [] global = new Vector3 [anchorPoints.Keys.Count];
            int i = 0;
            foreach (KeyValuePair <Transform, Anchor> entry in anchorPoints)
            {
                local[i] = entry.Value.anchor.position;
                global[i] = entry.Value.sceneElement.localPosition;
                
                i++;
            }
            // This aligns multiple anchors, but typically requires at least 3 non-colinear 
            // points to get a reliable fit.
            Matrix4x4 locToGlob = ICP.BestFit (global, local);
            
            // This just references relative to a single anchor point, but takes orientation
            // into account.
    //                     Matrix4x4 initial = activeObject.localToWorldMatrix;
    //                     Matrix4x4 final = anchorPoints[activeObject].anchor.localToWorldMatrix;
    //                     Matrix4x4 locToGlob = sceneRoot.transform.localToWorldMatrix * final * initial.inverse;
            Vector3 pos;
            Quaternion q;
            Vector3 s;
            pos = locToGlob.MultiplyPoint (Vector3.zero);
            q = locToGlob.rotation;
            s = locToGlob.lossyScale;
            
            sceneRoot.position = pos;
            sceneRoot.rotation = q;
            sceneRoot.localScale = s;
            
            foreach (KeyValuePair <Transform, Anchor> entry in anchorPoints)
            {
                entry.Value.anchor.GetComponent <LineRenderer> ().positionCount = 2;
                entry.Value.anchor.GetComponent <LineRenderer> ().SetPosition (0, entry.Value.anchor.position);
                entry.Value.anchor.GetComponent <LineRenderer> ().SetPosition (1, entry.Value.sceneElement.position);                
            }
            
        }
    }
    
    // Bit of a hack, to provide visual appearance to a anchor. Should really be
    // separated out to the class of object.
    private void setColour (Transform p, Color c)
    {
      if (p.Find ("ShapeCube").GetComponent <MeshRenderer> () != null)
      {
        p.transform.Find ("ShapeCube").GetComponent <MeshRenderer> ().material.color = c;
      }
    }
    
    private Transform findClosestManipulable ()
    {
        // find closest object, and attach.
        // Select all objects in the scene.
        Transform [] obj = sceneRoot.GetComponentsInChildren <Transform> ();
        // Find the closest.
        float bestDistance = 0.0f;
        Transform bestObj = null;
        foreach (Transform t in obj)
        {
            float distance = Vector3.Distance (transform.position, t.position);
            if (((bestObj == null) || (distance < bestDistance)) && (t.gameObject.GetComponent <Manipulable> () != null))
            {
                bestObj = t;
                bestDistance = distance;
            }
        }
        return bestObj;
    }
    
    public void handleControllerButton ()
    {
        Debug.Log ("Controller button pressed");
        
        switch (action)
        {
            case ActionMode.Move:
            {
                if (activeObject == null)
                {
                    activeObject = findClosestManipulable ();
                    if ((activeObject != null) && (activeObject != sceneRoot))
                    {
                        activeObject.SetParent (transform, false);
                        activeObject.localPosition = Vector3.zero;
                        activeObject.localRotation = Quaternion.identity;
                        label.text = "Place object";
                    }
                    else
                    {
                        label.text = "No object found";
                    }
                }
                else // make sure release requires a new event
                {
                    if (activeObject != null)
                    {
                        activeObject.SetParent (sceneRoot);
                        label.text = "Object placed";
                    }
                        
                    activeObject = null;
                }
            }
            break;
            
            case ActionMode.Reset:
            {
                if (activeObject == null)
                {
                    activeObject = findClosestManipulable ();
                    if ((activeObject != null) && (activeObject != sceneRoot))
                    {
                        // register anchor if it doesn't already exist.
                        if (!anchorPoints.ContainsKey (activeObject))
                        {
                            Anchor a = new Anchor ();
                            a.anchor = Instantiate (activeObject);
                            a.sceneElement = activeObject;
                            anchorPoints[activeObject] = a;
                        }

                        anchorPoints[activeObject].anchor.SetParent (transform, false);
                        anchorPoints[activeObject].anchor.localPosition = Vector3.zero;
                        anchorPoints[activeObject].anchor.localRotation = Quaternion.identity;
                        setColour (anchorPoints[activeObject].anchor, new Color (0.8f, 0.3f, 0.5f, 0.3f));

                        label.text = "Place object";
                    }
                    else
                    {
                        label.text = "No object found";
                    }
                }
                else // make sure release requires a new event
                {
                    if (activeObject != null)
                    {
                        anchorPoints[activeObject].anchor.SetParent (null);
                        label.text = "Object placed";
                        
                        updateTransformation ();
                            
                        label.text = "Local to global mapping updated";
                        
                        activeObject = null;
                    }
                }
            }
            break;
     
            case ActionMode.PlacePlane:
            {
                GameObject o = instantiateTemplate (Vector3.zero);
                GameObject f = instantiateTemplate (new Vector3 (0.0f, 0.0f, 0.1f));
                GameObject r = instantiateTemplate (new Vector3 (0.1f, 0.0f, 0.0f));
       
                GameObject plane = Instantiate (polygonPlaneTemplate, transform.position, transform.rotation);
                plane.GetComponent <PolygonPlane> ().setCorners (o, f, r);
                plane.transform.SetParent (sceneRoot);
            }
            break;
            
            case ActionMode.LinkPoints:
            {
                Transform t = findClosestManipulable ();
                bool found = false;
                for (int i = 0; i < linkCount; i++)
                {
                    found = found | (linkElements[i] == t);
                }
                if (!found)
                {
                    setColour (t, new Color (0.2f, 0.7f, 0.2f, 0.3f));                        
                    linkElements[linkCount++] = t;
                }
                
                if (linkCount == 3)
                {
                  GameObject plane = Instantiate (polygonPlaneTemplate, Vector3.zero, Quaternion.identity);
                  plane.GetComponent <PolygonPlane> ().setCorners (linkElements[0].gameObject, linkElements[1].gameObject, linkElements[2].gameObject);
                  plane.transform.SetParent (sceneRoot);
                  setColour (linkElements[0], new Color (1.0f, 1.0f, 1.0f, 0.5f));                        
                  setColour (linkElements[1], new Color (1.0f, 1.0f, 1.0f, 0.5f));                        
                  setColour (linkElements[2], new Color (1.0f, 1.0f, 1.0f, 0.5f));                        
                  linkCount = 0;
                }
            }
            break;
            
            default:
            break;
        }
        
        // Update the scene layout now that something has changed.
        if (GetComponent <PersistScene> () != null)
        {
            GetComponent <PersistScene> ().persist ();
        }
    }
                
                
                
                
                
                
                
                
                
                
                
                
                
                
    /*            
        if (activeObject == null)
        {
            // find closest object, and attach.
            // Select all objects in the scene.
            Transform [] obj = sceneRoot.GetComponentsInChildren <Transform> ();
            Debug.Log ("Havw " + obj.Length);
            // Find the closest.
            float bestDistance = 0.0f;
            Transform bestObj = null;
            foreach (Transform t in obj)
            {
                float distance = Vector3.Distance (transform.position, t.position);
                if (((bestObj == null) || (distance < bestDistance)) && (t.gameObject.GetComponent <Persistable> () != null))
                {
                    bestObj = t;
                    bestDistance = distance;
                }
            }
            activeObject = bestObj;
//             heldObject = bestObj;
//             Debug.Log ("Found " + heldObject +"xx");
            
            
//             heldObjectClone = Instantiate (heldObject); // clone it, for purposes of moving.
            
//             if ((heldObject != null) && (heldObject != sceneRoot))
            if ((activeObject != null) && (activeObject != sceneRoot))
            {
                // register anchor if it doesn't already exist.
                if (!anchorPoints.ContainsKey (activeObject))
                {
                    Anchor a = new Anchor ();
                    a.anchor = Instantiate (activeObject);
                    a.sceneElement = activeObject;
                    anchorPoints[activeObject] = a;
                }
//                 if (resetCoords)
//                 {
//                     initial = heldObject.localToWorldMatrix;
//                     orgpos = heldObject.localPosition;
//                     orgq = heldObject.localRotation;
//                     orgs = heldObject.localScale;
//                 }

                anchorPoints[activeObject].anchor.SetParent (transform, false);
                anchorPoints[activeObject].anchor.localPosition = Vector3.zero;
                anchorPoints[activeObject].anchor.localRotation = Quaternion.identity;
                if (anchorPoints[activeObject].anchor.transform.Find ("ShapeCube").GetComponent <MeshRenderer> () != null)
                {
                  anchorPoints[activeObject].anchor.transform.Find ("ShapeCube").GetComponent <MeshRenderer> ().material.color = new Color (0.8f, 0.3f, 0.5f, 0.3f);
                }

//                 originalParent = heldObject.parent;
//                 heldObjectClone.SetParent (transform, false);
//                 heldObjectClone.localPosition = Vector3.zero;
//                 heldObjectClone.localRotation = Quaternion.identity;
//                 if (heldObjectClone.GetComponent <MeshRenderer> () != null)
//                 {
//                   heldObjectClone.GetComponent <MeshRenderer> ().material.color = new Color (0.8f, 0.3f, 0.5f, 0.3f);
//                 }
//                 Debug.Log ("At " + heldObject.position +"xx");
                
                selectNext = false;
//                 heldInitial = true;
//                 heldAndReleased = false;
                
                label.text = "Place object";
            }
            else
            {
                label.text = "No object found";
            }
        }
        else // make sure release requires a new event
        {
//             if (heldObject != null)
            if (activeObject != null)
            {
//                 // drop object.
//                 if (resetCoords)
//                 {
//                     final = heldObject.localToWorldMatrix;
//                 }              
                
//                 // Put it back under the original parent.
//                 heldObject.transform.SetParent (originalParent);
                
//                 if (resetCoords)
//                 {
//                     heldObject.localPosition = orgpos;
//                     heldObject.localRotation = orgq;
//                     heldObject.localScale = orgs;
//                     Debug.Log ("Hel at : " + heldObject.localPosition + " " + heldObject.localRotation + " " + heldObject.localScale);
//                 }
                
//                 heldInitial = false;
//                 heldAndReleased = false;
                selectNext = false;
                
                anchorPoints[activeObject].anchor.SetParent (null);
                label.text = "Object placed";
                
                if (resetCoords)
                {
                    updateTransformation ();
                    
                    label.text = "Local to global mapping updated";
                    resetCoords = false;
                }
                else
                {
//                     heldObject.position = heldObjectClone.position;
//                     heldObject.rotation = heldObjectClone.rotation;
//                     heldObject.localScale = heldObjectClone.localScale;
                    activeObject.position = anchorPoints[activeObject].anchor.position;
                    activeObject.rotation = anchorPoints[activeObject].anchor.rotation;
                    activeObject.localScale = anchorPoints[activeObject].anchor.localScale;
                    
                    Destroy (anchorPoints[activeObject].anchor.gameObject);
                    anchorPoints.Remove (activeObject);
                }
                
                activeObject = null;
//                Destroy (heldObjectClone.gameObject);
                
                // Update the scene layout now that something has changed.
                if (GetComponent <PersistScene> () != null)
                {
                    GetComponent <PersistScene> ().persist ();
                }
            }
        }
        
    }
    */
    private bool buttonDown = false;
    void createControllerEvents ()
    {
        // Make up for apprent lack of events in OVRInput, by polling button.
        if (OVRInput.Get (button) && !buttonDown)
        {
            buttonDown = true;
            handleControllerButton (); // invoke event.
        }              
        if (!OVRInput.Get (button) && buttonDown)
        {
            buttonDown = false; // debounce.
        }
    }
    
    //     void updateStateMachine ()
    //     {
    //         if ((selectNext) && (heldObject == null))
    //         {
    //           if (OVRInput.Get (button))
    //           { 
    //               // find closest object, and attach.
    //               // Select all objects in the scene.
    //               Transform [] obj = sceneRoot.GetComponentsInChildren <Transform> (    );
    //               Debug.Log ("Havw " + obj.Length);
    //               // Find the closest.
    //               heldObject = obj.Aggregate ((curMin, x) => (curMin == null || (Vector3.Distance (transform.position, x.position) < Vector3.Distance (transform.position, curMin.position)) ? x : curMin));
    //               Debug.Log ("Found " + heldObject +"xx");
    // 
    //               if (heldObject != null)
    //               {
    //                 if (resetCoords)
    //                 {
    //                     initial = heldObject.localToWorldMatrix;
    //                     orgpos = heldObject.localPosition;
    //                     orgq = heldObject.localRotation;
    //                     orgs = heldObject.localScale;
    //                 }
    //                   
    //                 originalParent = heldObject.parent;
    //                 heldObject.SetParent (transform, false);
    //                 heldObject.localPosition = Vector3.zero;
    //                 heldObject.localRotation = Quaternion.identity;
    //               Debug.Log ("At " + heldObject.position +"xx");
    //                 
    //                 selectNext = false;
    //                 heldInitial = true;
    //                 heldAndReleased = false;
    //                 
    //                 label.text = "Place object";
    //               }
    //               else
    //               {
    //                 label.text = "No object found";
    //               }
    //           }
    //         }
    //         
    //         if (heldInitial) // holding, but released button.
    //         {
    //           if (!OVRInput.Get (button))
    //           { 
    //               heldInitial = false;
    //               heldAndReleased = true;
    //               label.text = "Place object and click to release";
    //           }
    //         }
    // 
    //         if (heldAndReleased)
    //         {
    //           if (OVRInput.Get (button))
    //           { 
    //               // drop object.
    //                 if (resetCoords)
    //                 {
    //                     final = heldObject.localToWorldMatrix;
    //                 }              
    //               
    //               // Put it back under the original parent.
    //               heldObject.transform.SetParent (originalParent);
    //               
    //               if (resetCoords)
    //               {
    //                   heldObject.localPosition = orgpos;
    //                   heldObject.localRotation = orgq;
    //                   heldObject.localScale = orgs;
    //                   Debug.Log ("Hel at : " + heldObject.localPosition + " " + heldObject.localRotation + " " + heldObject.localScale);
    //               }
    //               
    //               heldInitial = false;
    //               heldAndReleased = false;
    //               selectNext = false;
    //               
    //               label.text = "Object placed";
    //               
    //               if (resetCoords)
    //               {
    //                   Matrix4x4 locToGlob = final * initial.inverse;
    //                   Vector3 pos;
    //                   Quaternion q;
    //                   Vector3 s;
    //                   pos = locToGlob.MultiplyPoint (Vector3.zero);
    //                   q = locToGlob.rotation;
    //                   s = locToGlob.lossyScale;
    //                   
    //                   sceneRoot.transform.position = pos;
    //                   sceneRoot.transform.rotation = q;
    //                   sceneRoot.transform.localScale = s;
    //                   Debug.Log ("Inverse " + initial + " " + final + " " + locToGlob + " " + pos + " " + q + " " + s + " " + sceneRoot.transform + orgpos + " " + orgq + " " + orgs + " " + heldObject.localPosition + " " + heldObject.localRotation + " " + heldObject.localScale);
    //                   
    //                   label.text = "Local to global mapping updated";
    //                   resetCoords = false;
    //               }
    //               
    //               heldObject = null;
    //           }
    //         }
    //     }
    
    void Start ()
    {
        label.text = "";
        
        // Restore any previous scene elements.
        if (GetComponent <PersistScene> () != null)
        {
            GetComponent <PersistScene> ().unpersist ();
        }
        
    }
    
    void Update ()
    {
        createControllerEvents ();
        
        updateTransformation ();
    }
}
