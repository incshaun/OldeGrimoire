using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Test harness, for testing event response functions without
// having to deploy to device each time.
public class TestMenu : MonoBehaviour
{
    // Link these to key scene components, to simulate events by manipulating these directly.
    public GameObject head;
    public GameObject handLeft;
    public GameObject handRight;
    public ObjectCreator creator;
    public ManipulateNext manipulate;
    
    public delegate void StepAction ();
    public class StepDescription
    {
        public string description;
        public StepAction action;
        
        public StepDescription (string d, StepAction a)
        {
            description = d;
            action = a;
        }
    }
    private int step = 0;
    private List <StepDescription> steps = new List <StepDescription> ();
    
    void Start ()
    {
        // initialize head and hands to typical positions.
        head.transform.position = new Vector3 (0.0f, 1.5f, 0.0f);
        handLeft.transform.position = new Vector3 (-0.1f, 1.3f, 0.3f);
        handRight.transform.position = new Vector3 (0.1f, 1.3f, 0.3f);
        
        steps.Add (new StepDescription ("Place first object", placeFirst));
        steps.Add (new StepDescription ("Place second object", placeSecond));
        steps.Add (new StepDescription ("Grab first object", resetCoordinatesGrab));
//         steps.Add (new StepDescription ("Drop object and reset", resetCoordinatesPlace));
        steps.Add (new StepDescription ("Rotate object", replaceRotated));
    }
    
    void Update ()
    {
        if (Input.GetKeyDown (KeyCode.Space))
        {
            Debug.Log ("Activating step: " + step + " to " + steps[step].description);
            
            steps[step].action ();
            
            step = (step + 1) % steps.Count;
        }
    }
    
    public void handleEvent ()
    {
        Debug.Log ("Event happened");
    }
    
    void placeFirst ()
    {
        handLeft.transform.position = new Vector3 (-0.2f, 1.3f, 0.5f);
        handLeft.transform.rotation = Quaternion.AngleAxis (0.0f, new Vector3 (0.0f, 1.0f, 0.0f));
        creator.createObject (); // invoke create object.
    }
    
    void placeSecond ()
    {
//         handLeft.transform.position = new Vector3 (0.4f, 1.3f, 0.7f);
        handLeft.transform.position = new Vector3 (0.4f, 1.3f, 0.5f);
        handLeft.transform.rotation = Quaternion.AngleAxis (0.0f, new Vector3 (0.0f, 1.0f, 0.0f));
//         handLeft.transform.rotation = Quaternion.AngleAxis (33.0f, new Vector3 (0.5f, 0.6f, 0.7f));
        creator.createObject (); // invoke create object.
    }

    void resetCoordinatesGrab ()
    {
        handLeft.transform.position = new Vector3 (-0.2f, 1.3f, 0.4f);
//         handLeft.transform.position = new Vector3 (-0.2f, -1.3f, 0.4f);
        handLeft.transform.rotation = Quaternion.AngleAxis (0.0f, new Vector3 (0.0f, 1.0f, 0.0f));
        manipulate.activateNext (); // select move object on menu.
//         manipulate.activateReset (); // select reset coordinates on menu.
        manipulate.handleControllerButton (); // click controller.
    }
    
    void resetCoordinatesPlace ()
    {
        handLeft.transform.position = new Vector3 (0.4f, 1.3f, 0.5f);
        handLeft.transform.rotation = Quaternion.AngleAxis (45.0f, new Vector3 (0.0f, 1.0f, 0.0f));
//         handLeft.transform.rotation = Quaternion.AngleAxis (33.0f, new Vector3 (0.5f, 0.6f, 0.7f));
//        handLeft.transform.localScale = new Vector3 (0.5f, 0.3f, 0.1f);
//         manipulate.activateNext (); // select move object on menu.
        manipulate.handleControllerButton (); // click controller.
    }
    
    void replaceRotated ()
    {
        handLeft.transform.position = new Vector3 (-0.2f, 1.4f, 0.5f);
        handLeft.transform.rotation = Quaternion.AngleAxis (45.0f, new Vector3 (0.0f, 1.0f, 0.0f));
        manipulate.handleControllerButton (); // click controller.
    }
    
    
}
