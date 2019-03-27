using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Windows.Kinect;

public class RaiseHand : MonoBehaviour {

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

			Windows.Kinect.Joint LHandJoint = body.Joints [JointType.HandLeft];
			Windows.Kinect.Joint RHandJoint = body.Joints [JointType.HandRight];
			Windows.Kinect.Joint LShoulderJoint = body.Joints [JointType.ShoulderLeft];
			Windows.Kinect.Joint RShoulderJoint = body.Joints [JointType.ShoulderRight];
			if (((LHandJoint.Position.Y > LShoulderJoint.Position.Y) &&
 				 (RHandJoint.Position.Y < RShoulderJoint.Position.Y)) ||
				((LHandJoint.Position.Y < LShoulderJoint.Position.Y) &&
				 (RHandJoint.Position.Y > RShoulderJoint.Position.Y))) 
			{
				GetComponent <MeshRenderer> ().material.color = new Color (0, 1, 0);
			}
		}
	}
}
