using UnityEngine;

/// <summary>
/// Floating pickup: when player touches it, grants apple ammo via PlayerAttack.AddApples(...) and disables itself.
/// On respawn, the pickup is re-enabled instead of being destroyed.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class ApplePickup : MonoBehaviour
{
    [Tooltip("How many shots this pickup grants.")]
    public int appleCount = 3;

    [Tooltip("Optional pickup VFX prefab to spawn when collected.")]
    public GameObject pickupVfx;

    [Tooltip("Optional pickup sound (AudioSource on a manager or one-shot via AudioSource.PlayClipAtPoint).")]
    public AudioClip pickupClip;
    public float pickupVolume = 1f;

    // guard to ensure we only pick up once even if multiple trigger events fire
    bool pickedUp = false;

    void Reset()
    {
        // Make sure collider is trigger by default
        var col = GetComponent<Collider2D>();
        if (col != null) col.isTrigger = true;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (pickedUp) return; // already collected / in the process of being collected

        // Robust: find PlayerAttack in parents (works if collider belongs to child)
        var playerAttack = other.GetComponentInParent<PlayerAttack>();
        if (playerAttack != null)
        {
            pickedUp = true;

            // Debug logs so you can see exactly what happened
            Debug.Log($"[ApplePickup] Collected by '{other.gameObject.name}' (root='{other.transform.root.name}'). appleCount={appleCount}. playerAmmoBefore={playerAttack.appleAmmo}");

            playerAttack.AddApples(appleCount);

            Debug.Log($"[ApplePickup] playerAmmoAfter={playerAttack.appleAmmo}");

            if (pickupVfx != null)
                Instantiate(pickupVfx, transform.position, Quaternion.identity);

            if (pickupClip != null)
                AudioSource.PlayClipAtPoint(pickupClip, Camera.main != null ? Camera.main.transform.position : transform.position, pickupVolume);

            // CHANGED: disable instead of destroy so RespawnManager can re-enable it
            var col = GetComponent<Collider2D>();
            if (col != null) col.enabled = false;
            var sr = GetComponent<SpriteRenderer>();
            if (sr != null) sr.enabled = false;

            // disable the gameobject instead of destroying it
            gameObject.SetActive(false);
            Debug.Log($"[ApplePickup] Pickup disabled (not destroyed) so it can respawn.");
            return;
        }

        // Optional fallback: support tag-based detection fallback
        if (other.transform.root.CompareTag("Player"))
        {
            var pa = other.transform.root.GetComponentInChildren<PlayerAttack>();
            if (pa != null)
            {
                pickedUp = true;
                Debug.Log($"[ApplePickup] Collected by root tag fallback. appleCount={appleCount}. playerAmmoBefore={pa.appleAmmo}");
                pa.AddApples(appleCount);
                Debug.Log($"[ApplePickup] playerAmmoAfter={pa.appleAmmo}");

                if (pickupVfx != null) Instantiate(pickupVfx, transform.position, Quaternion.identity);
                if (pickupClip != null) AudioSource.PlayClipAtPoint(pickupClip, Camera.main != null ? Camera.main.transform.position : transform.position, pickupVolume);

                var col2 = GetComponent<Collider2D>();
                if (col2 != null) col2.enabled = false;
                var sr2 = GetComponent<SpriteRenderer>();
                if (sr2 != null) sr2.enabled = false;

                gameObject.SetActive(false);
                Debug.Log($"[ApplePickup] Pickup disabled (not destroyed) so it can respawn.");
            }
        }
    }

    /// <summary>
    /// Reset the pickup to its collected state (called on respawn).
    /// </summary>
    public void ResetPickup()
    {
        pickedUp = false;

        var col = GetComponent<Collider2D>();
        if (col != null) col.enabled = true;

        var sr = GetComponent<SpriteRenderer>();
        if (sr != null) sr.enabled = true;

        // Re-enable the gameobject
        gameObject.SetActive(true);

        Debug.Log($"[ApplePickup] Pickup {gameObject.name} reset and ready for collection.");
    }
}