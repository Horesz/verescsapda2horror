using UnityEngine;

/// <summary>
/// Reliable high-speed projectile for 2D:
/// - Moves by manual translation (frame-rate independent)
/// - Raycasts between previous and current position to avoid tunnelling
/// - Calls enemy.DieNow() on hit (falls back to IDamageable)
/// - Ensures projectile sprite draws on the Projectiles sorting layer and uses Enemy-only hit mask by default
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class Projectile : MonoBehaviour
{
    [Tooltip("Movement speed in units/second.")]
    public float speed = 8f;

    [Tooltip("Seconds before the projectile auto-destroys.")]
    public float lifetime = 3f;

    [Tooltip("Which layers the projectile can hit. If left as All, the script will try to set this to the 'Enemy' layer at Awake.")]
    public LayerMask hitLayers = ~0;

    [Tooltip("Optional radius for CircleCast (0 = Raycast). Useful for fast/large projectiles.")]
    public float castRadius = 0f;

    [Tooltip("Name of the sorting layer to force on the sprite renderer (must exist).")]
    public string sortingLayerName = "Projectiles";

    [Tooltip("Order in layer to force on the sprite renderer (higher draws on top).")]
    public int sortingOrder = 100;

    [Tooltip("Target layer name used as a fallback mask if hitLayers is not set.")]
    public string targetLayerName = "Enemy";

    Rigidbody2D rb;
    Vector2 direction;
    float spawnTime;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            // use the newer API instead of deprecated isKinematic
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }

        // If hitLayers was left at default (all bits set), try to set it to the Enemy layer
        if (hitLayers == (LayerMask)~0)
        {
            int li = LayerMask.NameToLayer(targetLayerName);
            if (li >= 0)
            {
                hitLayers = 1 << li;
            }
            // if layer doesn't exist, leave as is so you can set it in inspector
        }

        // Force sprite renderer sorting so projectile appears on top
        var sr = GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            sr.sortingLayerName = sortingLayerName;
            sr.sortingOrder = sortingOrder;
        }
    }

    void OnEnable()
    {
        spawnTime = Time.time;
    }

    void Update()
    {
        // lifetime check
        if (Time.time - spawnTime >= lifetime)
        {
            Destroy(gameObject);
            return;
        }

        // compute movement for this frame
        Vector2 currentPos = (Vector2)transform.position;
        Vector2 move = direction * speed * Time.deltaTime;
        Vector2 newPos = currentPos + move;

        float distance = move.magnitude;
        if (distance > 0f)
        {
            RaycastHit2D hit;
            if (castRadius > 0f)
            {
                hit = Physics2D.CircleCast(currentPos, castRadius, direction, distance, hitLayers);
            }
            else
            {
                hit = Physics2D.Raycast(currentPos, direction, distance, hitLayers);
            }

            if (hit.collider != null)
            {
                HandleHit(hit.collider, hit.point);
                return; // destroyed or handled
            }
        }

        transform.position = newPos;
    }

    /// <summary>
    /// Initialize direction (unit), speed override, and lifetime override.
    /// </summary>
    public void Initialize(Vector2 dir, float speedOverride, float lifeOverride)
    {
        direction = dir.normalized;
        speed = speedOverride;
        lifetime = lifeOverride;
        spawnTime = Time.time;

        // flip sprite horizontally to face direction
        var sr = GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            sr.flipX = direction.x < 0f;
        }
    }

    void HandleHit(Collider2D otherCollider, Vector2 hitPoint)
    {
        // ignore the player's own components
        if (otherCollider.GetComponentInParent<PlayerAttack>() != null)
        {
            Destroy(gameObject);
            return;
        }

        // first try enemy script
        var enemy = otherCollider.GetComponentInParent<EnemyPatrolAttacker>();
        if (enemy != null)
        {
            enemy.DieNow();
            Destroy(gameObject);
            return;
        }

        // fallback to IDamageable
        var dmg = otherCollider.GetComponentInParent<IDamageable>();
        if (dmg != null)
        {
            dmg.TakeDamage(1);
            Destroy(gameObject);
            return;
        }

        // otherwise we hit something else (wall) - just destroy
        Destroy(gameObject);
    }
}

/// <summary>
/// Optional damageable interface - implement on enemies if you want HP/damage instead of instant DieNow().
/// </summary>
public interface IDamageable
{
    void TakeDamage(int amount);
}