using UnityEngine;
using Photon.Pun;

public class SelfDestroyer : MonoBehaviour {

    public GameObject startGate;
    public float timer = 0f;
    public float destroyTimer = 3f;

    public void Update() {
         
        timer += Time.deltaTime;
        if (timer >= destroyTimer) {
            
            DestroyObject();
        
        }
    
    }

    public void DestroyObject() {
        Instantiate(Resources.Load("Prefabs/Particle/Explosion"), transform.position, Quaternion.identity);
        Destroy(gameObject);
    }

}
