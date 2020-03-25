using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spin : MonoBehaviour
{
  public float spinSpeed = 10.0f;
  
    void Update()
    {
      transform.rotation *= Quaternion.AngleAxis (spinSpeed * Time.deltaTime, Vector3.up);
    }
}
