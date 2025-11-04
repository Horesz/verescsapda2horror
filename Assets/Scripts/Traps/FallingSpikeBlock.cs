using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class FallingSpikeBlock : MonoBehaviour
{
    public Collider2D triggerArea; // a trigger placed under the block that detects the player
    public float resetDelay = 3f; // optional: time after which the block resets to its original pos
    Rigidbody2D rb;
    Vector3 initialPos;
    Quaternion initialRot;
    bool triggered = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        initialPos = transform.position;
        initialRot = transform.rotation;
        if (rb != null) rb.bodyType = RigidbodyType2D.Kinematic;
        if (triggerArea != null) triggerArea.isTrigger = true;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (triggered) return;
        if (!other.CompareTag("Player")) return;

        TriggerFall();
    }

    public void TriggerFall()
    {
        triggered = true;
        if (rb != null) rb.bodyType = RigidbodyType2D.Dynamic;
        // start reset routine
        if (resetDelay > 0f) StartCoroutine(ResetAfterDelay());
    }

    System.Collections.IEnumerator ResetAfterDelay()
    {
        yield return new WaitForSeconds(resetDelay);
        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;
        rb.bodyType = RigidbodyType2D.Kinematic;
        transform.position = initialPos;
        transform.rotation = initialRot;
        triggered = false;
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.CompareTag("Player"))
        {
            var ph = collision.collider.GetComponent<PlayerHealth>();
            if (ph != null) ph.Die();
        }
    }
}