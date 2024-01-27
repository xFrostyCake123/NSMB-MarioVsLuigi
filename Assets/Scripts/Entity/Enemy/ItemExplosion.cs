using UnityEngine;
using UnityEngine.Tilemaps;

using Photon.Pun;
using NSMB.Utils;

public class ItemExplosion : MonoBehaviour {
    
    [SerializeField] public string explosionPrefab;
    
    private void OnDestroy() {
        if (!gameObject.activeInHierarchy) {
            PhotonNetwork.InstantiateRoomObject(explosionPrefab, transform.position, Quaternion.identity);
        }  
    } 
}