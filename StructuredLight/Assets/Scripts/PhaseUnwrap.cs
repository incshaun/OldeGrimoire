using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhaseUnwrap: MonoBehaviour
{
    public Material structuredLightMaterial;
    
    public RenderTexture [] phaseImages;
    
    ThreePhase t;
    public MeshFilter scanObject;
    
    // TODO: force main camera to have same aspect as rendertextures.
    
    private IEnumerator captureAndUnwrap ()
    {
        // Render the camera view, updating the phase of the shader each time.
        for (int i = 0; i < phaseImages.Length; i++)
        {
            Camera.main.targetTexture = phaseImages[i];
            structuredLightMaterial.SetFloat ("_Phase", i * Mathf.PI * 2.0f / phaseImages.Length);
            yield return null; // complete render cycle.
        }
        Camera.main.targetTexture = null;
        structuredLightMaterial.SetFloat ("_Phase", 0.0f);
        
        t = new ThreePhase ();
        t.unwrapPhase (phaseImages[0], phaseImages[1], phaseImages[2]);
    }
    
    // To register camera view and 3D environment, define a number of
    // points on surfaces in the scene, and provide an interface to
    // click on each and record their positions.
    Vector3 [] regLoc = 
    { 
        new Vector3 (-0.4f, 0.4f, -0.5f), 
        new Vector3 (0.4f, 0.4f, -0.5f), 
        new Vector3 (-1.0f, -1.0f, 1.0f), 
        new Vector3 (-0.5f, 2.0f, 1.0f), 
        new Vector3 (1.9f, 2.5f, 1.0f), 
        //        new Vector3 (1.0f, 1.0f, -1.5f), 
        new Vector3 (-0.7f, -1.0f, -1.5f), 
        new Vector3 (0.6f, -1.0f, 0.5f),         
        new Vector3 (0.0f, -1.0f, 0.0f), 
    };
    
    void Update()
    {
        // Capture images under various phases of lighting, and unwrap.
        if (Input.GetKeyDown (KeyCode.Space))
        {
            StartCoroutine (captureAndUnwrap ());
        }
        
        // Calibrate phase against 3D scene, and generate mesh.
        if (Input.GetKeyDown (KeyCode.LeftAlt))
        {
            Vector2 [] mapLoc = new Vector2 [regLoc.Length];
            
            for (int i = 0; i < regLoc.Length; i++)
            {
                Debug.Log ("  " + Camera.main.WorldToScreenPoint (regLoc[i]));
                Vector3 p = Camera.main.WorldToScreenPoint (regLoc[i]);
                mapLoc[i] = new Vector2 (p.x / Screen.width, p.y / Screen.height);
            }
            
            t.findMatrix (regLoc, mapLoc, scanObject);
        }
    }
}
