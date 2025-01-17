using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Fusion;
using TMPro;

public class AvatarName : NetworkBehaviour
{
    [Networked, OnChangedRender (nameof (updateName))] 
    public string nickName { get; set; }
    
    public TextMeshPro text;
    
    // Set up the name initially.
    public override void Spawned()
    {
        if (Object.HasStateAuthority)
        {
            nickName = "Initial Name";
        }
    }

    // Called when nickName value changes, on each client.
    public void updateName ()
    {
        text.text = nickName;
    }
    
    // Called to update the nickName value. Must only run
    // on the server (or any node with state authority)
    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    public void RPC_UpdateNickname (string name)
    {
        nickName = name;
    }

    // Called from the application to update the name of
    // the current node.    
    public void setName (string name)
    {
        RPC_UpdateNickname (name);
    }
}
