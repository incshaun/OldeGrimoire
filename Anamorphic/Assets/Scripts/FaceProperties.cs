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
  
  public GameObject linePrefab;
  
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
  
  
 void DrawLine(Vector3 start, Vector3 end, Color color, float duration = 1.0f)
         {
             GameObject myLine = Instantiate (linePrefab);
            myLine.transform.SetParent (sessOrigin.transform);
             /*new GameObject();*/
             myLine.transform.position = start;
/*             myLine.AddComponent<LineRenderer>();*/
             LineRenderer lr = myLine.GetComponent<LineRenderer>();
/*             //lr.material = new Material(Shader.Find("Particles/Alpha Blended Premultiply"));*/
             lr.startColor = color;
             lr.endColor = color;
             lr.startWidth = 0.1f;
             lr.endWidth = 0.1f;
             lr.SetPosition(0, start);
             lr.SetPosition(1, end);
             GameObject.Destroy(myLine, duration);
         }
         
  private void faceChanged (ARFacesChangedEventArgs ev)
  {
    faceStatusText.text = "change - " + ev.added.Count + ev.updated.Count + ev.removed.Count;
    
    if (ev.updated.Count > 0)
    {
      ARFace face = ev.updated[0];
      faceStatusText.text += "Mesh " + face.vertices.Length + " ";
//      FaceMorph.drawFace (face);
      faceHandler.Invoke (face);
      
      List <UnityEngine.XR.InputDevice> devices = new List <UnityEngine.XR.InputDevice> (); arInputManager.subsystem.TryGetInputDevices (devices);
      faceStatusText.text += devices.Count + "XK";
      if (devices.Count > 0)
      {
        Vector3 position;
        Quaternion rotation;
        devices[0].TryGetFeatureValue (UnityEngine.XR.CommonUsages.colorCameraPosition, out position);
        devices[0].TryGetFeatureValue (UnityEngine.XR.CommonUsages.colorCameraRotation, out rotation);
      faceStatusText.text += devices.Count + " DL";
        DrawLine (face.transform.position, position, Color.red, 1.0f);
      faceStatusText.text += face.transform.position.ToString ("F4") + " " + face.transform.forward.ToString ("F4") + " ok";
      }
      
    }
  }
  
  public void Update ()
  {
        DrawLine (new Vector3 (0, 0, 1), new Vector3 (1, 1, 0), Color.red, 1.0f);
  }
}
