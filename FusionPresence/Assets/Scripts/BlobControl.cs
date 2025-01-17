using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Fusion;

public class BlobControl : NetworkBehaviour
{
    public override void Spawned()
    {
        GetComponent <Rigidbody> ().linearVelocity = transform.forward;
    }
}
