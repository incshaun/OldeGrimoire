using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhysicalCameraOrientation : MonoBehaviour {
  public CameraView camView;
  void Update () {
    transform.rotation =  Quaternion.AngleAxis (-camView.getWebcamTex ().videoRotationAngle, Vector3.forward);
  }
}
