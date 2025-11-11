using UnityEngine;

/// <summary>
/// Robust PlayerAttack:
/// - logs input and ammo changes for debugging
/// - auto-creates a persistent FirePoint under the player root if needed
/// - warns and attempts to recover when a scene instance was assigned to projectilePrefab
/// - fixes Rigidbody2D.velocity usage (was using obsolete/incorrect property)
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class PlayerAttack : MonoBehaviour
{
    [Header("Ammo")]
    public int appleAmmo = 0;

    [Header("Projectile")]
    [Tooltip("Assign the projectile prefab asset from the Project window (not a scene instance).")]
    public GameObject projectilePrefab;
    public Transform firePoint; // where projectiles spawn
    public float projectileSpeed = 8f;
    public float projectileLifetime = 3f;

    [Header("Shooting")]
    public string fireInput = "Fire1";
    public float shootCooldown = 0.25f;
    public bool allowHoldToFire = false; // If true uses GetButton instead of GetButtonDown

    [Header("Visual / Animation")]
    public Transform visual;
    public string shootTrigger = "Shoot";

    [Header("Debug")]
    public bool debugLogs = true;

    Rigidbody2D rb;
    Animator animator;
    SpriteRenderer spriteRenderer;
    float lastShotTime = -999f;

    // Prevent duplicate PlayerAttack components firing at same time
    static int activeInstanceId = -1;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

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

        // Create a persistent FirePoint under player root (so it won't be destroyed if Visual is disabled)
        if (firePoint == null)
        {
            var fp = new GameObject("FirePoint");
            fp.transform.SetParent(transform, worldPositionStays: false);
            fp.transform.localPosition = new Vector3(0.6f, 0.1f, 0f); // adjust as needed
            firePoint = fp.transform;
            Debug.Log("[PlayerAttack] Auto-created FirePoint under player root in Awake.");
        }

        // Warn if a Scene instance was assigned instead of an asset, and try to recover via Resources lookup
        if (projectilePrefab != null && projectilePrefab.scene.IsValid())
        {
            Debug.LogWarning("[PlayerAttack] projectilePrefab is a scene instance � assign the prefab asset from the Project window instead of a Hierarchy object.");

            // Try to find an asset with the same name in Resources (helpful if you accidentally dragged a scene object)
            var found = Resources.Load<GameObject>(projectilePrefab.name);
            if (found != null)
            {
                projectilePrefab = found;
                Debug.Log($"[PlayerAttack] Reassigned projectilePrefab from Resources/{projectilePrefab.name} (fallback).");
            }
            else
            {
                Debug.LogWarning("[PlayerAttack] Could not find a matching projectile prefab in Resources. Please assign the prefab asset in the inspector.");
                projectilePrefab = null;
            }
        }

        var comps = GetComponents<PlayerAttack>();
        if (comps != null && comps.Length > 1)
        {
            Debug.LogWarning($"[PlayerAttack] Multiple PlayerAttack components found on '{gameObject.name}'. Count={comps.Length}");
        }

        if (debugLogs) Debug.Log($"[PlayerAttack] Awake on '{gameObject.name}' instanceId={GetInstanceID()} initialAmmo={appleAmmo}");
    }

    void Update()
    {
        bool pressed = allowHoldToFire ? Input.GetButton(fireInput) : Input.GetButtonDown(fireInput);

        if (pressed)
        {
            if (debugLogs) Debug.Log($"[PlayerAttack] Input detected on '{gameObject.name}' instanceId={GetInstanceID()} time={Time.time:F2} appleAmmo={appleAmmo} lastShotDelta={Time.time - lastShotTime:F2}");
        }

        if (pressed && Time.time - lastShotTime >= shootCooldown)
        {
            // Guard to prevent multiple instances firing in same frame
            if (activeInstanceId != -1 && activeInstanceId != GetInstanceID())
            {
                if (debugLogs) Debug.Log($"[PlayerAttack] Another PlayerAttack instance ({activeInstanceId}) is active; skipping this frame for instance {GetInstanceID()}");
                return;
            }

            activeInstanceId = GetInstanceID();
            Shoot();
            activeInstanceId = -1;
        }
    }

    public void AddApples(int count)
    {
        int before = appleAmmo;
        appleAmmo += Mathf.Max(0, count);
        if (debugLogs) Debug.Log($"[PlayerAttack] AddApples called on '{gameObject.name}' instanceId={GetInstanceID()} +{count} (before={before}, after={appleAmmo})");
        // TODO: update UI if you have one
    }

    void Shoot()
    {
        int before = appleAmmo;

        // Ensure projectilePrefab is present
        if (projectilePrefab == null)
        {
            Debug.LogWarning("[PlayerAttack] Cannot shoot: projectilePrefab is not assigned on " + gameObject.name);
            return;
        }

        // Ensure firePoint exists, create a persistent one if needed
        if (firePoint == null)
        {
            Debug.LogWarning("[PlayerAttack] firePoint was null. Creating a fallback FirePoint under the player root.");
            var fp = new GameObject("FirePoint_fallback");
            fp.transform.SetParent(transform, worldPositionStays: false);
            fp.transform.localPosition = new Vector3(0.6f, 0.1f, 0f);
            firePoint = fp.transform;
        }

        if (appleAmmo <= 0)
        {
            if (debugLogs) Debug.Log($"[PlayerAttack] No apple ammo to shoot on '{gameObject.name}' instanceId={GetInstanceID()}");
            return;
        }

        // Consume ammo BEFORE instantiation to avoid reentrancy issues
        appleAmmo = Mathf.Max(0, appleAmmo - 1);
        lastShotTime = Time.time;

        if (debugLogs) Debug.Log($"[PlayerAttack] Shoot() called on '{gameObject.name}' instanceId={GetInstanceID()} time={Time.time:F2} ammoBefore={before} ammoAfter={appleAmmo}");

        // Determine direction
        float dir = 1f;
        if (spriteRenderer != null) dir = spriteRenderer.flipX ? -1f : 1f;
        else dir = transform.localScale.x < 0f ? -1f : 1f;

        Vector3 spawnPos = firePoint.position;
        Quaternion rot = Quaternion.identity;

        var go = Instantiate(projectilePrefab, spawnPos, rot);

        var proj = go.GetComponent<Projectile>();
        if (proj != null)
        {
            proj.Initialize(new Vector2(dir, 0f), projectileSpeed, projectileLifetime);
        }
        else
        {
            var rbproj = go.GetComponent<Rigidbody2D>();
            if (rbproj != null)
            {
                // use the correct property
                rbproj.linearVelocity = new Vector2(dir * projectileSpeed, 0f);
            }
            Destroy(go, projectileLifetime);
        }

        if (animator != null && !string.IsNullOrEmpty(shootTrigger))
        {
            animator.SetTrigger(shootTrigger);
        }
    }
}