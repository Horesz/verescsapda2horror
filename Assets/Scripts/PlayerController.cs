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

    [Header("Visual / Animation")]
    [Tooltip("Assign the Visual child (contains SpriteRenderer + Animator).")]
    public Transform visual;
    public string speedParam = "Speed";
    public string groundedParam = "Grounded";
    public string jumpTrigger = "Jump";
    public string verticalSpeedParam = "VerticalSpeed"; // optional parameter name

    [Header("Debug")]
    public bool debugGroundCheck = false;

    Rigidbody2D rb;
    float horizontal;
    bool facingRight = true;

    // cached animation components
    Animator animator;
    SpriteRenderer spriteRenderer;

    // cache whether animator has the optional parameter(s)
    bool hasVerticalSpeedParam = false;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        if (visual == null)
        {
            // try to auto-find a child named "Visual"
            var found = transform.Find("Visual");
            if (found != null) visual = found;
        }

        if (visual != null)
        {
            animator = visual.GetComponent<Animator>();
            spriteRenderer = visual.GetComponent<SpriteRenderer>();
        }

        // check for optional VerticalSpeed parameter to avoid exceptions
        if (animator != null)
        {
            hasVerticalSpeedParam = AnimatorHasParameter(animator, verticalSpeedParam);
            if (!hasVerticalSpeedParam)
            {
                Debug.LogWarning($"Animator on '{visual.name}' does not have a float parameter named '{verticalSpeedParam}'. Either add it in the Animator or leave this warning and the code will skip setting it.");
            }
        }
    }

    void Update()
    {
        horizontal = Input.GetAxisRaw("Horizontal");

        // Jump input
        if (Input.GetKeyDown(KeyCode.Space) && IsGrounded())
        {
            // apply jump immediately
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);

            // trigger jump animation
            if (animator != null) animator.SetTrigger(jumpTrigger);
        }

        // Variable jump height (release to cut)
        if (Input.GetButtonUp("Jump") && rb.linearVelocity.y > 0f)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y * 0.5f);
        }

        // Flip visual sprite instead of scaling root
        if (spriteRenderer != null)
        {
            if (horizontal > 0.1f) spriteRenderer.flipX = false;
            else if (horizontal < -0.1f) spriteRenderer.flipX = true;
        }
        else
        {
            // fallback to scale flip if visual not set (keeps old behavior)
            if (horizontal > 0.1f && !facingRight) FlipRootScale();
            if (horizontal < -0.1f && facingRight) FlipRootScale();
        }

        // Drive animator parameters
        if (animator != null)
        {
            animator.SetFloat(speedParam, Mathf.Abs(rb.linearVelocity.x));
            animator.SetBool(groundedParam, IsGrounded());

            // Only set VerticalSpeed if the Animator actually has the parameter
            if (hasVerticalSpeedParam)
            {
                animator.SetFloat(verticalSpeedParam, rb.linearVelocity.y);
            }
        }

        // Optional debug for ground checks (disabled by default)
        if (debugGroundCheck)
        {
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
        return Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer) != null;
    }

    void FlipRootScale()
    {
        facingRight = !facingRight;
        var s = transform.localScale;
        s.x *= -1f;
        transform.localScale = s;
    }

    void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }
    }

    // Utility: check whether an Animator has a parameter with the given name.
    bool AnimatorHasParameter(Animator anim, string paramName)
    {
        if (anim == null || string.IsNullOrEmpty(paramName)) return false;
        var pars = anim.parameters;
        for (int i = 0; i < pars.Length; i++)
        {
            if (pars[i].name == paramName) return true;
        }
        return false;
    }
}