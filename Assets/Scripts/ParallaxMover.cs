using UnityEngine;

public class ParallaxMover : MonoBehaviour {
    [SerializeField] protected float speed;
    protected Vector3 moveBy;

    public void OnValidate() {
        moveBy = Vector3.right * speed;
    }

    public void Start() {
        moveBy = new(speed, 0, 0);
        foreach (var mover in GetComponentsInParent<ParallaxMover>()) {
            if (mover.transform != transform) {
                enabled = false;
                break;
            }
        }
    }

    public virtual void Update() {
        transform.position += Time.deltaTime * moveBy;
    }
}
