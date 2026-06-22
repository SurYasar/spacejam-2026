using UnityEngine;

public class BGMManager : MonoBehaviour
{
    public static BGMManager instance;

    [Header("Audio Clips")]
    [Tooltip("Varian 1: Dimainkan saat Break Phase")]
    public AudioClip breakClip;
    [Tooltip("Varian 2: Dimainkan saat Battle (Player bergerak)")]
    public AudioClip battleMoveClip;
    [Tooltip("Varian 3: Dimainkan saat Battle (Player diam/Slow-mo)")]
    public AudioClip battleSlowClip;

    [Header("Volume Settings")]
    [Range(0f, 1f)]
    public float maxVolume = 0.5f;     // Batas volume maksimal agar tidak terlalu keras
    public float fadeSpeed = 5f;       // Kecepatan transisi perpindahan volume

    private AudioSource sourceBreak;
    private AudioSource sourceBattleMove;
    private AudioSource sourceBattleSlow;

    private void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);

        // Membuat 3 AudioSource secara otomatis pada komponen ini agar rapi
        sourceBreak = SetupAudioSource(breakClip);
        sourceBattleMove = SetupAudioSource(battleMoveClip);
        sourceBattleSlow = SetupAudioSource(battleSlowClip);
    }

    private void Start()
    {
        // Mainkan ketiga varian secara bersamaan sejak awal game
        sourceBreak.Play();
        sourceBattleMove.Play();
        sourceBattleSlow.Play();
    }

    private AudioSource SetupAudioSource(AudioClip clip)
    {
        AudioSource source = gameObject.AddComponent<AudioSource>();
        source.clip = clip;
        source.loop = true;
        source.volume = 0f; // Mulai dari volume kosong
        source.playOnAwake = false;
        return source;
    }

    private void Update()
    {
        float targetVol1 = 0f;
        float targetVol2 = 0f;
        float targetVol3 = 0f;

        // 1. KONDISI SAAT FASE BREAK (Varian 1 menyala, sisanya mati)
        if (WaveManager.instance != null && WaveManager.instance.isBreakPhase)
        {
            targetVol1 = maxVolume;
            targetVol2 = 0f;
            targetVol3 = 0f;
        }
        // 2. KONDISI SAAT FASE BATTLE (Varian 2 & 3 saling crossfade berdasarkan skala waktu)
        else if (TimeManager.instance != null)
        {
            targetVol1 = 0f;

            // Hitung persentase kelambatan waktu saat ini (0 = lambat total, 1 = waktu normal)
            float t = Mathf.InverseLerp(TimeManager.instance.slowTimeScale, TimeManager.instance.normalTimeScale, TimeManager.instance.CurrentTimeScale);

            targetVol2 = t * maxVolume;          // Bergerak -> Varian 2 makin keras
            targetVol3 = (1f - t) * maxVolume;   // Diam -> Varian 3 makin keras
        }

        // Aplikasikan perubahan volume secara smooth menggunakan unscaledDeltaTime
        sourceBreak.volume = Mathf.MoveTowards(sourceBreak.volume, targetVol1, fadeSpeed * Time.unscaledDeltaTime);
        sourceBattleMove.volume = Mathf.MoveTowards(sourceBattleMove.volume, targetVol2, fadeSpeed * Time.unscaledDeltaTime);
        sourceBattleSlow.volume = Mathf.MoveTowards(sourceBattleSlow.volume, targetVol3, fadeSpeed * Time.unscaledDeltaTime);
    }
}