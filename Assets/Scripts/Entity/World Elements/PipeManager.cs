using UnityEngine;

public class PipeManager : MonoBehaviour {
    public bool entryAllowed = true, bottom = false, miniOnly = false;
    public PipeManager otherPipe;
    public CameraArea attachedCamera;
    public bool fades;
    public float fadeFloat1 = 0.25f, fadeFloat2 = .1f;
}
