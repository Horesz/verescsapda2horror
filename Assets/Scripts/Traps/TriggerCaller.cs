using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class TriggerCaller : MonoBehaviour
{
    [Tooltip("The FallingSpikeBlock parent to call. If left empty the script will try to auto-find in parents.")]
    public FallingSpikeBlock parentBlock;

    [Tooltip("Layers considered 'player' for this trigger. If left empty the script will fall back to tag check.")]
    public LayerMask playerLayer;

    void Awake()
    {
        if (parentBlock == null) parentBlock = GetComponentInParent<FallingSpikeBlock>();

        var c = GetComponent<Collider2D>();
        if (c != null && !c.isTrigger)
            c.isTrigger = true;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // LayerMask check if configured
        if (playerLayer.value != 0)
        {
            if ((playerLayer.value & (1 << other.gameObject.layer)) == 0) return;
        }
        else
        {
            // fallback to tag
            if (!other.CompareTag("Player")) return;
        }

        // Optional: confirm player health component exists before triggering
        if (other.GetComponent<PlayerHealth>() == null)
        {
            // still allow trigger if tag was used as fallback
            if (!other.CompareTag("Player")) return;
        }

        parentBlock?.TriggerFall();
    }
}