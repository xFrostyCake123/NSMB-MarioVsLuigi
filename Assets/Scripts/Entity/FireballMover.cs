using UnityEngine;
using Photon.Pun;
using NSMB.Utils;
using UnityEngine.Tilemaps;

public class FireballMover : MonoBehaviourPun {
public AudioSource audioSource;

    public bool left, isIceball, isStarball, isWaterball, isTidalwave, isMagmaball, isBigMagmaball;
    public BoxCollider2D worldHitbox;

    [SerializeField] private float speed = 3f, bounceHeight = 4.5f, terminalVelocity = 6.25f, despawnTimer = 0f;

    private Rigidbody2D body;
    private PhysicsEntity physics;
    private bool breakOnImpact;
    
    public void Start() {
        body = GetComponent<Rigidbody2D>();
        physics = GetComponent<PhysicsEntity>();

        object[] data = photonView.InstantiationData;
        left = (bool) data[0];
        if (data.Length > 1 && isIceball)
            speed += Mathf.Abs((float) data[1] / 3f);

        body.velocity = new Vector2(speed * (left ? -1 : 1), -speed);

    }

       public void FixedUpdate() {
        if (GameManager.Instance && GameManager.Instance.gameover) {
            body.velocity = Vector2.zero;
            GetComponent<Animator>().enabled = false;
            body.isKinematic = true;
            return;
        }
        foreach (var player in GameManager.Instance.players) {
            if (player.cobalting > 0) {
                body.velocity = Vector2.zero;
                body.isKinematic = true;
                return;
            } else if (player.cobalting <= 0) {
                body.isKinematic = false;
            }
        }

        HandleCollision();

        float gravityInOneFrame = body.gravityScale * Physics2D.gravity.y * Time.fixedDeltaTime;
        body.velocity = new Vector2(speed * (left ? -1 : 1), Mathf.Max(-terminalVelocity, body.velocity.y));

        if (despawnTimer > 0 && (despawnTimer -= Time.fixedDeltaTime) <= 0) {
            PhotonNetwork.Destroy(gameObject);
        }
    }

    private bool CollideWithTiles() {
        if (!isStarball) {
            return physics.hitLeft || physics.hitRight || physics.hitRoof || (physics.onGround && breakOnImpact);
        }

        bool destroySelf = false;
        ContactPoint2D[] collisions = new ContactPoint2D[20];
        int collisionAmount = worldHitbox.GetContacts(collisions);
        for (int i = 0; i < collisionAmount; i++) {
            var point = collisions[i];
            Vector2 p = point.point + (point.normal * -0.15f);
            if (point.collider.gameObject.layer == Layers.LayerGround) {
                Vector3Int tileLoc = Utils.WorldToTilemapPosition(p);
                TileBase tile = GameManager.Instance.tilemap.GetTile(tileLoc);
                if (tile is InteractableTile it) {
                    bool ret = it.Interact(this, InteractableTile.InteractionDirection.Up, Utils.TilemapToWorldPosition(tileLoc));
                    destroySelf |= !ret;
                }  else if (tile) {
                      destroySelf = true;
                    
                }
            }
        }
        return destroySelf;
    }

    private void HandleCollision() {
        physics.UpdateCollisions();

        if (physics.onGround && !breakOnImpact) {
            float boost = bounceHeight * Mathf.Abs(Mathf.Sin(physics.floorAngle * Mathf.Deg2Rad)) * 1.25f;
            if (Mathf.Sign(physics.floorAngle) != Mathf.Sign(body.velocity.x))
                boost = 0;

            body.velocity = new Vector2(body.velocity.x, bounceHeight + boost);
        } else if (isIceball && body.velocity.y > 1.5f)  {
            breakOnImpact = true;
        } else if (isWaterball && body.velocity.y > 1.5f)  {
            breakOnImpact = true;
        } else if (isMagmaball && body.velocity.y > 1.5f)  {
            breakOnImpact = true;
        }
        bool breaking = CollideWithTiles();
        if (photonView && breaking) {
            if (photonView.IsMine)
                PhotonNetwork.Destroy(gameObject);
            else
                Destroy(gameObject);
        }
    }

    public void OnDestroy() {
        if (!GameManager.Instance.gameover)
            Instantiate(Resources.Load("Prefabs/Particle/" + (isIceball ? "IceballWall" : isStarball ? "Starballwall" : isWaterball ? "Waterballwall" : isTidalwave ? "Waterballwall" : "FireballWall")), transform.position, Quaternion.identity);
    }

    [PunRPC]
    protected void Kill() {
        if (photonView.IsMine)
            PhotonNetwork.Destroy(photonView);
    }
    
    [PunRPC]
    public void PlaySound(Enums.Sounds sound) {
        audioSource.PlayOneShot(sound.GetClip());
    }
    
    private void OnTriggerEnter2D(Collider2D collider) {
        if (!photonView.IsMine)
            return;

        switch (collider.tag) {
        case "koopa":
        case "goomba": {
            KillableEntity en = collider.gameObject.GetComponentInParent<KillableEntity>();
            if (en.dead || en.Frozen)
                return;

            if (isIceball) {
                PhotonNetwork.Instantiate("Prefabs/FrozenCube", en.transform.position + new Vector3(0, 0.1f, 0), Quaternion.identity, 0, new object[] { en.photonView.ViewID });
                PhotonNetwork.Destroy(gameObject);
            } else {
                en.photonView.RPC("SpecialKill", RpcTarget.All, !left, false, 0);
                PhotonNetwork.Destroy(gameObject);
            }
            break;
        }
        case "frozencube": {
            FrozenCube fc = collider.gameObject.GetComponentInParent<FrozenCube>();
            if (fc.dead)
                return;
            // TODO: Stuff here

            if (isIceball) {
                PhotonNetwork.Destroy(gameObject);
            } else {
                fc.gameObject.GetComponent<FrozenCube>().photonView.RPC("Kill", RpcTarget.All);
                PhotonNetwork.Destroy(gameObject);
            }
            break;
        }
        case "Fireball": {
            FireballMover otherball = collider.gameObject.GetComponentInParent<FireballMover>();
            if (isIceball ^ otherball.isIceball) {
                PhotonNetwork.Destroy(collider.gameObject);
                PhotonNetwork.Destroy(gameObject);
            }
            break;
        }
        case "bulletbill": {
            KillableEntity bb = collider.gameObject.GetComponentInParent<BulletBillMover>();
            if (isIceball && !bb.Frozen) {
                PhotonNetwork.Instantiate("Prefabs/FrozenCube", bb.transform.position + new Vector3(0, 0.1f, 0), Quaternion.identity, 0, new object[] { bb.photonView.ViewID });
            }
            PhotonNetwork.Destroy(gameObject);

            break;
        }
        case "bobomb": {
            BobombWalk bobomb = collider.gameObject.GetComponentInParent<BobombWalk>();
            if (bobomb.dead || bobomb.Frozen)
                return;
            if (!isIceball) {
                if (!bobomb.lit) {
                    bobomb.photonView.RPC("Light", RpcTarget.All);
                } else {
                    bobomb.photonView.RPC("Kick", RpcTarget.All, body.position.x < bobomb.body.position.x, 0f, false);
                }
                PhotonNetwork.Destroy(gameObject);
            } else {
                PhotonNetwork.Instantiate("Prefabs/FrozenCube", bobomb.transform.position + new Vector3(0, 0.1f, 0), Quaternion.identity, 0, new object[] { bobomb.photonView.ViewID });
                PhotonNetwork.Destroy(gameObject);
            }
            break;
        }
        case "piranhaplant": {
            KillableEntity killa = collider.gameObject.GetComponentInParent<KillableEntity>();
            if (killa.dead)
                return;
            AnimatorStateInfo asi = killa.GetComponent<Animator>().GetCurrentAnimatorStateInfo(0);
            if (asi.IsName("end") && asi.normalizedTime > 0.5f)
                return;
            if (!isIceball) {
                killa.photonView.RPC("Kill", RpcTarget.All);
                PhotonNetwork.Destroy(gameObject);
            } else {
                PhotonNetwork.Instantiate("Prefabs/FrozenCube", killa.transform.position + new Vector3(0, 0.1f, 0), Quaternion.identity, 0, new object[] { killa.photonView.ViewID });
            }
            break;
        }
        }
    }
}
