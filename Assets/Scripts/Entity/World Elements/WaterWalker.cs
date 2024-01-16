using UnityEngine;

public class WaterWalker : MonoBehaviour
{
    public float maxSpeed = 4.21875f;
    public float waterWalkingThreshold = 2f; // Adjust this threshold based on your requirements

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Check if the collision is with an object tagged as "WaterCollider"
        if (collision.gameObject.CompareTag("WaterCollider"))
        {
            Rigidbody2D rb = collision.gameObject.GetComponent<Rigidbody2D>();

            // Check if the object has a Rigidbody2D
            if (rb != null)
            {
                // Allow walking over the water collider when the player's X-axis velocity is at max speed
                if (Mathf.Abs(rb.velocity.x) >= waterWalkingThreshold)
                {
                    Physics2D.IgnoreCollision(collision.collider, GetComponent<Collider2D>(), true);
                }
            }
        }
    }
}
