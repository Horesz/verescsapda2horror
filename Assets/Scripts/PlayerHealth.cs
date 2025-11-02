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

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();
        audioSource = GetComponent<AudioSource>();
        initialPosition = transform.position;
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

        StartCoroutine(RespawnRoutine());
    }

    IEnumerator RespawnRoutine()
    {
        // disable control
        var controller = GetComponent<PlayerController>();
        if (controller) controller.enabled = false;

        rb.linearVelocity = Vector2.zero;
        rb.isKinematic = true;
        col.enabled = false;

        yield return new WaitForSeconds(respawnDelay);

        // move to respawn
        if (respawnPoint != null) transform.position = respawnPoint.position;

        rb.isKinematic = false;
        col.enabled = true;
        if (controller) controller.enabled = true;

        dead = false;
    }

    // Used by a checkpoint to set the respawn position
    public void SetRespawnPoint(Transform point)
    {
        respawnPoint = point;
    }
}