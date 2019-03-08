using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class CollisionCallback : MonoBehaviour
{
    public UnityEvent collisionHandler;

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Button active");
        collisionHandler.Invoke ();
    }
}
