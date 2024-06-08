using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;
using NSMB.Utils;
using UnityEngine.Tilemaps;

public class FireballMover : MonoBehaviourPun {
public AudioSource audioSource;

    public bool luigiFireball, left, isIceball, isStarball, isWaterball, isTidalwave, isMagmaball, isBigMagmaball, goesUp;
    public float terVelocityTreshold = -6.25f;
    private float analogDeadzone = 0.35f;
    public bool accelerates, deccelerates, fastAccelerates;
    public float accelOrDeccelTreshold;
    public float accelMultiplier;
    public bool boomerangs, controllableBoomerang;
    public bool arched;
    public float boomerangTreshold = 1.25f, boomerangMultiplier = 3f, boomerangArchVelocity = 2f, archMultiplier = 5f;
    public bool flipsAround, inverseFlipsAround;
    public PlayerController player;
    public SpriteRenderer spriteRenderer;
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
        if (flipsAround) {
            spriteRenderer = GetComponent<SpriteRenderer>();
            spriteRenderer.flipX = left;
        } else if (inverseFlipsAround) {
            spriteRenderer = GetComponent<SpriteRenderer>();
            spriteRenderer.flipX = !left;
        }

        PhotonView fireballView = GetComponent<PhotonView>();
        if (fireballView != null && fireballView.Owner != null)
        {
            int ownerId = photonView.Owner.ActorNumber;
            GameObject[] players = GameObject.FindGameObjectsWithTag("Player");

            foreach (GameObject p in players) {
                PhotonView playerView = p.GetComponent<PhotonView>();
                if (playerView != null && playerView.Owner != null && playerView.Owner.ActorNumber == ownerId) {
                    player = p.GetComponent<PlayerController>();
                    break;
                }
            }
        }
    }

       public void FixedUpdate() {
        if (GameManager.Instance && GameManager.Instance.gameover) {
            body.velocity = Vector2.zero;
            GetComponent<Animator>().enabled = false;
            body.isKinematic = true;
            return;
        }
        foreach (var player in GameManager.Instance.players) {
            if (player.cobalting > 0f) {
                body.velocity = Vector2.zero;
                body.isKinematic = true;
                return;
            } else if (player.cobalting <= 0f) {
                body.isKinematic = false;
            }
        }
        if (accelerates && speed != accelOrDeccelTreshold) {
            speed += Time.fixedDeltaTime;
        } else if (deccelerates && speed != accelOrDeccelTreshold) {
            speed -= Time.fixedDeltaTime;
        } else if (accelerates && speed != accelOrDeccelTreshold) {
            speed += Time.fixedDeltaTime * accelMultiplier;
        }

        if (boomerangs) {
            if (speed != boomerangTreshold) {
                speed -= Time.fixedDeltaTime * boomerangMultiplier;  
            }
            if (controllableBoomerang) {
                if (player.joystick.y > 0.5) {
                    boomerangArchVelocity += Time.fixedDeltaTime * archMultiplier;
                } else if (player.joystick.y < -analogDeadzone) {
                    boomerangArchVelocity -= Time.fixedDeltaTime * archMultiplier;  
                }  
                body.velocity = new Vector2(speed * (left ? -1 : 1), Mathf.Max(boomerangArchVelocity, body.velocity.y));       
            }

        }

        HandleCollision();

        float gravityInOneFrame = body.gravityScale * Physics2D.gravity.y * Time.fixedDeltaTime;
        if (!goesUp && !controllableBoomerang)
            body.velocity = new Vector2(speed * (left ? -1 : 1), Mathf.Max(-terminalVelocity, body.velocity.y));
        else if (goesUp)
            body.velocity = new Vector2(speed * (left ? -1 : 1), Mathf.Max(terminalVelocity, body.velocity.y));
        
        if (goesUp && terminalVelocity != terVelocityTreshold)
            terminalVelocity -= Time.fixedDeltaTime * 8f;

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
                    if (tile is CoinTile ct || tile is RouletteTile rt || tile is PowerupTile pt) {
                        destroySelf = true;
                    } else {
                        bool ret = it.Interact(this, InteractableTile.InteractionDirection.Up, Utils.TilemapToWorldPosition(tileLoc));
                        destroySelf |= !ret;
                    }
                } else if (tile) {
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
            if (isStarball)
                photonView.RPC(nameof(StarExplosion), RpcTarget.All);
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
            if (isStarball)
                photonView.RPC(nameof(StarExplosion), RpcTarget.All);

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
            if ((isTidalwave || isWaterball) && otherball.isIceball) {
                PhotonNetwork.Destroy(collider.gameObject);
            }
            if (isStarball && (otherball.isTidalwave || otherball.isWaterball)) {
                PhotonNetwork.Destroy(collider.gameObject);
            }
            break;
        }
        case "bulletbill": {
            KillableEntity bb = collider.gameObject.GetComponentInParent<BulletBillMover>();
            if (isStarball)
                photonView.RPC(nameof(StarExplosion), RpcTarget.All);
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
            if (isStarball)
                photonView.RPC(nameof(StarExplosion), RpcTarget.All);
            if (!isIceball) {
                if (!bobomb.lit) {
                    bobomb.photonView.RPC("Light", RpcTarget.All);
                } else {
                    bobomb.photonView.RPC("Kick", RpcTarget.All, body.position.x < bobomb.body.position.x, 0f, false);
                }
                PhotonNetwork.Destroy(gameObject);
            } else if (isStarball) {
                bobomb.photonView.RPC("Detonate", RpcTarget.All);
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
            if (isStarball)
                photonView.RPC(nameof(StarExplosion), RpcTarget.All);
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
    [PunRPC]
    public void StarExplosion() {
        GameObject starryExplosion = (GameObject) Resources.Load("Prefabs/Particle/StarFireworkExplosion");
        Instantiate(starryExplosion, transform.position, Quaternion.identity);

        if (!photonView.IsMine)
            return;

        RaycastHit2D[] hits = Physics2D.CircleCastAll(transform.position + new Vector3(0,0.5f), 1f, Vector2.zero);
        foreach (RaycastHit2D hit in hits) {
            GameObject obj = hit.collider.gameObject;

            if (obj == gameObject)
                continue;

            if (obj.GetComponent<KillableEntity>() is KillableEntity en && !en.dead) {
                en.photonView.RPC("SpecialKill", RpcTarget.All, transform.position.x < obj.transform.position.x, false, 0);
                continue;
            }

            switch (hit.collider.tag) {
            case "Player": {
                PlayerController possibleOwner = obj.GetComponent<PlayerController>();
                if (player = possibleOwner)
                    return;

                obj.GetPhotonView().RPC("Knockback", RpcTarget.All, left, 1, true);
                break;
            }
            }
        }
    }
}
