using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveParticipant : MonoBehaviour {

  private float latitude = 0.0f;
  private float longitude = 0.0f;
  private float stepSize = 0.1f;
  
  public SharedLocation networkManager;
  
  private void setPosition ()
  {
    transform.position = new Vector3 (longitude, latitude, 0.0f);
    // update status via the local client.
    networkManager.sendPosition (latitude, longitude);
  }
  
  private void setDropPosition ()
  {
    // inform client of dropped marker.
    networkManager.sendMarker (latitude, longitude);
  }
  
  public void moveUp () { latitude += stepSize; setPosition (); }
  public void moveDown () { latitude -= stepSize; setPosition (); }
  public void moveLeft () { longitude -= stepSize; setPosition (); }
  public void moveRight () { longitude += stepSize; setPosition (); }
  public void dropMarker () { setDropPosition (); }
}
