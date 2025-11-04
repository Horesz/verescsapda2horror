using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class FallingSpikeBlock : MonoBehaviour
{
    [Tooltip("Optional: assign the trigger collider here if you prefer the block to listen directly.")]
    public Collider2D triggerArea; // optional reference to a trigger on a child

    [Tooltip("Time after which the block returns to its start position. 0 = no reset.")]
    public float resetDelay = 3f;

    Rigidbody2D rb;
    Vector3 initialPos;
    Quaternion initialRot;
    bool triggered = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        initialPos = transform.position;
        initialRot = transform.rotation;

        // Start kinematic so it stays in place until triggered
        if (rb != null) rb.bodyType = RigidbodyType2D.Kinematic;

        // If someone assigned a trigger collider, make sure it's a trigger
        if (triggerArea != null) triggerArea.isTrigger = true;
    }

    // Optional: keep this if you prefer the block itself having the trigger collider.
    // If you use a separate TriggerCaller child (recommended), this method can be unused.
    void OnTriggerEnter2D(Collider2D other)
    {
        // If the trigger collider is on a child this may be called on the child instead,
        // so using TriggerCaller on the trigger child is more explicit and reliable.
        if (triggered) return;
        if (!other.CompareTag("Player")) return;

        TriggerFall();
    }

    // Called by the TriggerCaller child or by the OnTriggerEnter2D above
    public void TriggerFall()
    {
        if (triggered) return;
        triggered = true;

        if (rb != null)
        {
            // switch to dynamic so physics (gravity) makes it fall
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
            rb.bodyType = RigidbodyType2D.Dynamic;
            // ensure gravity scale is set in the inspector on the Rigidbody2D (e.g. 1)
        }

        if (resetDelay > 0f)
            StartCoroutine(ResetAfterDelay());
    }

    System.Collections.IEnumerator ResetAfterDelay()
    {
        yield return new WaitForSeconds(resetDelay);

        // stop motion and restore initial transform & kinematic state
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
            rb.bodyType = RigidbodyType2D.Kinematic;
        }

        transform.position = initialPos;
        transform.rotation = initialRot;
        triggered = false;
    }

    // Hurt the player when the falling block collides with them
    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider == null) return;
        if (!collision.collider.CompareTag("Player")) return;

        var ph = collision.collider.GetComponent<PlayerHealth>();
        if (ph != null) ph.Die();
    }
}