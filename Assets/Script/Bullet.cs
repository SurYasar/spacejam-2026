using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float speed = 5f;

    [Range(1, 10)]
    [SerializeField] private float lifeTime = 2f;

    private void Start()
    {

    }
    private void Update()
    {
        // Peluru bergerak maju berdasarkan CustomDeltaTime dari TimeManager
        if (TimeManager.instance != null)
        {
            transform.Translate(Vector3.up * speed * TimeManager.instance.CustomDeltaTime);

            // Mengurangi lifetime peluru secara dinamis
            lifeTime -= TimeManager.instance.CustomDeltaTime;
            if (lifeTime <= 0f)
            {
                Destroy(gameObject);
            }
        }
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Peluru player hancur jika menabrak apa pun selain player itu sendiri
        if (!collision.CompareTag("Player"))
        {
            Destroy(gameObject);
        }
    }

}
