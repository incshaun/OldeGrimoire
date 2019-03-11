using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Breadcrumb : MonoBehaviour {

  [Tooltip ("Time in seconds between drops")]
	public float dropInterval = 0.1f;

	[Tooltip ("Template for object to be dropped")]
	public GameObject dropPrefab;

  [Tooltip ("The object being tracked which provides location for crumbs")]
	public GameObject trackedObject;

  // amount of time since last drop.
	private float timeInterval = 0.0f;

	void Update () {
	  timeInterval += Time.deltaTime;
		if (timeInterval > dropInterval)
		{
			timeInterval = 0.0f;
			Instantiate (dropPrefab, trackedObject.transform.position, trackedObject.transform.rotation);
		}
	}
}
