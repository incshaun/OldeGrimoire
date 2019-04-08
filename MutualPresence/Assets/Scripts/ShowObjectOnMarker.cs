using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using GoogleARCore;

[System.Serializable]
public class MarkerTrackEvent : UnityEvent <Vector3, Quaternion> {}

public class ShowObjectOnMarker : MonoBehaviour
{
    [Tooltip ("The name of the marker in the database")]
    public string trackableName;
    [Tooltip ("The object that is shown when the marker is found")]
    public GameObject asset;
    [Tooltip ("The functions that are called while the marker is tracked")]
    public MarkerTrackEvent updateHandler;

    private Anchor anchor;

    void Start ()
    {
#if UNITY_ANDROID && !UNITY_EDITOR      
          asset.SetActive (false);
#endif
    }
    
    void Update()
    {
       List<AugmentedImage> arImages = new List <AugmentedImage> ();  
       Session.GetTrackables <AugmentedImage> (arImages, TrackableQueryFilter.Updated);
        
       foreach (AugmentedImage image in arImages)
        {
            if ((image.Name == trackableName) && (image.TrackingState == TrackingState.Tracking ))
            {
              if (anchor == null)
                {
                    anchor = image.CreateAnchor(image.CenterPose);
                }
              asset.SetActive (true);
              updateHandler.Invoke (anchor.transform.position, anchor.transform.rotation);
              break;
            }
        }
    }
    
    public void defaultTrackHandler (Vector3 p, Quaternion r)
    {
      asset.transform.position = p;
      asset.transform.rotation = r;
    }
}
