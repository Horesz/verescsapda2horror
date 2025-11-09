using UnityEngine;

/// <summary>
/// Drives Animator parameters from Rigidbody2D state and flips the Visual's SpriteRenderer.
/// Attach this to the Player2 root. Assign Visual (child), and ensure Animator on Visual has the parameters:
///    float "Speed", bool "Grounded", trigger "Jump", trigger "Attack", trigger "Die"
/// The script reads Rigidbody2D.velocity and exposes a simple IsGrounded check (optional � replace with your ground detection).
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class PlayerAnimationController : MonoBehaviour
{
    [Tooltip("Drag the Visual child here (contains SpriteRenderer + Animator).")]
    public Transform visual;

    [Tooltip("Name of the float parameter used for walk/run blending (abs horizontal speed).")]
    public string speedParam = "Speed";

    [Tooltip("Name of the bool parameter indicating grounded state.")]
    public string groundedParam = "Grounded";

    [Tooltip("Flip sprite renderer when moving left")]
    public bool flipWithVelocity = true;

    [Tooltip("How fast (units/sec) is considered moving for flip logic.")]
    public float flipThreshold = 0.1f;

    [Tooltip("Optional: small downward offset to check ground from (use your own ground layer mask in GroundCheckLayerMask).")]
    public Vector2 groundCheckOffset = new Vector2(0f, -0.6f);

    [Tooltip("Radius for ground overlap check")]
    public float groundCheckRadius = 0.12f;

    [Tooltip("LayerMask used to detect ground")]
    public LayerMask groundCheckLayerMask = ~0; // default: everything

    Rigidbody2D rb;
    Animator animator;
    SpriteRenderer sr;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        if (visual == null)
        {
            // try to auto-find a child named "Visual"
            var t = transform.Find("Visual");
            if (t != null) visual = t;
        }

        if (visual != null)
        {
            animator = visual.GetComponent<Animator>();
            sr = visual.GetComponent<SpriteRenderer>();
        }

        if (animator == null)
            Debug.LogWarning("PlayerAnimationController: Animator not found on Visual child.");
        if (sr == null)
            Debug.LogWarning("PlayerAnimationController: SpriteRenderer not found on Visual child.");
    }

    void Update()
    {
        if (rb == null || animator == null) return;

        float horizontalSpeed = Mathf.Abs(rb.linearVelocity.x);
        animator.SetFloat(speedParam, horizontalSpeed);

        bool grounded = IsGrounded();
        animator.SetBool(groundedParam, grounded);

        if (flipWithVelocity && sr != null)
        {
            if (rb.linearVelocity.x > flipThreshold) sr.flipX = false;
            else if (rb.linearVelocity.x < -flipThreshold) sr.flipX = true;
        }
    }

    // Simple ground check using OverlapCircle at an offset from root. Replace with your own ground-check if you have one.
    bool IsGrounded()
    {
        Vector2 origin = (Vector2)transform.position + groundCheckOffset;
        return Physics2D.OverlapCircle(origin, groundCheckRadius, groundCheckLayerMask) != null;
    }

    // Helper methods to expose triggers to other scripts (call these from your PlayerController)
    public void TriggerJump() { if (animator) animator.SetTrigger("Jump"); }
    public void TriggerAttack() { if (animator) animator.SetTrigger("Attack"); }
    public void TriggerDie() { if (animator) animator.SetTrigger("Die"); }
}