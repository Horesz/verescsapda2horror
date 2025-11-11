using UnityEngine;

/// <summary>
/// Small helper to make pickups float up and down.
/// Attach to the Visual child of the pickup (or root) and set amplitude/frequency.
/// </summary>
public class Bobbing : MonoBehaviour
{
    public float amplitude = 0.12f;
    public float frequency = 1.2f;

    Vector3 startPos;

    void Start()
    {
        startPos = transform.localPosition;
    }

    void Update()
    {
        float y = Mathf.Sin(Time.time * frequency) * amplitude;
        transform.localPosition = startPos + new Vector3(0f, y, 0f);
    }
}