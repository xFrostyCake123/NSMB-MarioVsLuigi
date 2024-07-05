using UnityEngine;
using Photon.Pun;
using NSMB.Utils;
using System.Collections.Generic;
public class TimedSpawner : MonoBehaviour {

    public string prefab;
    public GameObject currentEntity;
    public float timer = 0f;
    public float respawnTimer = 3f;
    public bool bypassCurrentEntity, ignoreCollision;
    public bool randomizedSpawn;
    public List<string> randomSpawns;
    public bool fromPipe;
    public Vector2 pipeSpawnDirection;
    public bool up, down, left, right;
    private float pipeTimer = 0f, pipeDuration = 1f;
    private void Update() {
        if (currentEntity && !bypassCurrentEntity) {
            return;   
        } else {
            timer += Time.fixedDeltaTime;
        }
            

        if (timer >= respawnTimer) {
        
            AttemptSpawning();
            timer = 0f;
        
        }
    
    }

    public virtual bool AttemptSpawning() {
        if (currentEntity && !bypassCurrentEntity)
            return false;

        foreach (var hit in Physics2D.OverlapCircleAll(transform.position, 1.5f)) {
            if (hit.gameObject.CompareTag("Player") && !ignoreCollision)
                //cant spawn here
                return false;
        }
        if (randomizedSpawn)
            currentEntity = PhotonNetwork.InstantiateRoomObject(randomSpawns[Random.Range(0, randomSpawns.Count)], transform.position, transform.rotation);
        else 
            currentEntity = PhotonNetwork.InstantiateRoomObject(prefab, transform.position, transform.rotation);

        if (fromPipe) {
            Rigidbody2D body = currentEntity.GetComponent<Rigidbody2D>();
            
            int oldLayer = currentEntity.layer;
            int layer = currentEntity.layer;
            if (body != null) {
                if (pipeTimer < pipeDuration / 2f && pipeTimer + Time.fixedDeltaTime >= pipeDuration / 2f) {
                    body.velocity = left ? Vector2.left : down ? Vector2.down : up ? Vector2.up : Vector2.right;
                    pipeTimer = 0f;
                    layer = Layers.LayerHitsNothing;
                } else {
                    body.velocity = new Vector2(body.velocity.x, body.velocity.y);
                    layer = oldLayer;
                }
            }
            currentEntity.layer = layer;
        }
            
        return true;
    }

    public void OnDrawGizmos() {
        string icon = prefab.Split("/")[^1];
        float offset = prefab switch {
            "Prefabs/Enemy/BlueKoopa" => 0.15f,
            "Prefabs/Enemy/RedKoopa" => 0.15f,
            "Prefabs/Enemy/Koopa" => 0.15f,
            "Prefabs/Enemy/Bobomb" => 0.22f,
            "Prefabs/Enemy/Goomba" => 0.22f,
            "Prefabs/Enemy/Spiny" => -0.03125f,
            _ => 0,
        };
        Gizmos.DrawIcon(transform.position + offset * Vector3.up, icon, true, new Color(1, 1, 1, 0.5f));
    }
}
