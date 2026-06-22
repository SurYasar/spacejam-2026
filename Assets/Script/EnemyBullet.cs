using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyBullet : MonoBehaviour
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
        // Peluru musuh hancur jika menabrak apa pun selain sesama kelompok musuh
        if (!collision.CompareTag("Enemy") && !collision.CompareTag("EnemyBullet"))
        {
            Destroy(gameObject);
        }
    }
}
