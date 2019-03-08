using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class ColourChangeEvent : UnityEvent <Color> {}

public class DialControl : MonoBehaviour
{
    public ColourChangeEvent collisionHandler;

    private void OnTriggerStay(Collider other)
    {
    
       Vector3 direction = other.ClosestPoint (transform.position) - transform.position;
       transform.forward = direction;
       Debug.Log("Collision" + other.ClosestPoint (transform.position) + " " + transform.position + " " + 100.0f * direction);

       ShowUpdate ();
    }

    public void ShowUpdate ()
    {
        // assumes up is y axis.
        float hue = Mathf.Atan2 (transform.forward.z, transform.forward.x) / (Mathf.PI * 2.0f);
        Color c = Color.HSVToRGB (hue, 1, 1);
        collisionHandler.Invoke (c);

    }
}
