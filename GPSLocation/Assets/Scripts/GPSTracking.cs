using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.Android;

public class GPSTracking : MonoBehaviour {
  
  [Tooltip ("Enable this while testing on devices without GPS")]
  public bool fakeLocation = false;
  
  /*
   * Retrieve the location from the location service, typically using GPS. Returns true
   * if the operation succeeded, or false if location is not available at the current 
   * time.
   */
  public bool retrieveLocation (out float latitude, out float longitude, out float altitude)
  {
    latitude = 0.0f;
    longitude = 0.0f;
    altitude = 0.0f;
  
    if (fakeLocation)
    {
      // Mount Everest (according to chatGPT)
      latitude = 27.9881f;
      longitude = 86.9250f;
      altitude = 0.0f;
      
      // Pyramid of Giza
      latitude = 29.9792f;
      longitude = 31.1342f;
      altitude = 0.0f;
      
      // Eiffel Tower
      latitude = 48.8584f;
      longitude = 2.2945f;
      altitude = 0.0f;
      
      // South Pole
      latitude = -90.0f;
      longitude = 0.0f;
      altitude = 0.0f;
      
      return true;
    }
    
    if (!Permission.HasUserAuthorizedPermission(Permission.FineLocation))
    {
        Permission.RequestUserPermission(Permission.FineLocation);
    }
    
    if (!Input.location.isEnabledByUser)
    {
      Debug.Log ("Location service needs to be enabled");
      return false;
    }
    if (Input.location.status != LocationServiceStatus.Running)
    {
      Debug.Log ("Starting location service");
      if (Input.location.status == LocationServiceStatus.Stopped)
      {
        Input.location.Start ();
      }
      return false;
    }
    else
    {
      // Valid data is available.
      latitude = Input.location.lastData.latitude;
      longitude = Input.location.lastData.longitude;
      altitude = Input.location.lastData.altitude;
      return true;
    }
  }
}
