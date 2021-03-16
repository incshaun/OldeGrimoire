using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using TMPro;

public class FaceProperties : MonoBehaviour
{
  public TextMeshProUGUI faceStatusText;
  
  public ARFaceManager faceManager;

  [Tooltip ("Provide the AR Session object for this field")]
  public ARInputManager arInputManager;
  
  public GameObject sessOrigin;
  
  public delegate void FaceEventHandler (ARFace face);
  public event FaceEventHandler faceHandler;
  
  void OnEnable()
  {
    faceManager.facesChanged += faceChanged;
  }
  
  void OnDisable()
  {
    faceManager.facesChanged -= faceChanged;
  }
  
  private void faceChanged (ARFacesChangedEventArgs ev)
  {
//     faceStatusText.text = "change - " + ev.added.Count + ev.updated.Count + ev.removed.Count;
    
    if (ev.updated.Count > 0)
    {
      ARFace face = ev.updated[0];
      faceHandler.Invoke (face);
    }
  }
}
