using UnityEngine;
using System;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerHealth : MonoBehaviour
{
    [Header("Respawn")]
    public Transform respawnPoint; // assigned by checkpoint or left null to use initial spawn
    public float respawnDelay = 0.5f; // default ~0.5s
    public GameObject deathVFX;
    public AudioClip deathSFX;

    [Header("Lives")]
    public int startingLives = 3;

    public static event Action<int> OnLivesChanged; // broadcaster for UI

    Rigidbody2D rb;
    Collider2D col;
    AudioSource audioSource;
    int lives;
    bool dead = false;
    Vector3 initialPosition;

    // keep track of the previous bodyType so we can restore it after respawn
    RigidbodyType2D previousBodyType = RigidbodyType2D.Dynamic;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();
        audioSource = GetComponent<AudioSource>();
        initialPosition = transform.position;

        if (rb != null)
            previousBodyType = rb.bodyType;
    }

    void Start()
    {
        lives = startingLives;
        OnLivesChanged?.Invoke(lives);
        if (respawnPoint == null)
        {
            // use initial position as default respawn
            GameObject go = new GameObject("DefaultRespawnPoint");
            go.transform.position = initialPosition;
            respawnPoint = go.transform;
        }
    }

    // Called by traps or other systems to kill the player
    public void Die()
    {
        if (dead) return;
        dead = true;

        // decrement lives even if it goes negative (infinite lives)
        lives--;
        OnLivesChanged?.Invoke(lives);

        // VFX/SFX
        if (deathVFX) Instantiate(deathVFX, transform.position, Quaternion.identity);
        if (audioSource && deathSFX) audioSource.PlayOneShot(deathSFX);

        Debug.Log("[PlayerHealth] Die() called for " + gameObject.name);
        var spawner = GetComponent<SpawnBloodOnDeath>();
        if (spawner != null)
        {
            Debug.Log("[PlayerHealth] Calling spawner.SpawnAt at " + transform.position);
            spawner.SpawnAt(transform.position);
        }
        else
        {
            Debug.LogWarning("[PlayerHealth] No SpawnBloodOnDeath component found on " + gameObject.name);
        }

        StartCoroutine(RespawnRoutine());
    }

    IEnumerator RespawnRoutine()
    {
        // disable control
        var controller = GetComponent<PlayerController>();
        if (controller) controller.enabled = false;

        // zero velocity and disable physics interactions by making the body kinematic (store previous)
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            previousBodyType = rb.bodyType;
            rb.bodyType = RigidbodyType2D.Kinematic;
        }

        if (col != null) col.enabled = false;

        yield return new WaitForSeconds(respawnDelay);

        // move to respawn
        if (respawnPoint != null) transform.position = respawnPoint.position;

        // restore physics
        if (rb != null)
        {
            rb.bodyType = previousBodyType;
            rb.linearVelocity = Vector2.zero;
        }

        if (col != null) col.enabled = true;
        if (controller) controller.enabled = true;

        dead = false;
    }

    // Used by a checkpoint to set the respawn position
    public void SetRespawnPoint(Transform point)
    {
        respawnPoint = point;
    }
}