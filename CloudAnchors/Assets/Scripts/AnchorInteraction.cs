using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using GoogleARCore;
using GoogleARCore.CrossPlatform;
using System.IO;

public class AnchorInteraction : MonoBehaviour {
  
  public AnchorList anchorList;
  public Camera firstPersonCamera;
  public Text updateMessage;
  
  void Update () {
    Touch touch;
    if (Input.touchCount < 1 || (touch = Input.GetTouch(0)).phase != TouchPhase.Began)
    {
      return;
    }
    
    updateMessage.text += "Touch at: " + touch.position;
    
    // Screen touched, find a trackable in the image under the touch point.
    TrackableHit hit;
    if (Frame.Raycast(touch.position.x, touch.position.y, TrackableHitFlags.PlaneWithinPolygon |TrackableHitFlags.FeaturePointWithSurfaceNormal, out hit))
    {
      if (!(hit.Trackable is DetectedPlane) || Vector3.Dot(firstPersonCamera.transform.position - hit.Pose.position, hit.Pose.rotation * Vector3.up) >= 0)
      {
        // Hit a point or front of a plane.
        updateMessage.text += "Hit " + anchorList;
        
        // Create a local anchor on that trackable.
        Component anchor = hit.Trackable.CreateAnchor(hit.Pose);
        if (anchorList != null)
        {
          string label = anchorList.getLabel ();
          updateMessage.text += "Adding anchor " + label;
          // Attach object to the anchor.
          anchorList.addInstanceToAnchor (label, anchor.transform, hit.Pose.position, hit.Pose.rotation);
          //Instantiate(markerPrefab, hit.Pose.position, hit.Pose.rotation);
          
          // Copy the anchor to the cloud.
          anchorList.createCloudAnchor (label, (Anchor) anchor);
        }
      }
    }
  }
}
