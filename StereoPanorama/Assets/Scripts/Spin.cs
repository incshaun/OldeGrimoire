using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spin : MonoBehaviour {
  public float speed = 10.0f;
  
  void Update () {
    transform.rotation *= Quaternion.AngleAxis (speed * Time.deltaTime, Vector3.up);
  }
}
