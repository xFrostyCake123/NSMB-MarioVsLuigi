using UnityEngine;
using UnityEngine.Tilemaps;

using Photon.Pun;
using NSMB.Utils;

public class ItemExplosion : MonoBehaviour {
    
    [SerializeField] private GameObject explosionPrefab;
    [SerializeField] private int explosionTileSize = 2;

    private void OnDestroy() {
        if (!gameObject.activeInHierarchy) {
            Instantiate(explosionPrefab, transform.position, Quaternion.identity);
        }  
    } 
}