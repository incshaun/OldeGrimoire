using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using Photon.Pun;

public class AvatarActions : MonoBehaviourPun
{
  [Tooltip ("The prefab for objects created when the drop event is triggered.")]
  public GameObject blockTemplate;
  
  [Tooltip ("Turn speed in degrees per second")]
  public float turnSpeed = 100.0f;
  
  [Tooltip ("Movement speed in meters per second (assumes 1 unit = 1 meter)")]
  public float moveSpeed = 10.0f;
  
  private float turn = 0.0f;
  
  private float move = 0.0f;
  
  private void setButtonCallbacks ()
  {
    // Assumes each button has already been set with two triggers; a pointer down followed by a pointer up.
    GameObject.Find ("Canvas/LeftButton").GetComponent <EventTrigger> ().triggers[0].callback.AddListener ((data) => { Left (); });
    GameObject.Find ("Canvas/LeftButton").GetComponent <EventTrigger> ().triggers[1].callback.AddListener ((data) => { Stop (); });
    GameObject.Find ("Canvas/RightButton").GetComponent <EventTrigger> ().triggers[0].callback.AddListener ((data) => { Right (); });
    GameObject.Find ("Canvas/RightButton").GetComponent <EventTrigger> ().triggers[1].callback.AddListener ((data) => { Stop (); });
    GameObject.Find ("Canvas/ForwardButton").GetComponent <EventTrigger> ().triggers[0].callback.AddListener ((data) => { Forward (); });
    GameObject.Find ("Canvas/ForwardButton").GetComponent <EventTrigger> ().triggers[1].callback.AddListener ((data) => { Stop (); });
    GameObject.Find ("Canvas/DropButton").GetComponent <EventTrigger> ().triggers[0].callback.AddListener ((data) => { Drop (); });
    GameObject.Find ("Canvas/DropButton").GetComponent <EventTrigger> ().triggers[1].callback.AddListener ((data) => { Stop (); });
  }
  
  public void Start ()
  {
    if (photonView.IsMine == true || PhotonNetwork.IsConnected == false)
    {
      setButtonCallbacks ();
    }
    else
    {
      transform.Find ("Head/Camera").gameObject.SetActive (false);
    }
  }
  
  public void Update ()
  {
    if (photonView.IsMine == true || PhotonNetwork.IsConnected == false)
    {
      transform.rotation *= Quaternion.AngleAxis (turn * turnSpeed * Time.deltaTime, Vector3.up);
      transform.position += move * moveSpeed * transform.forward;
    }
  }
  
  private void dropObject ()
  {
    // Create the new object up and to the front, so it is away from the avatar.
    GameObject o = PhotonNetwork.Instantiate (blockTemplate.name, transform.position + transform.forward + transform.up, Quaternion.identity);
  }
  
  public void Left ()
  {
    turn = -1.0f;
  }
  public void Right ()
  {
    turn = 1.0f;
  }
  public void Forward ()
  {
    move = 1.0f;
  }
  public void Drop ()
  {
    dropObject ();
  }
  public void Stop ()
  {
    turn = 0.0f;
    move = 0.0f;
  }
}
