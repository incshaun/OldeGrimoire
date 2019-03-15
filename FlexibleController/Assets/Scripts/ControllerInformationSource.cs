using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ControllerInformationSource : MonoBehaviour {
  
  private Gyroscope gyro;
  private bool gyroSupported;
  
  private DatagramCommunication dc;
  private Quaternion gyroAttitude;
  private bool centerPressed;
  private bool triggerPressed;
  
  void Start()
  {
    dc = new DatagramCommunication ();
    
    gyroSupported = SystemInfo.supportsGyroscope;
    if (gyroSupported)
    {
      gyro = Input.gyro;
      gyro.enabled = true;
    }
  }
  
  public void centerClicked () { centerPressed = true; }
  public void triggerClicked () { triggerPressed = true; }  
  public void centerRelease () { centerPressed = false; }
  public void triggerRelease () { triggerPressed = false; }  
  
  void Update () {
    if (gyroSupported)
    {
      gyroAttitude = gyro.attitude;
    }
    
    dc.sendControllerDetails (new ControllerDetails (gyroAttitude, centerPressed, triggerPressed));
  }
}
