using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class CampfireManager : MonoBehaviourPunCallbacks
{
  public TextMesh roomLabel;
  public GameObject avatarPrefab;
  
  public override void OnJoinedRoom ()
  {
    if (roomLabel != null)
    {
      if (PhotonNetwork.CurrentRoom != null)
      {
        roomLabel.text = "Room:\n" + PhotonNetwork.CurrentRoom.Name;
      }
      else
      {
        roomLabel.text = "Room:\n" + "Not available";
      }
    }
    
    PhotonNetwork.Instantiate (avatarPrefab.name, new Vector3 (), Quaternion.identity, 0);
  }

  public void LeaveRoom ()
  {
    PhotonNetwork.LeaveRoom();
    PhotonNetwork.LoadLevel ("MeetingRooms");
  }
}
