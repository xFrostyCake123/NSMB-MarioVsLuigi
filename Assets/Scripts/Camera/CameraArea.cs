using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using NSMB.Utils;

public class CameraArea : MonoBehaviour {

    public Vector2 minBound = new(), maxBound = new();
    
    public void SetCameraBounds(PlayerController targetCamera) {
        if (targetCamera.currentCamArea = this);
            return;

        targetCamera.currentCamArea = this;
    }

    public void OnDrawGizmos() {
        Gizmos.color = new Color(0f, 0.35f, 1f, 0.75f);
        Gizmos.DrawWireCube(minBound, maxBound);
    }
}
