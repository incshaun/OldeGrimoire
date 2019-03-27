using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Windows.Kinect;

public class HandGestures : MonoBehaviour {

	public BodySourceManager bodyManager;

	void Update () {
		GetComponent <MeshRenderer> ().material.color = new Color (0, 0, 1);
		if (bodyManager == null)
		{
			return;
		}

		Body[] data = bodyManager.GetData ();
		if (data == null)
		{
			return;
		}

		foreach (Body body in data)
		{
			if (body == null)
			{
				continue;
			}

			if (body.HandLeftState == Windows.Kinect.HandState.Lasso) {
				GetComponent <MeshRenderer> ().material.color = new Color (0, 1, 0);
			}
		}
	}
}
