using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class BlockTransform : NetworkBehaviour {
  
  [SyncVar (hook = "updatePosition")] 
  Vector3 localPosition;
  
  [SyncVar (hook = "updateScale")]
  Vector3 localScale;
  
  void updatePosition (Vector3 p)
  {
    transform.localPosition = p;
    transform.localRotation = Quaternion.identity;
  }
  void updateScale (Vector3 s)
  {
    transform.localScale = s;
  }
  
  void Start ()
  {
    GameObject parentObject = GameObject.Find ("AROverlay");
    transform.SetParent (parentObject.transform);
    if (localScale.x > 0)
    {
      updatePosition (localPosition);
      updateScale (localScale);
    }
  }
  
  private float timer = 0.0f;
  void Update ()
  {
    if (isServer)
    {
      timer += Time.deltaTime;
      if (timer > 0.1f)
      {
        timer = 0.0f;
        localPosition = transform.localPosition;
        localScale = transform.localScale;
      }
    }
  }
}
