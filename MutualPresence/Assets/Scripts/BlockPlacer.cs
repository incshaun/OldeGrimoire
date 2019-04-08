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
          parentObject = GameObject.Find ("AROverlay");
          
          controller = GetComponent <GazeController> ();
          
          blockHint = Instantiate (blockTemplate);
          blockHint.transform.localScale = new Vector3 (blockSize, blockSize, blockSize);
          blockHint.GetComponent <MeshRenderer> ().material = hintMaterial;
          blockHint.SetActive (false);
          blockHint.GetComponent <Collider> ().enabled = false;
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
          NetworkServer.Spawn (g);
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
                Vector3 hintPosition = parentObject.transform.TransformPoint (quantize (parentObject.transform.InverseTransformPoint (hit.point + hit.normal * 0.5f * blockSize)));
                blockHint.transform.position = hintPosition;
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
