using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class ColourChangeEvent : UnityEvent <Color> {}

public class DialControl : MonoBehaviour
{
    public ColourChangeEvent collisionHandler;

    private float angle = 10.0f;
    private Vector3 direction;
    private Quaternion baseRotation;
    
    public void dialTrackHandler (Vector3 p, Quaternion r)
    {    
      baseRotation = r;
      transform.position = p;
      transform.rotation = baseRotation;
      transform.rotation = Quaternion.AngleAxis (angle, transform.up) * transform.rotation;
      
      ShowUpdate ();
    }
    
    private float getRotationAngle (Vector3 direction)
    {
      return Mathf.Atan2 (Vector3.Dot (direction, baseRotation * Vector3.right), Vector3.Dot (direction, baseRotation * Vector3.forward))  * Mathf.Rad2Deg;
    }
    
    private void OnTriggerStay(Collider other)
    {
       direction = other.ClosestPoint (transform.position) - transform.position;
       // Force rotation about the y (up) axis.
       angle = getRotationAngle (direction);
       dialTrackHandler (transform.position, baseRotation);
    }

    public void ShowUpdate ()
    {
        // assumes up is y axis.
        float hue = Mathf.Atan2 (transform.forward.z, transform.forward.x) / (Mathf.PI * 2.0f);
        Color c = Color.HSVToRGB (hue, 1, 1);
        collisionHandler.Invoke (c);

    }
}
