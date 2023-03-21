using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SinglePlayerMovement : MonoBehaviour
{
    public float moveSpeed = 1.0f;
    public float turnSpeed = 10.0f;
    
    void Update()
    {
       float h = Input.GetAxis ("Horizontal");
       float v = Input.GetAxis ("Vertical");
       transform.rotation *= Quaternion.AngleAxis (h * turnSpeed * Time.deltaTime, Vector3.up);
       transform.position += v * moveSpeed * Time.deltaTime * transform.forward;
    }
}
