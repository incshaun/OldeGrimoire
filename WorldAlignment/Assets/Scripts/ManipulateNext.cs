using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TMPro;

public class ManipulateNext : MonoBehaviour
{
    public Transform sceneRoot;
    
    public TextMeshPro label;
    
    private bool selectNext = false;
    
    private bool resetCoords = false;
//     private Matrix4x4 initial;
//     private Matrix4x4 final;
//     private              Vector3 orgpos;
//     private                  Quaternion orgq;
//     private              Vector3 orgs;
    
    // With some elements for debounce.
    //     private bool heldInitial = false;
    //     private bool heldAndReleased = false;
    
    private Transform heldObject = null;
    private Transform heldObjectClone = null;
//     private Transform originalParent = null;
    
    [Tooltip ("Controller input used to trigger selection/deselection")]
    public OVRInput.RawButton button = OVRInput.RawButton.LThumbstickUp;
    
    public void activateNext ()
    {
        selectNext = true;
        label.text = "Click to select closest object";
    }
    
    public void activateReset ()
    {
        selectNext = true;
        resetCoords = true;
        label.text = "Click to relocate closest object";
    }
    
    public void handleControllerButton ()
    {
        Debug.Log ("Controller button pressed");
        
        if ((selectNext) && (heldObject == null))
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
            heldObject = bestObj;
            Debug.Log ("Found " + heldObject +"xx");
            
            heldObjectClone = Instantiate (heldObject); // clone it, for purposes of moving.
            
            if ((heldObject != null) && (heldObject != sceneRoot))
            {
//                 if (resetCoords)
//                 {
//                     initial = heldObject.localToWorldMatrix;
//                     orgpos = heldObject.localPosition;
//                     orgq = heldObject.localRotation;
//                     orgs = heldObject.localScale;
//                 }
                
//                 originalParent = heldObject.parent;
                heldObjectClone.SetParent (transform, false);
                heldObjectClone.localPosition = Vector3.zero;
                heldObjectClone.localRotation = Quaternion.identity;
                if (heldObjectClone.GetComponent <MeshRenderer> () != null)
                {
                  heldObjectClone.GetComponent <MeshRenderer> ().material.color = new Color (0.8f, 0.3f, 0.5f, 0.3f);
                }
                Debug.Log ("At " + heldObject.position +"xx");
                
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
            if (heldObject != null)
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
                
                label.text = "Object placed";
                
                if (resetCoords)
                {
                    Matrix4x4 initial = heldObject.localToWorldMatrix;
                    Matrix4x4 final = heldObjectClone.localToWorldMatrix;
                    Matrix4x4 locToGlob = sceneRoot.transform.localToWorldMatrix * final * initial.inverse;
                    Vector3 pos;
                    Quaternion q;
                    Vector3 s;
                    pos = locToGlob.MultiplyPoint (Vector3.zero);
                    q = locToGlob.rotation;
                    s = locToGlob.lossyScale;
                    
                    sceneRoot.position = pos;
                    sceneRoot.rotation = q;
                    sceneRoot.localScale = s;
                    Debug.Log ("Inverse " + initial + " " + final + " " + locToGlob + " " + pos + " " + q + " " + s + " " + sceneRoot + heldObject.localPosition + " " + heldObject.localRotation + " " + heldObject.localScale);
                    
                    label.text = "Local to global mapping updated";
                    resetCoords = false;
                }
                else
                {
                    heldObject.position = heldObjectClone.position;
                    heldObject.rotation = heldObjectClone.rotation;
                    heldObject.localScale = heldObjectClone.localScale;
                }
                
                heldObject = null;
                Destroy (heldObjectClone.gameObject);
                
                // Update the scene layout now that something has changed.
                if (GetComponent <PersistScene> () != null)
                {
                    GetComponent <PersistScene> ().persist ();
                }
            }
        }
        
    }
    
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
    }
}
