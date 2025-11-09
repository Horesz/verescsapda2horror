using System.Collections.Generic;
using UnityEngine;

public class ParticlePool : MonoBehaviour
{
    public GameObject prefab;
    public int initialSize = 8;
    private Queue<GameObject> pool = new Queue<GameObject>();

    void Awake()
    {
        for (int i = 0; i < initialSize; i++)
        {
            var go = Instantiate(prefab);
            go.SetActive(false);
            pool.Enqueue(go);
        }
    }

    public GameObject Get(Vector3 pos, Quaternion rot)
    {
        GameObject go;
        if (pool.Count > 0)
        {
            go = pool.Dequeue();
            go.SetActive(true);
            go.transform.position = pos;
            go.transform.rotation = rot;
        }
        else
        {
            go = Instantiate(prefab, pos, rot);
        }

        var ps = go.GetComponent<ParticleSystem>();
        if (ps != null)
        {
            ps.Clear();
            ps.Play();
            StartCoroutine(ReturnAfter(ps.main.duration + ps.main.startLifetime.constantMax, go));
        }

        return go;
    }

    System.Collections.IEnumerator ReturnAfter(float seconds, GameObject go)
    {
        yield return new WaitForSeconds(seconds);
        go.SetActive(false);
        pool.Enqueue(go);
    }
}