using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlipOrder : MonoBehaviour {
  
  public Material phyCamMaterial;
  public Material virCamMaterial;
  public GameObject transition;
  public GameObject portal;
  
  public bool inPhysical = true;
  
  // Use this for initialization
  void Start () {
    
  }
  
  // Update is called once per frame
  void Update () {
    if (inPhysical)
    {
      phyCamMaterial.SetInt ("_Stencil_Level", 1);
      virCamMaterial.SetInt ("_Stencil_Level", 0);
    }
    else
    {
      phyCamMaterial.SetInt ("_Stencil_Level", 0);
      virCamMaterial.SetInt ("_Stencil_Level", 1);
    }
    
  }
  
  public void OnTriggerEnter(Collider other)
  {
    if (other.gameObject == portal)
    {
      transition.SetActive (true);
      Debug.Log ("Changing universe");
      inPhysical = !inPhysical;
    }
  }
  
  public void OnTriggerExit (Collider other)
  {
    transition.SetActive (false);
  }
  
}
