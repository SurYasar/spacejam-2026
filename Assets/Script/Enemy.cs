using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    public Transform target;
    public float speed = 3f;
    public float rotateSpeed = 0.02f;
    public Rigidbody2D rb;

    private float arenaLimit = 20f;

    public AudioClip hitsfx;
    public AudioClip playerHit;

    protected virtual void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    protected virtual void Update()
    {
        if (!target)
        {
            GetTarget();
        }
        else
        {
            RotateTowardsTarget();
        }
        CheckOutOfBounds();
    }

    private void CheckOutOfBounds()
    {
        if (Mathf.Abs(transform.position.x) > arenaLimit || Mathf.Abs(transform.position.y) > arenaLimit)
        {
            Destroy(gameObject);
        }
    }

    protected virtual void FixedUpdate()
    {
        // Pergerakan menggunakan posisi Rigidbody + arah depan dikali CustomDeltaTime
        // Safety check untuk base enemy
        if (TimeManager.instance == null) return;
        // Gunakan Time.fixedDeltaTime yang dikalikan dengan skala waktu saat ini
        float dynamicFixedDelta = Time.fixedDeltaTime * TimeManager.instance.CurrentTimeScale;

        Vector2 nextPosition = rb.position + (Vector2)(transform.up * speed * dynamicFixedDelta);
        rb.MovePosition(nextPosition);
    }

    protected virtual void RotateTowardsTarget()
    {
        if (TimeManager.instance == null) return;

        Vector2 targetDirection = target.position - transform.position;
        float angle = Mathf.Atan2(targetDirection.y, targetDirection.x) * Mathf.Rad2Deg - 90f;
        Quaternion q = Quaternion.Euler(new Vector3(0, 0, angle));

        // Kecepatan rotasi (Slerp) dikalikan dengan CurrentTimeScale agar ikut melambat secara visual
        float dynamicRotateSpeed = rotateSpeed * TimeManager.instance.CurrentTimeScale;
        transform.localRotation = Quaternion.Slerp(transform.localRotation, q, dynamicRotateSpeed);
    }
    protected virtual void GetTarget()
    {
        if (GameObject.FindGameObjectWithTag("Player"))
        {
            target = GameObject.FindGameObjectWithTag("Player").transform;
        }
    }
    protected virtual void OnCollisionEnter2D(Collision2D other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            LevelManager.manager.GameOver();
            AudioManager.instance.PlaySFX(playerHit);
            Destroy(other.gameObject);
            target = null;
        }
    }
    protected virtual void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Bullet"))
        {
            if (speed <= 5f)
            {
                LevelManager.manager.IncreaseScore(1);
            }
            else
            {
                LevelManager.manager.IncreaseScore(2);
            }

            Destroy(other.gameObject);

            AudioManager.instance.PlaySFX(hitsfx);

            Destroy(gameObject);
        }

    }
}
