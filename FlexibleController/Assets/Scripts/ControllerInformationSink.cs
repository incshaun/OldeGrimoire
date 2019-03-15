using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ControllerInformationSink : MonoBehaviour {
  
  [Tooltip ("A cylindrical beam object attached to the controller.")]
  public GameObject laserBeam;
  [Tooltip ("An adjustment factor for how fast objects blow up.")]
  public float inflationRate = 1.0f;
  [Tooltip ("A sound effect played during inflation.")]
  public AudioSource hiss;
  [Tooltip ("A sound effect played when the object is destroyed.")]
  public AudioSource pop;
  
  // Link to the network functions.
  private DatagramCommunication dc;
  // The controller orientation (inverse) for the centered pose. 
  private Quaternion centeredAttitude;
  
  void Start () {
    dc = new DatagramCommunication ();
    centeredAttitude = Quaternion.identity;
  }
  
  void Update () {
    ControllerDetails cd = dc.receiveControllerDetails ();
    // decay hiss so it stops if no button is pressed.
    hiss.volume *= 0.9f;
    if ((cd != null))
    {
      // calculate the rotation to cancel out the current pose.
      if (cd.center)
      {
        centeredAttitude = Quaternion.Inverse (Quaternion.Euler (90, 0, 90) * new Quaternion (cd.gyrox, cd.gyroy, cd.gyroz, cd.gyrow) * Quaternion.Euler (180, 180, 0));
      }
      // make the laser beam active if the trigger is pressed.
      laserBeam.SetActive (cd.trigger);
      if (cd.trigger)
      {
        // Raycast, inflate, explode.
        RaycastHit hit;
        if ((Physics.Raycast(transform.position, transform.forward, out hit, Mathf.Infinity)) &&
            (hit.collider.gameObject.tag == "Inflatable"))
        {
          // Inflate the object by manipulating scale.
          hit.collider.gameObject.transform.localScale *= 1.0f + (inflationRate * Time.deltaTime);
          // Play the inflation sound.
          if (hiss != null) { hiss.volume = 1.0f; if (!hiss.isPlaying) hiss.Play (); }
          // Pop the object if it gets too big.
          if (hit.collider.gameObject.transform.localScale.magnitude > 3)
          {
            Destroy (hit.collider.gameObject);
            if (pop != null) pop.Play ();
          }
        }
      }
      // Match the pose of the virtual controller to that of the remote controller device.
      transform.rotation = Quaternion.Euler (90, 0, 90) * new Quaternion (cd.gyrox, cd.gyroy, cd.gyroz, cd.gyrow) * Quaternion.Euler (180, 180, 0) * centeredAttitude;

    }
  }
}
