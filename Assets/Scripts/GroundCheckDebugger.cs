using UnityEngine;

// Attach to the Player. Assign groundCheck and groundLayer.
// This prints a single message when the grounded state changes (less spammy than every frame).
public class GroundCheckDebugger : MonoBehaviour
{
    public Transform groundCheck;
    public float radius = 0.08f;
    public LayerMask groundLayer;

    bool lastGrounded = false;

    void Update()
    {
        if (groundCheck == null) return;
        bool grounded = Physics2D.OverlapCircle(groundCheck.position, radius, groundLayer);
        if (grounded != lastGrounded)
        {
            Debug.Log("Grounded: " + grounded, gameObject);
            lastGrounded = grounded;
        }
    }
}