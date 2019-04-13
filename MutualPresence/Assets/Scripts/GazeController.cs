using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using UnityEngine.EventSystems;

public class GazeController : NetworkBehaviour {

  private GameObject beam;
  private GameObject button1;
  private GameObject button2;
  
  private bool button1Pressed;
  private bool button2Pressed;

  private GameObject defaultCamera;
  private GameObject ARCamera;
  
  void Start ()
  {
    defaultCamera = GameObject.Find ("Main Camera");
    ARCamera = GameObject.Find ("First Person Camera");

    // Find child beam.
    beam = transform.Find ("Head/LaserBeam").gameObject;
    beam.SetActive (false);
    
    if (isLocalPlayer)
    {
      EventTrigger trigger;
      EventTrigger.Entry pointerDown;
      EventTrigger.Entry pointerUp;
      
      // Find in scene.
      button1 = GameObject.Find ("PrimaryButton");
      
      trigger = button1.gameObject.AddComponent<EventTrigger>();
      pointerDown = new EventTrigger.Entry();
      pointerDown.eventID = EventTriggerType.PointerDown;
      pointerDown.callback.AddListener((e) => button1Pressed = true);
      trigger.triggers.Add(pointerDown);
      pointerUp = new EventTrigger.Entry();
      pointerUp.eventID = EventTriggerType.PointerUp;
      pointerUp.callback.AddListener((e) => button1Pressed = false);
      trigger.triggers.Add(pointerUp);

      button2 = GameObject.Find ("SecondaryButton");
      
      trigger = button2.gameObject.AddComponent<EventTrigger>();
      pointerDown = new EventTrigger.Entry();
      pointerDown.eventID = EventTriggerType.PointerDown;
      pointerDown.callback.AddListener((e) => button2Pressed = true);
      trigger.triggers.Add(pointerDown);
      pointerUp = new EventTrigger.Entry();
      pointerUp.eventID = EventTriggerType.PointerUp;
      pointerUp.callback.AddListener((e) => button2Pressed = false);
      trigger.triggers.Add(pointerUp);
      
      setActive (true);

#if UNITY_ANDROID && !UNITY_EDITOR
      defaultCamera.SetActive (false);
      ARCamera.SetActive (true);
#else      
      defaultCamera.SetActive (true);
      ARCamera.SetActive (false);
      transform.position = new Vector3 (0, 1.5f, 0);
      defaultCamera.transform.SetParent (transform.Find ("Head"));
      defaultCamera.transform.localPosition = new Vector3 (0, 0.1f, 0);
#endif
    }
  }

  void Update ()
  {
#if UNITY_ANDROID && !UNITY_EDITOR   
    // Calculate position to set the avatar so that head is at camera position.
    // Small offset so the beam is visible.
    if (isLocalPlayer)
    {
      transform.position = ARCamera.transform.position + new Vector3 (0, 0.1f, -0.2f);
      transform.rotation = ARCamera.transform.rotation;
    }
#endif    
  }
  
  public Ray getRay ()
  {
    return new Ray (beam.transform.position, beam.transform.forward);
  }
  public bool getPrimary ()
  {
    return button1Pressed;
  }
  public bool getSecondary ()
  {
    return button2Pressed;
  }
  
  public void setActive (bool active)
  {
    if (isLocalPlayer)
    {
      beam.SetActive (active);
      button1.SetActive (active);
      button2.SetActive (active);
    }
  }
}
