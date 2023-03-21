using UnityEngine;
using Fusion;

public struct InputNetworkData : INetworkInput
{
    public float forwardAmount;
    public float turnAmount;
    
    // for triggering new objects.
    public bool create;
}
