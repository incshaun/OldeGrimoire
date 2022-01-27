using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class MenuEvents : MonoBehaviour
{
   public UnityEvent onClick = new UnityEvent();
   
   // Respond to objects colliding with the button.
   private bool triggered = false;
   
   private void OnTriggerEnter(Collider other)
   {
     if (other.gameObject.tag == "ControllerObject")
     {
       if (!triggered)
       {
         triggered = true;
         onClick.Invoke ();
       }
     }
   }
   void OnTriggerExit(Collider other)
   {
       triggered = false;
   }
}
