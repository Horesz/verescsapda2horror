using System.Collections;
using UnityEngine;

/// <summary>
/// Patrols between two waypoints or reverses on hitting "wallTag" trigger colliders.
/// On player contact it triggers the Attack animation, calls PlayerHealth.Die(), optionally spawns VFX,
/// and optionally destroys itself. If destroyOnAttack is false the enemy will remain alive and use a short cooldown.
/// </summary>
[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class EnemyPatrolAttacker : MonoBehaviour
{
    [Header("Patrol")]
    public Transform leftPoint;
    public Transform rightPoint;
    [Tooltip("If true use left/right points; otherwise reverse on colliding with objects tagged wallTag.")]
    public bool useWaypoints = false;
    public float moveSpeed = 2.5f;

    [Header("Wall detection")]
    [Tooltip("Tag of invisible boundary objects (BoxCollider2D isTrigger=true).")]
    public string wallTag = "EnemyWall";

    [Header("Attack / Death")]
    [Tooltip("If true, touching the player will call PlayerHealth.Die()")]
    public bool killPlayerOnTouch = true;
    [Tooltip("When true the enemy will be destroyed immediately (or after destroyDelay) when it attacks the player. When false the enemy stays alive.")]
    public bool destroyOnAttack = false;
    public string attackTrigger = "Attack";
    public string dieTrigger = "Die";
    public GameObject deathVfxPrefab;
    [Tooltip("Delay before destroying this enemy (0 = immediate).")]
    public float destroyDelay = 0.05f;

    [Header("Attack cooldown (when not destroying)")]
    [Tooltip("If destroyOnAttack is false, how long the enemy waits before it can attack again.")]
    public float attackCooldown = 0.5f;

    [Header("Visual")]
    [Tooltip("Assign Visual child (contains SpriteRenderer + Animator)")]
    public Transform visual;

    Rigidbody2D rb;
    Collider2D col;
    Animator animator;
    SpriteRenderer spriteRenderer;
    int moveDir = 1; // +1 = right, -1 = left
    bool isDead = false;

    // cooldown guard for repeated hits
    bool onAttackCooldown = false;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();

        if (visual == null)
        {
            var found = transform.Find("Visual");
            if (found != null) visual = found;
        }

        if (visual != null)
        {
            animator = visual.GetComponent<Animator>();
            spriteRenderer = visual.GetComponent<SpriteRenderer>();
        }

        // If waypoints are set, ensure left < right and pick initial direction
        if (useWaypoints && leftPoint != null && rightPoint != null)
        {
            if (leftPoint.position.x > rightPoint.position.x)
            {
                var tmp = leftPoint; leftPoint = rightPoint; rightPoint = tmp;
            }
            moveDir = transform.position.x < (leftPoint.position.x + rightPoint.position.x) * 0.5f ? 1 : -1;
        }
    }

    void FixedUpdate()
    {
        if (isDead) return;

        float vx = moveDir * moveSpeed;
        rb.linearVelocity = new Vector2(vx, rb.linearVelocity.y);

        if (animator != null) animator.SetFloat("Speed", Mathf.Abs(vx));

        if (spriteRenderer != null)
        {
            if (vx > 0.05f) spriteRenderer.flipX = false;
            else if (vx < -0.05f) spriteRenderer.flipX = true;
        }

        if (useWaypoints && leftPoint != null && rightPoint != null)
        {
            if (moveDir > 0 && transform.position.x >= rightPoint.position.x) ReverseDirection();
            else if (moveDir < 0 && transform.position.x <= leftPoint.position.x) ReverseDirection();
        }
    }

    void ReverseDirection()
    {
        moveDir *= -1;
        // nudge to avoid stuck-on-boundary
        var p = transform.position;
        p.x += 0.02f * moveDir;
        transform.position = p;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (isDead) return;
        if (onAttackCooldown) return; // ignore while cooling down

        // Player detection (robust to child colliders)
        var playerHealth = other.GetComponentInParent<PlayerHealth>();
        if (playerHealth != null)
        {
            if (killPlayerOnTouch) playerHealth.Die();

            if (animator != null && !string.IsNullOrEmpty(attackTrigger)) animator.SetTrigger(attackTrigger);

            if (deathVfxPrefab != null) Instantiate(deathVfxPrefab, transform.position, Quaternion.identity);

            if (destroyOnAttack)
            {
                DieNow();
            }
            else
            {
                // temporary disable collider and start cooldown so enemy doesn't immediately retrigger
                if (col != null) col.enabled = false;
                StartCoroutine(AttackCooldownCoroutine());
            }
            return;
        }

        // Wall detection: reverse when hitting wallTag
        if (!useWaypoints && !string.IsNullOrEmpty(wallTag) && other.CompareTag(wallTag))
        {
            ReverseDirection();
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (isDead) return;
        if (onAttackCooldown) return;

        // player via collision
        var playerHealth = collision.collider.GetComponentInParent<PlayerHealth>();
        if (playerHealth != null)
        {
            if (killPlayerOnTouch) playerHealth.Die();
            if (animator != null && !string.IsNullOrEmpty(attackTrigger)) animator.SetTrigger(attackTrigger);
            if (deathVfxPrefab != null) Instantiate(deathVfxPrefab, transform.position, Quaternion.identity);

            if (destroyOnAttack)
            {
                DieNow();
            }
            else
            {
                if (col != null) col.enabled = false;
                StartCoroutine(AttackCooldownCoroutine());
            }
            return;
        }

        // wall via collision
        if (!useWaypoints && !string.IsNullOrEmpty(wallTag) && collision.collider.CompareTag(wallTag))
        {
            ReverseDirection();
        }
    }

    IEnumerator AttackCooldownCoroutine()
    {
        onAttackCooldown = true;
        yield return new WaitForSeconds(attackCooldown);
        if (!isDead && col != null) col.enabled = true;
        onAttackCooldown = false;
    }

    public void DieNow()
    {
        if (isDead) return;
        isDead = true;
        rb.linearVelocity = Vector2.zero;
        if (col != null) col.enabled = false;
        if (animator != null && !string.IsNullOrEmpty(dieTrigger)) animator.SetTrigger(dieTrigger);

        // CHANGED: disable instead of destroy so RespawnManager can re-enable it
        gameObject.SetActive(false);
        Debug.Log($"[EnemyPatrolAttacker] Enemy {gameObject.name} disabled (not destroyed) for respawn.");
    }

    // Add this method to EnemyPatrolAttacker.cs (paste inside the class):

    /// <summary>
    /// Reset the enemy to its alive state (called on respawn).
    /// </summary>
    public void ResetEnemy()
    {
        isDead = false;
        rb.linearVelocity = Vector2.zero;
        if (col != null) col.enabled = true;
        onAttackCooldown = false;

        // Stop cooldown coroutine if running
        StopAllCoroutines();

        // Reset animation speed to idle
        if (animator != null)
        {
            animator.SetFloat("Speed", 0f);
            animator.ResetTrigger(attackTrigger);
            animator.ResetTrigger(dieTrigger);
        }

        Debug.Log($"[EnemyPatrolAttacker] Enemy {gameObject.name} reset to alive state.");
    }
    void OnDrawGizmosSelected()
    {
        if (leftPoint != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(leftPoint.position, 0.08f);
        }
        if (rightPoint != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(rightPoint.position, 0.08f);
        }
    }
}