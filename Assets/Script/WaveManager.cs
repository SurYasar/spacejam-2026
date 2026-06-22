using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class WaveManager : MonoBehaviour
{
    public static WaveManager instance;

    [System.Serializable]
    public class EnemyConfig
    {
        public GameObject enemyPrefab;
        public float baseSpeed = 3f; // Kecepatan awal musuh tipe ini di Wave 1
    }

    [Header("Wave Settings")]
    public EnemyConfig[] enemyTypes;
    public Transform[] spawnPoints;
    public Transform player;

    public float waveDuration = 30f; // Durasi per wave dalam hitungan detik nyata
    public float breakDuration = 10f; // Durasi jeda antar wave (10 detik)
    public float baseSpawnRate = 2f; // Jeda spawn awal (semakin kecil semakin rapat)
    public float speedMultiplierPerWave = 1.15f; // Musuh bertambah cepat 15% setiap wave baru

    [Header("UI References (Optional)")]
    public TextMeshProUGUI waveText;
    public TextMeshProUGUI timerText;

    // Status Wave Saat Ini
    public int currentWave { get; private set; } = 1;
    private float waveTimer;
    private float spawnTimer;
    public bool isBreakPhase { get; private set; } = false;
    private bool isWaveActive = true;

    //[SerializeField] private float spawnRate = 1f;
    //[SerializeField] private GameObject[] enemyPrefabs;
    //[SerializeField] private bool canSpawn = true;
    private void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        //StartCoroutine(Spawner());
        waveTimer = waveDuration;
        UpdateUI();

        // Otomatis mencari player jika belum di-drag di Inspector
        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null) player = playerObj.transform;
        }
    }
    private void Update()
    {
        if (!isWaveActive || LevelManager.manager.deathScreen.activeSelf) return;

        // PERUBAHAN: Pengurangan timer wave dibuat dinamis berdasarkan fase
        if (isBreakPhase)
        {
            // Saat fase BREAK, timer berjalan normal di dunia nyata (konstan)
            waveTimer -= Time.unscaledDeltaTime;
        }
        else
        {
            // Saat fase BATTLE, timer ikut melambat jika player diam (menggunakan CustomDeltaTime)
            if (TimeManager.instance != null)
            {
                waveTimer -= TimeManager.instance.CustomDeltaTime;
            }
            else
            {
                waveTimer -= Time.deltaTime;
            }
        }

        if (timerText != null)
        {
            if (isBreakPhase)
            {
                // Jika sedang jeda, tampilkan teks "Break"
                timerText.text = "Next Wave in: " + Mathf.Max(0, Mathf.CeilToInt(waveTimer)).ToString() + "s";
            }
            else
            {
                // Jika wave aktif, tampilkan sisa waktu wave
                timerText.text = "Time Left: " + Mathf.Max(0, Mathf.CeilToInt(waveTimer)).ToString() + "s";
            }
        }

        if (waveTimer <= 0f)
        {
            if (!isBreakPhase)
            {
                // Jika wave habis, masuk ke masa Jeda (Break)
                StartBreakPhase();
            }
            else
            {
                // Jika masa jeda habis, masuk ke Wave berikutnya
                StartNextWave();
            }
        }

        // Logic Spawning Musuh (Jeda spawn mematuhi putaran waktu game/CustomDeltaTime)
        if (!isBreakPhase && TimeManager.instance != null)
        {
            spawnTimer -= TimeManager.instance.CustomDeltaTime;
            if (spawnTimer <= 0f)
            {
                SpawnEnemy();
                // Menghitung jeda spawn dinamis (bisa dibuat makin cepat di wave tinggi jika mau)
                spawnTimer = baseSpawnRate / Mathf.Pow(1.05f, currentWave - 1);
            }
        }
    }
    private void SpawnEnemy()
    {
        // Pastikan player, tipe musuh, dan spawn points sudah siap
        if (player == null || enemyTypes.Length == 0 || spawnPoints.Length == 0) return;

        // 1. Sinkronisasi posisi WaveManager agar selalu mengikuti koordinat Player (seperti Spawner lama)
        Vector3 newPosition = new Vector3(player.position.x, player.position.y, transform.position.z);
        transform.position = newPosition;

        // 2. Filter Spawn Points yang valid (berada di dalam batas arena -20 hingga 20)
        List<Transform> validSpawnPoints = new List<Transform>();
        foreach (Transform spawnPoint in spawnPoints)
        {
            if (spawnPoint.position.x >= -20f && spawnPoint.position.x <= 20f &&
                spawnPoint.position.y >= -20f && spawnPoint.position.y <= 20f)
            {
                validSpawnPoints.Add(spawnPoint);
            }
        }

        // Jika tidak ada titik spawn yang valid di dalam batas arena, batalkan proses spawn
        if (validSpawnPoints.Count == 0) return;

        // 3. Ambil tipe musuh dan titik spawn valid secara acak
        int randomEnemyIdx = Random.Range(0, enemyTypes.Length);
        Transform randomSpawnPoint = validSpawnPoints[Random.Range(0, validSpawnPoints.Count)];

        EnemyConfig selectedEnemy = enemyTypes[randomEnemyIdx];

        // 4. Spawn musuh ke scene
        GameObject spawnedEnemyObj = Instantiate(selectedEnemy.enemyPrefab, randomSpawnPoint.position, Quaternion.identity);
        Enemy enemyScript = spawnedEnemyObj.GetComponent<Enemy>();

        if (enemyScript != null)
        {
            // HITUNG SCALE-UP KECEPATAN BERDASARKAN WAVE
            float upgradedSpeed = selectedEnemy.baseSpeed * Mathf.Pow(speedMultiplierPerWave, currentWave - 1);
            enemyScript.speed = upgradedSpeed;
        }
    }
    // TAMBAHAN: Fungsi untuk memulai masa istirahat
    private void StartBreakPhase()
    {
        isBreakPhase = true;
        waveTimer = breakDuration; // Atur timer ke 10 detik jeda

        // TAMBAHAN: Bersihkan semua musuh yang masih hidup di arena saat wave clear
        GameObject[] remainingEnemies = GameObject.FindGameObjectsWithTag("Enemy");
        foreach (GameObject enemy in remainingEnemies)
        {
            Destroy(enemy);
        }
        // Bersihkan sisa peluru musuh agar aman saat istirahat
        GameObject[] enemyBullets = GameObject.FindGameObjectsWithTag("EnemyBullet");
        foreach (GameObject bullet in enemyBullets) Destroy(bullet);

        if (waveText != null)
        {
            waveText.text = "Wave " + currentWave.ToString() + " Clear!";
        }
    }
    
    // Fungsi untuk memulai wave baru setelah jeda selesai
    private void StartNextWave()
    {
        currentWave++;
        isBreakPhase = false;
        waveTimer = waveDuration; // Reset timer kembali ke durasi wave (30 detik)

        UpdateUI();
    }
    private void UpdateUI()
    {
        if (waveText != null)
        {
            waveText.text = "Wave: " + currentWave.ToString();
        }
    }

    //private IEnumerator Spawner()
    //{
    //    WaitForSeconds wait = new WaitForSeconds(spawnRate);

    //    while (canSpawn)
    //    {
    //        yield return wait;
    //        int rand = Random.Range(0, enemyPrefabs.Length);
    //        GameObject enemyToSpawn = enemyPrefabs[rand];

    //        Instantiate(enemyToSpawn, transform.position, Quaternion.identity);
    //    }
    //}
}
