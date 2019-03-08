using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using GoogleARCore;

public class ShowObjectOnMarker : MonoBehaviour
{
    public string trackableName;
    public GameObject asset;
    public UnityEvent updatedHandler;

    private Anchor anchor;

    void Start ()
    {
          asset.SetActive (false);
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
              asset.transform.position = anchor.transform.position;
              asset.transform.rotation = anchor.transform.rotation;
              asset.SetActive (true);
              updatedHandler.Invoke ();
              break;
            }
        }
    }
}
