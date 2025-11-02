using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 8f;
    public float airAcceleration = 20f;
    public float groundAcceleration = 50f;

    [Header("Jump")]
    public float jumpForce = 14f;
    public Transform groundCheck;
    public float groundCheckRadius = 0.08f;
    public LayerMask groundLayer;

    Rigidbody2D rb;
    float horizontal;
    bool facingRight = true;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        horizontal = Input.GetAxisRaw("Horizontal");

        // Flip sprite based on input
        if (horizontal > 0.1f && !facingRight) Flip();
        if (horizontal < -0.1f && facingRight) Flip();

        // Jump input
        if (Input.GetKeyDown(KeyCode.Space) && IsGrounded())
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        }

        // Variable jump height (hold jump)
        if (Input.GetButtonUp("Jump") && rb.linearVelocity.y > 0f)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y * 0.5f);
        }

        // Temporary debug — paste into Update() in your PlayerController while testing
        Collider2D[] hits = Physics2D.OverlapCircleAll(groundCheck.position, groundCheckRadius, groundLayer);
        if (hits.Length != 0)
        {
            string names = "";
            for (int i = 0; i < hits.Length; i++) names += hits[i].name + (i == hits.Length - 1 ? "" : ", ");
            Debug.Log("GroundCheck hits: " + hits.Length + " -> " + names);
        }
        else
        {
            Debug.Log("GroundCheck found NOTHING at pos " + groundCheck.position + " radius " + groundCheckRadius);
        }
    }

    void FixedUpdate()
    {
        float accel = IsGrounded() ? groundAcceleration : airAcceleration;
        float targetVx = horizontal * moveSpeed;
        float vx = Mathf.MoveTowards(rb.linearVelocity.x, targetVx, accel * Time.fixedDeltaTime);
        rb.linearVelocity = new Vector2(vx, rb.linearVelocity.y);
    }

    bool IsGrounded()
    {
        if (groundCheck == null) return false;
        return Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
    }

    void Flip()
    {
        facingRight = !facingRight;
        transform.localScale = new Vector3(transform.localScale.x * -1f, transform.localScale.y, transform.localScale.z);
    }

    void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }
    }
}