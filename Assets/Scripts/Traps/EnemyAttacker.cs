using UnityEngine;

/// <summary>
/// Attach to an enemy GameObject with a trigger Collider2D and an Animator.
/// When the player touches the trigger, this script will:
///  - try to find a PlayerHealth on the colliding object (GetComponentInParent),
///  - call playerHealth.Die() (or apply damage),
///  - optionally play the enemy Attack trigger,
///  - optionally spawn a death VFX, and
///  - destroy the enemy (immediately or after a tiny delay).
/// </summary>
public class EnemyAttacker : MonoBehaviour
{
    [Tooltip("If true, the enemy will call PlayerHealth.Die() when colliding.")]
    public bool killPlayerOnTouch = true;

    [Tooltip("Animator trigger name to play when attacking the player.")]
    public string attackTrigger = "Attack";

    [Tooltip("Animator trigger name to play when this enemy dies (optional).")]
    public string dieTrigger = "Die";

    [Tooltip("Optional particle prefab (blood) to spawn when the enemy dies.")]
    public GameObject deathVfxPrefab;

    [Tooltip("Delay before destroying the enemy (allows short animations/VFX). Use 0 for immediate.")]
    public float destroyDelay = 0.06f;

    Animator animator;
    Collider2D col;

    void Awake()
    {
        animator = GetComponent<Animator>();
        col = GetComponent<Collider2D>();
        if (col == null)
            Debug.LogWarning($"EnemyAttacker on '{name}' has no Collider2D - it won't detect player touches.");
        else if (!col.isTrigger)
            Debug.LogWarning($"EnemyAttacker on '{name}' expects Collider2D.isTrigger = true for OnTriggerEnter2D detection.");
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // Robustly find PlayerHealth in parents (handles child colliders)
        var playerHealth = other.GetComponentInParent<PlayerHealth>();
        if (playerHealth == null)
        {
            // Optional fallback: check tag on root
            if (!other.transform.root.CompareTag("Player")) return;
            playerHealth = other.transform.root.GetComponent<PlayerHealth>();
            if (playerHealth == null) return;
        }

        // Attack: call player's death/damage method
        if (killPlayerOnTouch)
        {
            playerHealth.Die();
        }

        // Play attack animation if animator present
        if (animator != null && !string.IsNullOrEmpty(attackTrigger))
        {
            animator.SetTrigger(attackTrigger);
        }

        // Optionally spawn a VFX immediately at the enemy position
        if (deathVfxPrefab != null)
        {
            Instantiate(deathVfxPrefab, transform.position, Quaternion.identity);
        }

        // Kill this enemy (immediate or after a very short delay so animation/VFX can start)
        if (destroyDelay <= 0f)
        {
            Destroy(gameObject);
        }
        else
        {
            // disable collider to avoid double-trigger
            if (col != null) col.enabled = false;
            // optionally disable other behaviour here (movement, AI)
            Destroy(gameObject, destroyDelay);
        }
    }

    // Public helper if you want to trigger death from code
    public void DieNow()
    {
        if (!string.IsNullOrEmpty(dieTrigger) && animator != null) animator.SetTrigger(dieTrigger);
        if (deathVfxPrefab != null) Instantiate(deathVfxPrefab, transform.position, Quaternion.identity);
        if (destroyDelay <= 0f) Destroy(gameObject);
        else Destroy(gameObject, destroyDelay);
    }
}