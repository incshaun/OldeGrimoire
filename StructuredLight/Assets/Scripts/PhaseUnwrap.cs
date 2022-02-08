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
    bool startReg = false;
    int regCount = 0;
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
    Vector2 [] mapLoc;
    
    // Templates for a marker object to be clicked on.
    GameObject ro = null;
    public GameObject regTemplate;
    
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
            regCount = 0;
            mapLoc = new Vector2 [regLoc.Length];
            startReg = true;
        }
        
        if (startReg)
        {
            if (ro == null)
            {
                // create a new marker, if none exists.
                int i = regCount % regLoc.Length;
                ro = Instantiate (regTemplate, regLoc[i], Quaternion.identity);
            }
            
            // record clicks, assuming they are on the target object.
            if (Input.GetMouseButtonDown(0))
            {
                Camera.main.targetTexture = phaseImages[0];
                Vector3 p = Input.mousePosition;
                mapLoc[regCount] = new Vector2 (p.x / Screen.width, p.y / Screen.height);
                Camera.main.targetTexture = null;
                
                Destroy (ro);
                ro = null;
                regCount += 1;
                
                if (regCount >= regLoc.Length)
                {
                    startReg = false;
                    t.findMatrix (regLoc, mapLoc, scanObject);
                }
            }
        }
    }
}
