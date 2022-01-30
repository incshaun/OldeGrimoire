using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using MathNet.Numerics.LinearAlgebra.Single;

// Test harness, for testing event response functions without
// having to deploy to device each time.
public class TestMenu : MonoBehaviour
{
    // Link these to key scene components, to simulate events by manipulating these directly.
    public GameObject head;
    public GameObject handLeft;
    public GameObject handRight;
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
        
//         steps.Add (new StepDescription ("Place first object", placeFirst));
//         steps.Add (new StepDescription ("Place second object", placeSecond));
//         steps.Add (new StepDescription ("Grab first object", resetCoordinatesGrab));
//         steps.Add (new StepDescription ("Drop object and reset", resetCoordinatesPlace));
//         steps.Add (new StepDescription ("Rotate object", replaceRotated));
         steps.Add (new StepDescription ("Create Plane", createPlane));
        
        
//         int order = 5;
//         var matrixA = new DenseMatrix(order, order);
//             matrixA[0, 0] = 1;
//             matrixA[order - 1, order - 1] = 1;
//             for (var i = 1; i < order - 1; i++)
//             {
//                 matrixA[i, i - 1] = 1;
//                 matrixA[i, i + 1] = 1;
//                 matrixA[i - 1, i] = 1;
//                 matrixA[i + 1, i] = 1;
//             }
// 
//             var factorSvd = matrixA.Svd();
//             Debug.Log ("SVD " + matrixA + " " + factorSvd.VT);
//             
// //             ICP a = new ICP ();
// //             Matrix A = (Matrix) Matrix.Build.Dense(7,3,(i,j) => 10*i + j);
// //             Matrix B = (Matrix) Matrix.Build.Dense(7,3,(i,j) => i - 3 *(j+i));
// //             Debug.Log ("AAA " + a.best_fit_transform (A, B));
// //             Debug.Log ("AAAA " + a.findCentroid (A) + " " + A);
// //             Debug.Log ("AAAB " + a.findCentroid (B) + " " + B);
// Vector3 [] A = { new Vector3 (0, 1, 2), new Vector3 (10, 11, 12), new Vector3 (20, 21, 22), new Vector3 (30, 31, 32), new Vector3 (40, 41, 42), new Vector3 (50, 51, 52), new Vector3 (60, 61, 62) };
// Vector3 [] B = { new Vector3 (0, -3, -6), new Vector3 (-2, -5, -8), new Vector3 (-4, -7, -10), new Vector3 (6, -9, -12), new Vector3 (-8, -11, -14), new Vector3 (-10, -13, -16), new Vector3 (-12, -15, -18) };
// for (int i = 0; i < A.Length; i++)
// {
//     B[i] = Quaternion.AngleAxis (23.0f, new Vector3 (1, 0.4f, -0.6f)) * A[i] + new Vector3 (3, 2, 6);
// }
//             
//             Matrix4x4 T = ICP.BestFit (A, B);
//             Debug.Log ("Goh fit " + T);
//             for (int i = 0; i < A.Length; i++)
//             {
//                 Vector3 v = T.MultiplyPoint (A[i]);
//                 Debug.Log ("R: " + A[i] + " " + B[i] + " " + v + " " + Vector3.Distance (v, B[i]));
//             }
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
        manipulate.createPoint (); // invoke create object.
        manipulate.handleControllerButton (); // click controller.
    }
    
    void placeSecond ()
    {
//         handLeft.transform.position = new Vector3 (0.4f, 1.3f, 0.7f);
        handLeft.transform.position = new Vector3 (0.4f, 1.3f, 0.5f);
        handLeft.transform.rotation = Quaternion.AngleAxis (0.0f, new Vector3 (0.0f, 1.0f, 0.0f));
//         handLeft.transform.rotation = Quaternion.AngleAxis (33.0f, new Vector3 (0.5f, 0.6f, 0.7f));
        manipulate.createPoint (); // invoke create object.
        manipulate.handleControllerButton (); // click controller.
    }

    void resetCoordinatesGrab ()
    {
        handLeft.transform.position = new Vector3 (-0.2f, 1.3f, 0.4f);
//         handLeft.transform.position = new Vector3 (-0.2f, -1.3f, 0.4f);
        handLeft.transform.rotation = Quaternion.AngleAxis (0.0f, new Vector3 (0.0f, 1.0f, 0.0f));
//         manipulate.activateNext (); // select move object on menu.
        manipulate.activateReset (); // select reset coordinates on menu.
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
    
    void createPlane ()
    {
        handLeft.transform.position = new Vector3 (-0.2f, 1.3f, 0.5f);
        handLeft.transform.rotation = Quaternion.AngleAxis (0.0f, new Vector3 (0.0f, 1.0f, 0.0f));
        manipulate.placePlane (); // create new triangle.
        manipulate.handleControllerButton (); // click controller.
    }
    
    
}
