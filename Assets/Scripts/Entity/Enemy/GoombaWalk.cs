using UnityEngine;
using Photon.Pun;
using NSMB.Utils;

public class GoombaWalk : KillableEntity {
    [SerializeField] float speed, deathTimer = -1, terminalVelocity = -8;
    private Vector2 blockOffset = new(0, 0.05f), velocityLastFrame;
    public bool dontFallOffEdge;

    public new void Start() {
        base.Start();
        body.velocity = new Vector2(speed * (left ? -1 : 1), body.velocity.y);
        animator.SetBool("dead", false);
    }

    public new void FixedUpdate() {
        if (GameManager.Instance && GameManager.Instance.gameover) {
            body.velocity = Vector2.zero;
            body.angularVelocity = 0;
            animator.enabled = false;
            body.isKinematic = true;
            return;
        }
        foreach (var player in GameManager.Instance.players) {
            if (player.cobalting > 0f) {
                body.velocity = Vector2.zero;
                animator.enabled = false;
                body.isKinematic = true;
                return;
            } else if (player.cobalting <= 0f && !Frozen && !dead) {
                body.velocity = new Vector2(speed * (left ? -1 : 1), Mathf.Max(terminalVelocity, body.velocity.y));
                animator.enabled = true;
                body.isKinematic = false;
            }
        }

        base.FixedUpdate();
        if (dead) {
            if (deathTimer >= 0 && (photonView?.IsMine ?? true)) {
                Utils.TickTimer(ref deathTimer, 0, Time.fixedDeltaTime);
                if (deathTimer == 0)
                    PhotonNetwork.Destroy(gameObject);
            }
            return;
        }

        physics.UpdateCollisions();
        if (physics.hitLeft || physics.hitRight) {
            left = physics.hitRight;
        }

        if (physics.hitRight && !left) {
            if (photonView && photonView.IsMine) {
                photonView.RPC(nameof(Turnaround), RpcTarget.All, false, velocityLastFrame.x);
            } else {
                Turnaround(false, velocityLastFrame.x);
            }
        } else if (physics.hitLeft && left) {
            if (photonView && photonView.IsMine) {
                photonView.RPC(nameof(Turnaround), RpcTarget.All, true, velocityLastFrame.x);
            } else {
                Turnaround(true, velocityLastFrame.x);
            }
        }

        if (physics.onGround && Physics2D.Raycast(body.position, Vector2.down, 0.5f, Layers.MaskAnyGround) && dontFallOffEdge) {
            Vector3 goombaCheckPos = body.position + new Vector2(0.1f * (left ? -1 : 1), 0);
            if (GameManager.Instance)
                Utils.WrapWorldLocation(ref goombaCheckPos);

            if (!Physics2D.Raycast(goombaCheckPos, Vector2.down, 0.5f, Layers.MaskAnyGround)) {
                if (photonView && photonView.IsMine) {
                    photonView.RPC(nameof(Turnaround), RpcTarget.All, left, velocityLastFrame.x);
                } else {
                    Turnaround(left, velocityLastFrame.x);
                }
            }
        }
        
        body.velocity = new Vector2(speed * (left ? -1 : 1), Mathf.Max(terminalVelocity, body.velocity.y));
        sRenderer.flipX = !left;
    
    }
    
    [PunRPC]
    protected void Turnaround(bool hitWallOnLeft, float x) {
        left = !hitWallOnLeft;
        body.velocity = new Vector2((x > 0.5f ? Mathf.Abs(x) : speed) * (left ? -1 : 1), body.velocity.y);
    }

    [PunRPC]
    public override void Kill() {
        body.velocity = Vector2.zero;
        body.isKinematic = true;
        speed = 0;
        dead = true;
        deathTimer = 0.5f;
        hitbox.enabled = false;
        animator.SetBool("dead", true);
    }
}