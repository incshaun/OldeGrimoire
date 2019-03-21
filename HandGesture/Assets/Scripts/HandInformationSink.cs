using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Leap.Unity;
using Leap;

public class HandInformationSink : MonoBehaviour
{
    [Tooltip ("The hand model to work with.")]
    public HandModelBase hand;
    [Tooltip("A cylindrical beam object attached to the controller.")]
    public GameObject laserBeam;
    [Tooltip("An adjustment factor for how fast objects blow up.")]
    public float inflationRate = 1.0f;
    [Tooltip("A sound effect played during inflation.")]
    public AudioSource hiss;
    [Tooltip("A sound effect played when the object is destroyed.")]
    public AudioSource pop;

    // The laser is switched on when the correct gesture is performed.
    public bool beamActive;

    // Link to the network functions.
    private DatagramCommunication dc;

    public void setLaserActive (bool value)
    {
        beamActive = value;
    }

    void Start()
    {
        dc = new DatagramCommunication();
        hand.BeginHand();
    }

    void Update()
    {
        // decay hiss so it stops if no button is pressed.
        if (hiss != null)
        {
            hiss.volume *= 0.9f;
        }

        HandDetails cd = dc.receiveHandDetails();
        if (cd != null)
        {
            hand.SetLeapHand(cd.hand);

            hand.UpdateHand();

            // make the laser beam active if the trigger is pressed.
            laserBeam.SetActive(beamActive);
            if (beamActive)
            {
                Finger f = hand.GetLeapHand().Fingers[(int)Finger.FingerType.TYPE_INDEX]; // index finger.
                Bone b = f.Bone(Bone.BoneType.TYPE_DISTAL);
                Vector3 bCenter = new Vector3(b.Center.x, b.Center.y, b.Center.z);
                Vector3 bDirection = new Vector3(b.Direction.x, b.Direction.y, b.Direction.z);
                laserBeam.transform.position = bCenter;
                laserBeam.transform.forward = bDirection;

                // Raycast, inflate, explode.
                RaycastHit hit;
                if ((Physics.Raycast(bCenter, bDirection, out hit, Mathf.Infinity)) &&
                    (hit.collider.gameObject.tag == "Inflatable"))
                {
                    // Inflate the object by manipulating scale.
                    hit.collider.gameObject.transform.localScale *= 1.0f + (inflationRate * Time.deltaTime);
                    // Play the inflation sound.
                    if (hiss != null) { hiss.volume = 1.0f; if (!hiss.isPlaying) hiss.Play(); }
                    // Pop the object if it gets too big.
                    if (hit.collider.gameObject.transform.localScale.magnitude > 1.5)
                    {
                        Destroy(hit.collider.gameObject);
                        if (pop != null) pop.Play();
                    }
                }
            }

        }
    }
}
