using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;

public class ServerTasks : NetworkBehaviour {
  private Text monitor;
  
  public void Awake ()
  {
    monitor = GameObject.Find ("MonitorText").GetComponent <Text> ();
    monitor.text += "\nStarting ";
    //NetworkServer.RegisterHandler(MsgType.OnServerInitialized, OnServerInitialized);
  }
  public override void OnStartServer()
  {
    monitor.text += "\nServer up and running";
    Debug.Log ("A");
  }
  public override void OnStartClient()
  {
    base.OnStartClient ();
    monitor.text += "\nClient running and connected";
    Debug.Log ("B");
  }
}
