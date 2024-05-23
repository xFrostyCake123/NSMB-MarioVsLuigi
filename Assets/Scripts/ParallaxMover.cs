using UnityEngine;

public class ParallaxMover : MonoBehaviour {
    [SerializeField] protected float speed;
    [SerializeField] protected bool noLimit;
    [SerializeField] protected bool moveVertically;
    [SerializeField] protected float verticalSpeed;
    protected Vector3 moveBy;

    public void OnValidate() {
        moveBy = Vector3.right * speed;
    }

    public void Start() {
        moveBy = new(speed, moveVertically ? verticalSpeed : 0, 0);
        if (!noLimit) {
        foreach (var mover in GetComponentsInParent<ParallaxMover>()) {
            if (mover.transform != transform) {
                enabled = false;
                break;
            }
        }
        }
    }

    public virtual void Update() {
        transform.position += Time.deltaTime * moveBy;
    }
}
