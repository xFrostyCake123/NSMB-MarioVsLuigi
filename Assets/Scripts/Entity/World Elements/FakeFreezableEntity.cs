using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NSMB.Utils;
using Photon.Pun;
public class FakeFreezableEntity : MonoBehaviourPun, IFreezableEntity
{
    // this is just a script for empty frozen cubes to use
    public bool iceCarryable = true, flying = false;
    public bool Frozen { get; set; } = false;
    public bool IsCarryable => iceCarryable;
    public bool IsFlying => flying;
    public Rigidbody2D body;
    public BoxCollider2D collider;

    [PunRPC]
    public void Freeze(int cube) {}

    [PunRPC]
    public void Unfreeze(byte reasonByte) {}

}
