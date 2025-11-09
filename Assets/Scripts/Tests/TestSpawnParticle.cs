using UnityEngine;

/// <summary>
/// Small test helper: attach anywhere, assign the same blood prefab and press K in Play mode to spawn.
/// Useful to confirm spawn works at runtime and to see logs when player dies.
/// </summary>
public class TestSpawnParticle : MonoBehaviour
{
    public SpawnBloodOnDeath spawner;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.K))
        {
            if (spawner != null)
            {
                spawner.SpawnAt(transform.position);
                Debug.Log("[TestSpawnParticle] K pressed - spawn requested.");
            }
            else
            {
                Debug.LogWarning("[TestSpawnParticle] spawner reference not assigned.");
            }
        }
    }
}