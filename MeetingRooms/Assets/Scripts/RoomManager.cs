using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;

public class RoomManager : MonoBehaviourPunCallbacks
{
  void Start()
  {
    PhotonNetwork.ConnectUsingSettings();
  }
  
  public override void OnConnectedToMaster()
  {
    Debug.Log ("Connected to master.");
    PhotonNetwork.JoinLobby ();
  }
  
  public override void OnRoomListUpdate (List<RoomInfo>	roomList)	
  {
    Debug.Log ("Got room list");
    foreach (RoomInfo ri in roomList)
    {
      Debug.Log ("Room: " + ri);
    }
  }

  public override void OnJoinedLobby ()
  {
    Debug.Log ("Joined lobby");
  }
  
  public override void OnCreatedRoom ()
  {
    Debug.Log ("Room created");
//    PhotonNetwork.JoinLobby ();    
//    PhotonNetwork.LeaveLobby ();    
  }
  
  public void addRoom (Text name)
  {
    Debug.Log ("Adding room: " + name.text);
    PhotonNetwork.CreateRoom (name.text);
  }
  
  void Update()
  {
    
  }
}
