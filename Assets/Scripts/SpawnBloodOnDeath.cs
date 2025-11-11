using UnityEngine;

/// <summary>
/// Simple spawner: instantiate a ParticleSystem prefab and ensure it plays and is auto-destroyed.
/// Backwards-compatible: provides Spawn(), Spawn(Vector3) and SpawnAt(Vector3).
/// </summary>
public class SpawnBloodOnDeath : MonoBehaviour
{
    [Tooltip("Assign a BloodEffect prefab (contains ParticleSystem).")]
    public GameObject bloodPrefab;

    [Tooltip("If true, the spawned particle system will be unparented so it survives player destruction.")]
    public bool unparentOnSpawn = true;

    [Tooltip("Optional offset from the origin where blood spawns.")]
    public Vector3 offset = Vector3.zero;

    [Tooltip("Time after which the spawned object will be destroyed (safety).")]
    public float autoDestroyAfter = 5f;

    /// <summary>
    /// Backwards-compatible zero-arg spawn: spawn at this GameObject's position.
    /// Call: spawner.Spawn();
    /// </summary>
    public void Spawn()
    {
        SpawnAt(transform.position);
    }

    /// <summary>
    /// Backwards-compatible spawn with explicit position.
    /// Call: spawner.Spawn(transform.position);
    /// </summary>
    public void Spawn(Vector3 worldPosition)
    {
        SpawnAt(worldPosition);
    }

    /// <summary>
    /// Core spawn worker. Instantiates prefab, ensures ParticleSystem plays and schedules cleanup.
    /// </summary>
    public void SpawnAt(Vector3 worldPosition)
    {
        if (bloodPrefab == null)
        {
            Debug.LogWarning("[SpawnBloodOnDeath] bloodPrefab is not assigned.");
            return;
        }

        var go = Instantiate(bloodPrefab, worldPosition + offset, Quaternion.identity);

        // Try to find a ParticleSystem on the root or in children
        ParticleSystem ps = go.GetComponent<ParticleSystem>();
        if (ps == null) ps = go.GetComponentInChildren<ParticleSystem>();

        if (ps != null)
        {
            // Ensure it starts clean and playing
            ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            ps.Play();

            // If requested, unparent so particles survive parent destruction
            if (unparentOnSpawn)
            {
                go.transform.parent = null;
            }

            // compute approximate lifetime (duration + max startLifetime)
            float maxStartLifetime = 0f;
            var main = ps.main;
            if (main.startLifetime.mode == ParticleSystemCurveMode.Constant)
            {
                maxStartLifetime = main.startLifetime.constant;
            }
            else if (main.startLifetime.mode == ParticleSystemCurveMode.TwoConstants)
            {
                maxStartLifetime = main.startLifetime.constantMax;
            }
            else
            {
                // conservative fallback
                maxStartLifetime = main.duration;
            }

            float life = main.duration + maxStartLifetime;
            float destroyAfter = Mathf.Clamp(life + 0.5f, 1f, autoDestroyAfter);

            Destroy(go, destroyAfter);
            Debug.Log($"[SpawnBloodOnDeath] Spawned blood at {worldPosition}, duration~{life:F2}s (destroy in {destroyAfter:F2}s)");
            return;
        }

        Debug.LogWarning("[SpawnBloodOnDeath] No ParticleSystem found on bloodPrefab or its children.");
        Destroy(go, autoDestroyAfter);
    }
}