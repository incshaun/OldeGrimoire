using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Fusion;

public class AvatarControl : NetworkBehaviour
{
  public float moveSpeed = 1.0f;
  public float turnSpeed = 100.0f;
  
  public GameObject blobTemplate;

  public IEnumerator removeBlob (NetworkObject blob)
  {
    yield return new WaitForSeconds (5.0f);
    Runner.Despawn (blob);
  }
  
  [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
  public void RPC_SpawnBlob ()
  {
    NetworkObject blob = Runner.Spawn (blobTemplate, transform.position + 0.9f * transform.forward + 1.2f * transform.up, transform.rotation, Runner.LocalPlayer);
    // Despawn after a while, otherwise uses up all the networked object slots.
    StartCoroutine (removeBlob (blob));
  }
  
  public override void FixedUpdateNetwork()
  {
    if (GetInput (out InputNetworkData move))
    {
      //         Debug.Log ("Doing update: " + move.forwardAmount + " " + move.turnAmount);
      transform.rotation *= Quaternion.AngleAxis (move.turnAmount * turnSpeed * Runner.DeltaTime, Vector3.up);
      transform.position += move.forwardAmount * moveSpeed * Runner.DeltaTime * transform.forward;
      
      // when creating new objects.
      if (move.create && (blobTemplate != null))
      {
        RPC_SpawnBlob ();
      }
    }
  }
}
