using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GoogleARCore;

public class ShowObjectOnTrackable : MonoBehaviour {
  
  public GameObject displayObject;
  
  private List<AugmentedImage> trackedImages = new List<AugmentedImage>();
  
  // Update is called once per frame
  void Update () {
    displayObject.SetActive (false);
    
    Session.GetTrackables<AugmentedImage> (trackedImages, TrackableQueryFilter.All);
    foreach (var image in trackedImages)
    {
      if (image.TrackingState == TrackingState.Tracking)
      {
        displayObject.transform.position = image.CenterPose.position;
        displayObject.transform.rotation = image.CenterPose.rotation;
        displayObject.SetActive (true);
      }
    }
  }
}
