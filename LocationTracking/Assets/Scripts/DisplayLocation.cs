using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DisplayLocation : MonoBehaviour {

    [Tooltip ("The element whose position and rotation are reported.")]
    public GameObject trackedElement;

    [Tooltip ("The text element that will be updated.")]
    public Text textOutput;

	void Update () {
	  textOutput.text = trackedElement.transform.position + "\n" + trackedElement.transform.rotation;	
	}
}
