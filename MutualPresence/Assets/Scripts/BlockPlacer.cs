using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class BlockPlacer : NetworkBehaviour {
  
  public GameObject blockTemplate;
  public float blockSize = 0.1f;
  
  private GameObject parentObject;
  
  private GameObject blockHint;
  public Material hintMaterial;
  private GazeController controller;
  
  private GameObject currentActive = null;
  
  // Have to let the button go before it will reactivate.
  private bool debouncePrimary = false;        
  private bool debounceSecondary = false;
  
  void Start ()
  {
    // Indirect find, since AROverlay is not active at this stage and Find won't work directly.
    parentObject = GameObject.Find ("Marker").transform.Find ("AROverlay").gameObject;
    
    controller = GetComponent <GazeController> ();
    
    blockHint = Instantiate (blockTemplate);
    blockHint.transform.localScale = new Vector3 (blockSize, blockSize, blockSize);
    blockHint.GetComponent <MeshRenderer> ().material = hintMaterial;
    blockHint.SetActive (false);
    blockHint.GetComponent <Collider> ().enabled = false;
    blockHint.transform.SetParent (parentObject.transform);
    blockHint.tag = "Untagged";
  }
  
  Vector3 quantize (Vector3 p)
  {
    return blockSize * new Vector3 (Mathf.Round (p.x / blockSize), Mathf.Round (p.y / blockSize), Mathf.Round (p.z / blockSize));
  }
  
  [Command]
  void CmdCreateBlock (Vector3 position, float blockSize)
  {
    GameObject g = Instantiate (blockTemplate);
    g.transform.SetParent (parentObject.transform);
    g.transform.localScale = new Vector3 (blockSize, blockSize, blockSize);
    g.transform.localPosition = position;
    g.transform.localRotation = Quaternion.identity;
    NetworkServer.Spawn (g);
    Debug.Log ("Server put at" + (100.0f * position) + " " + (100.0f * g.transform.localPosition) + " " + (100.0f * g.transform.position));
  }
  
  [Command]
  void CmdRemoveBlock (GameObject g)
  {
    NetworkServer.Destroy (g);
  }
  
  void Update () {
    if (isLocalPlayer)
    {
      // Only reset the debounce controls when the buttons are not being pressed.
      if (!controller.getPrimary ())
      {
        debouncePrimary = false;
      }
      if (!controller.getSecondary ())
      {
        debounceSecondary = false;
      }
      
      RaycastHit hit;
      if (Physics.Raycast (controller.getRay (), out hit))
      {
        if (hit.collider.tag == "ActiveBlock")
        {
          if (currentActive != null)
          {
            currentActive.GetComponent <MeshRenderer> ().material.color = new Color (1,1,1);
          }
          currentActive = hit.collider.gameObject;
          currentActive.GetComponent <MeshRenderer> ().material.color = new Color (1,0,0);
          Vector3 hintPosition = quantize (parentObject.transform.InverseTransformPoint (hit.point + hit.normal * 0.5f * blockSize));
          blockHint.transform.localPosition = hintPosition;
          blockHint.transform.localRotation = Quaternion.identity;
          blockHint.SetActive (true);
        }
      }
      else
      {
        blockHint.SetActive (false);
      }
      
      // Add a new block.
      if (blockHint.activeSelf && controller.getPrimary () && !debouncePrimary)
      {
        Debug.Log ("Place at " + (100.0f * blockHint.transform.localPosition) + " " + (100.0f * blockHint.transform.position));
        CmdCreateBlock (blockHint.transform.localPosition, blockSize);
        debouncePrimary = true;
      }
      
      // Delete a block.
      if (controller.getSecondary () && !debounceSecondary)
      {
        CmdRemoveBlock (currentActive);
        currentActive = null;
        blockHint.SetActive (false);
        debounceSecondary = true;
      }
    }
  }
}
