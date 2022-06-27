using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioSpatializer : MonoBehaviour {

  public List<AudioSource> sources;

  public float dropoffDistanceConstant = 0.8f;
  
  public float attenuationFactor = 1.5f; // Should be 2.0f;
  
  public float speedOfSound = 330.0f;
  
  void Update () {
    foreach (AudioSource source in sources)
    {
      // Volume effects.
      GameObject sourceObject = source.gameObject;
      float distance = Vector3.Distance (sourceObject.transform.position, transform.position);
      source.volume = 1.0f / Mathf.Pow (dropoffDistanceConstant * distance, attenuationFactor);
      
      // Doppler effects.
      // Assume sound sources are stationary.
      Vector3 sourceVelocity = new Vector3 (0, 0, 0);
      Vector3 myVelocity = GetComponent <PrototypeLocationTracking> ().getVelocity ();
      Vector3 relativeVelocity = myVelocity - sourceVelocity;
      Vector3 directionBetweenMeAndSource = Vector3.Normalize (sourceObject.transform.position - transform.position);
      float relativeSpeed = Vector3.Dot (directionBetweenMeAndSource, relativeVelocity);
      
      source.pitch = (speedOfSound + relativeSpeed) / (speedOfSound - relativeSpeed);
      
    }
  }
}
