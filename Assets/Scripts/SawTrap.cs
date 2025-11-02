using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class SawTrap : MonoBehaviour
{
    [Header("Saw")]
    public float rotateSpeed = 360f; // degrees per second
    public bool rotateClockwise = true;

    void Update()
    {
        float dir = rotateClockwise ? -1f : 1f;
        transform.Rotate(Vector3.forward, dir * rotateSpeed * Time.deltaTime);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        var ph = other.GetComponent<PlayerHealth>();
        if (ph != null) ph.Die();
    }
}