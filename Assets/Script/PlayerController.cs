using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 5f;
    public Rigidbody2D rb;
    public Weapon weapon;

    public AudioSource src;
    public AudioClip shootsfx;
    public AudioClip hitsfx;
    public AudioClip swapSFX; // Slot baru untuk audio skill swap

    [Header("Skill Swap Settings")]
    public float swapCooldown = 10f;
    private float cooldownTimer = 0f;

    private Vector2 moveDirection;
    private Vector2 mousePosition;

    private float fireTimer;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        float moveX = Input.GetAxisRaw("Horizontal");
        float moveY = Input.GetAxisRaw("Vertical");

        mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        moveDirection = new Vector2(moveX, moveY).normalized;

        bool isMoving = moveDirection.magnitude > 0;
        bool isShooting = Input.GetMouseButton(0);

        // Beritahu TimeManager jika player sedang bergerak ATAU menembak
        if (TimeManager.instance != null)
        {
            TimeManager.instance.SetPlayerState(isMoving || isShooting);
        }
        // LOGIKA PENGURANGAN COOLDOWN SKILL (Menggunakan unscaledDeltaTime agar tidak lambat saat player diam)
        if (cooldownTimer > 0f)
        {
            // Cek apakah WaveManager sedang dalam fase istirahat (break) atau tidak
            if (WaveManager.instance != null && WaveManager.instance.isBreakPhase)
            {
                cooldownTimer -= Time.unscaledDeltaTime; // Fase break: normal
            }
            else if (TimeManager.instance != null)
            {
                cooldownTimer -= TimeManager.instance.CustomDeltaTime; // Fase battle: ikut slowmo
            }
        }

        // UPDATE DATA COOLDOWN KE UI
        if (LevelManager.manager != null)
        {
            LevelManager.manager.UpdateSkillUI(cooldownTimer, swapCooldown);
        }

        // DETEKSI INPUT TOMBOL E
        if (Input.GetKeyDown(KeyCode.E) && cooldownTimer <= 0f)
        {
            TryExecuteSwap();
        }

        if (isShooting && fireTimer <= 0f)
        {
            weapon.Fire();
            src.clip = shootsfx;
            src.Play();
            fireTimer = weapon.fireRate;
        }
        else
        {
            // Pengurangan cooldown tembakan harus menggunakan unscaledDeltaTime 
            // agar player tidak macet menembak saat waktu melambat
            fireTimer -= Time.unscaledDeltaTime;
        }

    }
    private void TryExecuteSwap()
    {
        // Deteksi apakah ada Collider2D tepat di posisi mouse cursor
        Collider2D hit = Physics2D.OverlapPoint(mousePosition);

        if (hit != null && hit.CompareTag("Enemy"))
        {
            // Ambil referensi posisi musuh
            Vector3 enemyPos = hit.transform.position;
            Vector3 playerPos = transform.position;

            // Tukar posisi keduanya (Z sumbu tetap dipertahankan 0)
            transform.position = new Vector3(enemyPos.x, enemyPos.y, playerPos.z);
            hit.transform.position = new Vector3(playerPos.x, playerPos.y, enemyPos.z);

            // Trigger sfx dan aktifkan cooldown
            if (swapSFX != null && AudioManager.instance != null)
            {
                AudioManager.instance.PlaySFX(swapSFX);
            }

            cooldownTimer = swapCooldown;
        }
    }
    private void FixedUpdate()
    {
        rb.linearVelocity = new Vector2(moveDirection.x * moveSpeed, moveDirection.y * moveSpeed);

        Vector2 aimDirection = mousePosition - rb.position;
        float aimAngle = Mathf.Atan2(aimDirection.y, aimDirection.x) * Mathf.Rad2Deg - 90f;
        rb.rotation = aimAngle;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("EnemyBullet"))
        {
            AudioManager.instance.PlaySFX(hitsfx);
            LevelManager.manager.GameOver();
            Destroy(gameObject);
        }
    }
}
