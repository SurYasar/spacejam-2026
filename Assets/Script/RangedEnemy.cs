using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RangedEnemy : Enemy
{
    public GameObject enemyBulletPrefab;
    public float distanceToShoot = 5f;
    public float distanceToStop = 3f;

    private float timeToFire;
    public float fireRate;
    public Transform firePoint;

    public AudioClip shootSFX;

    protected override void Start()
    {
        base.Start(); // Memanggil start milik parent
    }

    protected override void Update()
    {
        base.Update();

        if (target == null)
        {
            GetTarget();
            return;
        }

        if (Vector2.Distance(target.position, transform.position) <= distanceToShoot)
        {
            Shoot();
        }

        
    }

    private void Shoot()
    {
        if (TimeManager.instance == null) return;

        if (timeToFire <= 0f)
        {
            Instantiate(enemyBulletPrefab, firePoint.position, firePoint.rotation);
            AudioManager.instance.PlaySFX(shootSFX);
            fireRate = Random.Range(1.5f, 3f); // Variasi pemicu tembakan (opsional)
            timeToFire = fireRate;
        }
        else
        {
            // Pengurangan cooldown tembakan musuh harus melambat saat player diam
            timeToFire -= TimeManager.instance.CustomDeltaTime;
        }

    }
    protected override void FixedUpdate()
    {
        // Safety check: jika target (player) atau TimeManager belum siap, langsung batalkan proses bawahnya
        if (target == null || TimeManager.instance == null)
            return;

        if (Vector2.Distance(target.position, transform.position) >= distanceToStop)
        {
            // Gunakan Time.fixedDeltaTime yang dikalikan dengan skala waktu saat ini
            float dynamicFixedDelta = Time.fixedDeltaTime * TimeManager.instance.CurrentTimeScale;

            Vector2 nextPosition = rb.position + (Vector2)(transform.up * speed * dynamicFixedDelta);
            rb.MovePosition(nextPosition);
        }
        else
        {
            rb.linearVelocity = Vector2.zero;
        }
        
        
    }

    protected override void GetTarget()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            target = player.transform;
        }
        else
        {
            Debug.LogWarning("Player not found! Make sure an object with tag 'Player' exists in the scene.");
        }
    }


    protected override void OnTriggerEnter2D(Collider2D other)
    {
        base.OnTriggerEnter2D(other);

        if (other.gameObject.CompareTag("Bullet"))
        {
            LevelManager.manager.IncreaseScore(3);
            Destroy(other.gameObject);
            AudioManager.instance.PlaySFX(hitsfx);
            Destroy(gameObject);
        }

    }
}
