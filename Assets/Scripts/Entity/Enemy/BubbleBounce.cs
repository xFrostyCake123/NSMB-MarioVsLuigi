using UnityEngine;

using Photon.Pun;
using NSMB.Utils;

public class BubbleBounce : KillableEntity {

    public override void InteractWithPlayer(PlayerController player) {

        Vector2 damageDirection = (player.body.position - body.position).normalized;
        bool attackedFromAbove = Vector2.Dot(damageDirection, Vector2.up) > 0.5f;

        if (player.invincible > 0 || player.inShell || player.sliding
            || ((player.groundpound || player.drill) && player.state != Enums.PowerupState.MiniMushroom && attackedFromAbove)
            || player.state == Enums.PowerupState.MegaMushroom) {

            if (player.drill) {
                player.bounce = true;
                player.drill = false;
            }
            photonView.RPC(nameof(Kill), RpcTarget.All);
            return;
        }
        if (attackedFromAbove) {
            if (!(player.state == Enums.PowerupState.MiniMushroom && !player.groundpound)) {
                photonView.RPC(nameof(Kill), RpcTarget.All);
            }
            player.photonView.RPC(nameof(PlayerController.PlaySound), RpcTarget.All, Enums.Sounds.Enemy_Generic_Stomp);
            player.drill = false;
            player.groundpound = false;
            player.bounce = true;
            return;
        }
    }
}