using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Collider2D))]
public class SpikeEmerging : MonoBehaviour
{
    [Header("Movement")]
    public Vector3 raisedOffset = new Vector3(0, 0.6f, 0);
    public float raiseDuration = 0.12f;
    public float stayUpTime = 0.6f;
    public float retractDuration = 0.12f;

    [Header("Detection")]
    public LayerMask playerLayer;
    public Collider2D spikeCollider; // collider that actually hurts the player

    Vector3 hiddenLocalPos;
    Vector3 raisedLocalPos;
    bool busy = false;

    void Start()
    {
        hiddenLocalPos = transform.localPosition;
        raisedLocalPos = hiddenLocalPos + raisedOffset;
        if (spikeCollider == null) spikeCollider = GetComponent<Collider2D>();
        spikeCollider.enabled = false;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (((1 << other.gameObject.layer) & playerLayer) == 0) return;

        if (!busy)
            StartCoroutine(RaiseCycle());
    }

    IEnumerator RaiseCycle()
    {
        busy = true;
        // raise
        yield return AnimatePosition(transform, hiddenLocalPos, raisedLocalPos, raiseDuration);
        spikeCollider.enabled = true;

        yield return new WaitForSeconds(stayUpTime);

        // retract
        spikeCollider.enabled = false;
        yield return AnimatePosition(transform, raisedLocalPos, hiddenLocalPos, retractDuration);
        busy = false;
    }

    IEnumerator AnimatePosition(Transform t, Vector3 from, Vector3 to, float dur)
    {
        float elapsed = 0f;
        while (elapsed < dur)
        {
            elapsed += Time.deltaTime;
            t.localPosition = Vector3.Lerp(from, to, elapsed / dur);
            yield return null;
        }
        t.localPosition = to;
    }
}