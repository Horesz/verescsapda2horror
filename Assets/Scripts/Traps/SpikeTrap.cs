using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class SpikeTrap : MonoBehaviour
{
    [Tooltip("If non-empty, the trap will only trigger for GameObjects on these layers. If left empty, the script will use the Player tag.")]
    public LayerMask playerLayer;

    [Tooltip("If true the spike instantly kills the player by calling PlayerHealth.Die().")]
    public bool instantKill = true;

    [Tooltip("Optional VFX to spawn at the player's position on hit.")]
    public GameObject hitVFX;

    [Tooltip("Optional SFX to play on hit.")]
    public AudioClip hitSFX;

    void Reset()
    {
        // Make the collider a trigger by default so OnTriggerEnter2D will fire.
        var c = GetComponent<Collider2D>();
        if (c != null) c.isTrigger = true;
    }

    void Awake()
    {
        // If the inspector left the mask as all zeros, try to auto-select the "Player" layer if it exists.
        if (playerLayer.value == 0)
        {
            int playerLayerIndex = LayerMask.NameToLayer("Player");
            if (playerLayerIndex != -1)
                playerLayer = 1 << playerLayerIndex;
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // If a layer mask is set, use it. Otherwise fall back to tag check for "Player".
        if (playerLayer.value != 0)
        {
            if (!IsInLayerMask(other.gameObject.layer, playerLayer)) return;
        }
        else
        {
            if (!other.CompareTag("Player")) return;
        }

        var ph = other.GetComponent<PlayerHealth>();
        if (ph != null)
        {
            if (instantKill)
                ph.Die();
            else
                ph.Die(); // placeholder for future damage logic
        }

        if (hitVFX != null)
            Instantiate(hitVFX, other.transform.position, Quaternion.identity);

        if (hitSFX != null)
            AudioSource.PlayClipAtPoint(hitSFX, transform.position);
    }

    bool IsInLayerMask(int layer, LayerMask mask)
    {
        return (mask.value & (1 << layer)) != 0;
    }
}