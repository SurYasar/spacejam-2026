using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class LevelManager : MonoBehaviour
{
    public static LevelManager manager;

    public GameObject deathScreen;
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI highscoreText;

    [Header("Skill UI References")]
    public Image skillCooldownImage; // Image dengan isi warna gelap penutup skill

    public SaveData data;
    public int score;

    private void Awake()
    {
        manager = this;
        SaveSystem.Initialize();

        data = new SaveData(1);
    }

    public void GameOver()
    {
        deathScreen.SetActive(true);

        // 1. SEMBUNYIKAN HUD GAMEPLAY SECARA OTOMATIS
        // Mengakses objek teks langsung dari WaveManager agar tidak perlu drag & drop manual lagi
        if (WaveManager.instance != null)
        {
            if (WaveManager.instance.waveText != null) WaveManager.instance.waveText.gameObject.SetActive(false);
            if (WaveManager.instance.timerText != null) WaveManager.instance.timerText.gameObject.SetActive(false);
        }

        // Hapus semua enemy di scene
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        foreach (GameObject enemy in enemies)
        {
            Destroy(enemy);
        }

        // Hapus sisa peluru musuh
        GameObject[] enemyBullets = GameObject.FindGameObjectsWithTag("EnemyBullet");
        foreach (GameObject bullet in enemyBullets)
        {
            Destroy(bullet);
        }

        // 2. LOGIKA PENILAIAN BERBASIS WAVE
        int currentWaveReached = 1;
        if (WaveManager.instance != null)
        {
            currentWaveReached = WaveManager.instance.currentWave;
        }

        // Ubah teks tampilan di Death Screen
        scoreText.text = "Wave Reached: " + currentWaveReached.ToString();

        // Load data rekor wave tertinggi dari lokal storage
        string loadedData = SaveSystem.Load("save_wave"); // Menggunakan nama file baru agar tidak bentrok dengan save lama
        if (loadedData != null)
        {
            data = JsonUtility.FromJson<SaveData>(loadedData);
        }

        // Cek apakah pencapaian wave saat ini berhasil memecahkan rekor tertinggi
        if (data.highWave < currentWaveReached)
        {
            data.highWave = currentWaveReached;
        }

        highscoreText.text = "Highest Wave: " + data.highWave.ToString();

        // Simpan kembali data rekor terbaru
        string saveData = JsonUtility.ToJson(data);
        SaveSystem.Save("save_wave", saveData);
    }

    // FUNGSI BARU: Mengontrol Fill Amount Transparansi Penutup Gelap Gambar Skill
    public void UpdateSkillUI(float currentCooldown, float maxCooldown)
    {
        if (skillCooldownImage != null)
        {
            if (currentCooldown > 0f)
            {
                // Menghitung persentase sisa waktu cooldown (1 ke 0)
                skillCooldownImage.fillAmount = currentCooldown / maxCooldown;
            }
            else
            {
                skillCooldownImage.fillAmount = 0f;
            }
        }
    }
    public void ReplayGame()
    {
        // Pastikan Audio Pitch dikembalikan ke normal sebelum pindah scene
        if (AudioManager.instance && AudioManager.instance.sfxSource)
        {
            AudioManager.instance.sfxSource.pitch = 1f;
        }

        // Reset timeScale bawaan Unity ke normal demi keamanan UI/Scene baru
        Time.timeScale = 1f;

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void BackToMenu()
    {
        if (AudioManager.instance && AudioManager.instance.sfxSource)
        {
            AudioManager.instance.sfxSource.pitch = 1f;
        }
        Time.timeScale = 1f;

        // Ganti dengan nama scene menu kamu, misal "Menu"
        SceneManager.LoadScene("Menu");
    }

    public void IncreaseScore(int amount)
    {
        //score += amount;
    }
}

[System.Serializable]
public class SaveData
{
    public int highWave;
    public SaveData(int _hw)
    {
        highWave = _hw;
    }
}
