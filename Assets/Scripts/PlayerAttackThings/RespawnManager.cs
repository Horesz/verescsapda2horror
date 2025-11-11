using UnityEngine;

/// <summary>
/// Manages respawning of enemies and pickups when the player dies.
/// Debug version with detailed logging.
/// Uses modern Unity APIs (FindFirstObjectByType, FindObjectsByType).
/// </summary>
public class RespawnManager : MonoBehaviour
{
    [Tooltip("Assign the PlayerHealth component here (auto-finds if left empty).")]
    public PlayerHealth playerHealth;

    [Tooltip("Automatically find all EnemyPatrolAttacker instances at Start.")]
    public bool autoFindEnemies = true;

    [Tooltip("Automatically find all ApplePickup instances at Start.")]
    public bool autoFindPickups = true;

    private EnemyPatrolAttacker[] enemies;
    private Vector3[] enemySpawnPositions;

    private ApplePickup[] pickups;
    private Vector3[] pickupSpawnPositions;

    private bool hasRespawned = false;

    void Start()
    {
        if (playerHealth == null)
        {
            // Use FindFirstObjectByType instead of deprecated FindObjectOfType
            playerHealth = Object.FindFirstObjectByType<PlayerHealth>();
            if (playerHealth != null)
            {
                Debug.Log($"[RespawnManager] Auto-found PlayerHealth on '{playerHealth.gameObject.name}'");
            }
            else
            {
                Debug.LogError("[RespawnManager] PlayerHealth not found!");
                return;
            }
        }

        // Find all enemies and record their spawn positions
        if (autoFindEnemies)
        {
            // Use FindObjectsByType instead of deprecated FindObjectsOfType
            enemies = Object.FindObjectsByType<EnemyPatrolAttacker>(FindObjectsSortMode.None);
            enemySpawnPositions = new Vector3[enemies.Length];
            for (int i = 0; i < enemies.Length; i++)
            {
                enemySpawnPositions[i] = enemies[i].transform.position;
                Debug.Log($"[RespawnManager] Enemy {i} '{enemies[i].gameObject.name}' at {enemySpawnPositions[i]}");
            }
            Debug.Log($"[RespawnManager] Found {enemies.Length} enemies total.");
        }

        // Find all pickups and record their spawn positions
        if (autoFindPickups)
        {
            // Use FindObjectsByType instead of deprecated FindObjectsOfType
            pickups = Object.FindObjectsByType<ApplePickup>(FindObjectsSortMode.None);
            pickupSpawnPositions = new Vector3[pickups.Length];
            for (int i = 0; i < pickups.Length; i++)
            {
                pickupSpawnPositions[i] = pickups[i].transform.position;
                Debug.Log($"[RespawnManager] Pickup {i} '{pickups[i].gameObject.name}' at {pickupSpawnPositions[i]}");
            }
            Debug.Log($"[RespawnManager] Found {pickups.Length} pickups total.");
        }
    }

    void Update()
    {
        if (playerHealth == null) return;

        if (playerHealth.isDead && !hasRespawned)
        {
            Debug.Log("[RespawnManager] *** DEATH DETECTED ***");
            hasRespawned = true;
            Invoke(nameof(RespawnAll), 0.5f);
        }

        if (!playerHealth.isDead && hasRespawned)
        {
            Debug.Log("[RespawnManager] Player respawned, reset flag for next death.");
            hasRespawned = false;
        }
    }

    void RespawnAll()
    {
        Debug.Log("[RespawnManager] ===== RESPAWNING =====");

        // Check enemies
        if (enemies != null)
        {
            Debug.Log($"[RespawnManager] Checking {enemies.Length} enemies...");
            for (int i = 0; i < enemies.Length; i++)
            {
                if (enemies[i] == null)
                {
                    Debug.LogWarning($"[RespawnManager] *** ENEMY {i} IS NULL - IT WAS DESTROYED ***");
                }
                else if (!enemies[i].gameObject.activeSelf)
                {
                    Debug.LogWarning($"[RespawnManager] Enemy {i} is disabled, re-enabling...");
                    enemies[i].gameObject.SetActive(true);
                    enemies[i].transform.position = enemySpawnPositions[i];
                    enemies[i].ResetEnemy();
                }
                else
                {
                    Debug.Log($"[RespawnManager] Enemy {i} exists and active, moving to {enemySpawnPositions[i]}");
                    enemies[i].transform.position = enemySpawnPositions[i];
                    enemies[i].ResetEnemy();
                }
            }
        }

        // Check pickups
        if (pickups != null)
        {
            Debug.Log($"[RespawnManager] Checking {pickups.Length} pickups...");
            for (int i = 0; i < pickups.Length; i++)
            {
                if (pickups[i] == null)
                {
                    Debug.LogWarning($"[RespawnManager] *** PICKUP {i} IS NULL - IT WAS DESTROYED ***");
                }
                else if (!pickups[i].gameObject.activeSelf)
                {
                    Debug.LogWarning($"[RespawnManager] Pickup {i} is disabled, re-enabling...");
                    pickups[i].gameObject.SetActive(true);
                    pickups[i].ResetPickup();
                }
                else
                {
                    Debug.Log($"[RespawnManager] Pickup {i} exists and active, resetting...");
                    pickups[i].gameObject.SetActive(true);
                    pickups[i].ResetPickup();
                }
            }
        }

        // Reset player ammo
        var playerAttack = playerHealth.GetComponent<PlayerAttack>();
        if (playerAttack != null)
        {
            playerAttack.appleAmmo = 0;
            Debug.Log("[RespawnManager] Player ammo reset to 0.");
        }

        Debug.Log("[RespawnManager] ===== RESPAWN COMPLETE =====");
    }
}