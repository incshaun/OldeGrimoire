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
        Debug.Log ("Capture started");
        for (int i = 0; i < phaseImages.Length; i++)
        {
          Camera.main.targetTexture = phaseImages[i];
          structuredLightMaterial.SetFloat ("_Phase", i * Mathf.PI * 2.0f / phaseImages.Length);
          yield return null; // complete render cycle.
          Debug.Log ("Screen A " + Screen.width);
        }
        Camera.main.targetTexture = null;
        structuredLightMaterial.SetFloat ("_Phase", 0.0f);
        
        t = new ThreePhase ();
        t.unwrapPhase (phaseImages[0], phaseImages[1], phaseImages[2]);
    }
   
    bool startReg = false;
    int regCount = 0;
    Vector3 [] regLoc = 
    { 
//        new Vector3 (-0.5f, 0.5f, -0.5f), 
//        new Vector3 (0.5f, 0.5f, -0.5f), 
        new Vector3 (-1.0f, -1.0f, 1.0f), 
        new Vector3 (-0.5f, 2.0f, 1.0f), 
        new Vector3 (1.9f, 2.5f, 1.0f), 
//        new Vector3 (1.0f, 1.0f, -1.5f), 
        new Vector3 (-0.7f, -1.0f, -1.5f), 
        new Vector3 (0.6f, -1.0f, 0.5f),         
        new Vector3 (0.0f, -1.0f, 0.0f), 
    };
//    Vector2 [] mapLoc;
    
    // Measured data, for testing during development.
    Vector2 [] mapLoc =
    {
        new Vector2 (0.3652f, 0.3711f),
        new Vector2 (0.4277f, 0.8008f),
        new Vector2 (0.7988f, 0.8789f),
        new Vector2 (0.3496f, 0.1836f),
        new Vector2 (0.5918f, 0.3438f),
        new Vector2 (0.5059f, 0.3125f)
    };
    
     GameObject ro = null;
    public GameObject regTemplate;
     
    void Update()
    {
      if (Input.GetKeyDown (KeyCode.Space))
      {
          StartCoroutine (captureAndUnwrap ());
//         t = new ThreePhase ();
//         t.findMatrix ();
      }
      
      if (Input.GetKeyDown (KeyCode.LeftAlt))
      {
          regCount = 0;
  //        mapLoc = new Vector2 [regLoc.Length];
  //        startReg = true;
          t.findMatrix (regLoc, mapLoc, scanObject);
      }
      
      if (startReg)
      {
          if (ro == null)
          {
              int i = regCount % regLoc.Length;
              ro = Instantiate (regTemplate, regLoc[i], Quaternion.identity);
              
          }
          
          
          if (Input.GetMouseButtonDown(0))
          {
          Camera.main.targetTexture = phaseImages[0];
              Vector3 p = Input.mousePosition;
              mapLoc[regCount] = new Vector2 (p.x / Screen.width, p.y / Screen.height);
Debug.Log ("Point: " + regCount + " " + mapLoc[regCount].ToString ("F4"));              
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
