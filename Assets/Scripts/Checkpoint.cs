using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class Checkpoint : MonoBehaviour
{
    [Tooltip("Transform where the player will respawn. If left empty, this object's transform will be used.")]
    public Transform respawnTransform;

    [Tooltip("If true the checkpoint can only be activated once.")]
    public bool oneUse = false;

    [Tooltip("Optional VFX to play when checkpoint activates.")]
    public GameObject activateVFX;

    [Tooltip("Optional SFX to play when checkpoint activates.")]
    public AudioClip activateSFX;

    [Tooltip("Optional sprite to swap to when activated (visual feedback).")]
    public Sprite activatedSprite;

    Collider2D col;
    bool activated = false;
    SpriteRenderer spriteRenderer;

    void Awake()
    {
        col = GetComponent<Collider2D>();
        if (col != null)
            col.isTrigger = true;

        if (respawnTransform == null)
            respawnTransform = this.transform;

        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (activated && oneUse) return;

        if (!other.CompareTag("Player")) return;

        var ph = other.GetComponent<PlayerHealth>();
        if (ph == null)
        {
            // Player does not have PlayerHealth component; nothing to do.
            return;
        }

        // Set the player's respawn point
        ph.SetRespawnPoint(respawnTransform);

        // Play visual effect at respawn location (if any)
        if (activateVFX != null)
            Instantiate(activateVFX, respawnTransform.position, Quaternion.identity);

        // Play sound effect at respawn location (if any)
        if (activateSFX != null)
            AudioSource.PlayClipAtPoint(activateSFX, respawnTransform.position);

        // Optional visual feedback: swap sprite when activated
        if (activatedSprite != null && spriteRenderer != null)
            spriteRenderer.sprite = activatedSprite;

        activated = true;
    }

    // Draw a small gizmo to show the respawn location in the Scene view
    void OnDrawGizmosSelected()
    {
        Transform t = respawnTransform != null ? respawnTransform : transform;
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(t.position, 0.12f);
        Gizmos.DrawLine(t.position + Vector3.up * 0.22f, t.position - Vector3.up * 0.22f);
        Gizmos.DrawLine(t.position + Vector3.left * 0.22f, t.position - Vector3.left * 0.22f);
    }
}