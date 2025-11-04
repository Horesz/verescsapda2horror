using UnityEngine;

public class SpikeBall : MonoBehaviour
{
    public Transform leftPoint;
    public Transform rightPoint;
    public float speed = 3f;
    bool goingRight = true;

    void Reset()
    {
        leftPoint = new GameObject(name + "_Left").transform;
        rightPoint = new GameObject(name + "_Right").transform;
        leftPoint.parent = transform.parent;
        rightPoint.parent = transform.parent;
        leftPoint.position = transform.position + Vector3.left;
        rightPoint.position = transform.position + Vector3.right;
    }

    void Update()
    {
        if (leftPoint == null || rightPoint == null) return;

        Transform target = goingRight ? rightPoint : leftPoint;
        transform.position = Vector3.MoveTowards(transform.position, target.position, speed * Time.deltaTime);
        if (Vector3.Distance(transform.position, target.position) < 0.05f)
            goingRight = !goingRight;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        var ph = other.GetComponent<PlayerHealth>();
        if (ph != null) ph.Die();
    }
}