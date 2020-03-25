using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.SceneManagement;

public class RoomManager : MonoBehaviourPunCallbacks
{
  public GameObject roomPrefab;
  public Canvas roomCanvas;

    private bool allowingJoining = false;

//   List <string> rooms = new List <string> ();
  List <GameObject> displayRooms = new List <GameObject> ();
  
  void Start()
  {
    PhotonNetwork.ConnectUsingSettings();
  }
  
  public static string getName (GameObject o)
  {
    if (o.GetComponent <PhotonView> () != null)
    {
      if ((o.GetComponent <PhotonView> ().Owner.NickName != null) && !(o.GetComponent <PhotonView> ().Owner.NickName.Equals ("")))
      {
        return o.GetComponent <PhotonView> ().Owner.NickName;
      }
      else
      {
        return o.GetComponent <PhotonView> ().Owner.UserId;
      }
    }
    else
    {
      // Not a networked object. Just return the current player's id
      if ((PhotonNetwork.NickName != null) && !(PhotonNetwork.NickName.Equals ("")))
      {
        return "X" + PhotonNetwork.NickName;
      }
      else
      {
        return "X" + PhotonNetwork.AuthValues.UserId;
      }
    }
  }
  
  // Find the display version of the room, creating one
  // if none exists.
  GameObject getRoomObject (string name)
  {
    foreach (GameObject g in displayRooms)
    {
      DisplayRoom dr = g.GetComponent <DisplayRoom> ();
      Debug.Log ("Com " + dr.getName () + " " + name);
      if (dr.getName ().Equals (name))
      {
        return g;
      }
    }
    GameObject room = Instantiate (roomPrefab);
    room.transform.SetParent (roomCanvas.transform);
        room.GetComponent<DisplayRoom>().setName(name);
        room.GetComponent<LocalRoomBehaviour>().setManager(this);
        displayRooms.Add (room);
    return room;    
  }
  
  void updateRooms ()
  {
    int row = 0;
    int col = 0;
    int columnLimit = 3;
    foreach (GameObject room in displayRooms)
    {
//       GameObject room = getRoomObject (roomName);
      room.transform.localPosition = new Vector3 (col * 200 - 100, row * 100, 0);
      
      col += 1;
      if (col >= columnLimit)
      {
        col = 0;
        row -= 1;
      }
    }
  }
  
  public void JoinRoom (string roomName)
    {
        allowingJoining = true;
        PhotonNetwork.JoinRoom(roomName);
        PhotonNetwork.LoadLevel ("Campfire");
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
      GameObject room = getRoomObject (ri.Name);
      room.GetComponent <DisplayRoom> ().display (ri.Name + "\n\n" + ri.CustomProperties["notices"]);

      Debug.Log ("Room: " + ri + " " + ri.CustomProperties["notices"]);
//       if (!rooms.Contains (ri.Name))
//       {
//         rooms.Add (ri.Name);
//       }
    }
    updateRooms ();
  }

  public override void OnJoinedLobby ()
  {
    Debug.Log ("Joined lobby");
  }

  public override void OnJoinedRoom ()
  {
    Debug.Log ("Room joined");
    Room r = PhotonNetwork.CurrentRoom;
    Debug.Log ("In room " + r.Name + " - " + r.CustomProperties["notices"]);
    ExitGames.Client.Photon.Hashtable p = r.CustomProperties;
    p["notices"] += RoomManager.getName (this.gameObject) + " was here " + Time.time + "\n";
    r.SetCustomProperties (p);
    string [] roomPropsInLobby = { "notices" };
    r.SetPropertiesListedInLobby (null);
    r.SetPropertiesListedInLobby (roomPropsInLobby);
        //    Debug.Log ("After room " + r.Name + " - " + r.CustomProperties["notices"] + " and " + p["notices"]);
        if (!allowingJoining)
        {
            PhotonNetwork.LeaveRoom();
        }
  }  
  
  public override void OnCreatedRoom ()
  {
    Debug.Log ("Room created");
//     Room r = PhotonNetwork.CurrentRoom;
//     Debug.Log ("In room " + r.Name);
//     ExitGames.Client.Photon.Hashtable p = r.CustomProperties;
//     p["notices"] += PhotonNetwork.AuthValues.UserId + " was here\n";
//     r.SetCustomProperties (p);
//     string [] roomPropsInLobby = { "notices" };
//     r.SetPropertiesListedInLobby (null);
//     r.SetPropertiesListedInLobby (roomPropsInLobby);
//    PhotonNetwork.JoinLobby ();    
//    PhotonNetwork.LeaveLobby ();    
//    PhotonNetwork.GetRoomList ();
    // Creating the room adds the participant to that
    // room. Return to the lobby to see the list of
    // rooms.
//    PhotonNetwork.LeaveRoom ();
  }

  public override void OnCreateRoomFailed (short returnCode, string message)
  {
    Debug.Log ("Failed to create room " + returnCode + " " + message);
  }
  
  public void addRoom (Text name)
  {
    Debug.Log ("Adding room: " + name.text);
    RoomOptions ro = new RoomOptions ();
    ro.EmptyRoomTtl = 100000;
    string [] roomPropsInLobby = { "notices" };
    ro.CustomRoomPropertiesForLobby = roomPropsInLobby;
    ExitGames.Client.Photon.Hashtable customRoomProperties = new ExitGames.Client.Photon.Hashtable () { { "notices", "Room Start\n" } };
    ro.CustomRoomProperties = customRoomProperties;
    PhotonNetwork.JoinOrCreateRoom (name.text, ro, null);
  }
  
  void Update()
  {
    
  }
}
