using UnityEngine;
using UnityEngine.Rendering; // Diperlukan untuk mengakses Global Volume
using UnityEngine.Rendering.Universal; // Diperlukan untuk mengakses efek URP

public class TimeManager : MonoBehaviour
{
    public static TimeManager instance;

    [Header("Time Settings")]
    public float slowTimeScale = 0.05f;
    public float normalTimeScale = 1f;

    [Header("VFX Settings")]
    [Tooltip("Tarik objek Global Volume dari Hierarchy ke sini")]
    public Volume globalVolume;
    public float maxChromaticAberration = 0.6f;
    public float maxVignetteIntensity = 0.45f;
    public float slowMoSaturation = -40f;

    private Camera mainCamera;
    private float baseFOV;

    private ChromaticAberration chromaticAberration;
    private Vignette vignette;
    private ColorAdjustments colorAdjustments;

    public float CustomDeltaTime { get; private set; }
    public float CurrentTimeScale { get; private set; }

    private bool isPlayerMovingOrShooting = false;
    private void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);

        if (globalVolume != null && globalVolume.profile != null)
        {
            globalVolume.profile.TryGet(out chromaticAberration);
            globalVolume.profile.TryGet(out vignette);
            globalVolume.profile.TryGet(out colorAdjustments);
        }
    }

    private void Start()
    {
        mainCamera = Camera.main;
        if (mainCamera != null)
        {
            baseFOV = mainCamera.fieldOfView;
        }
    }

    private void Update()
    {
        float targetTimeScale = isPlayerMovingOrShooting ? normalTimeScale : slowTimeScale;

        CurrentTimeScale = Mathf.MoveTowards(CurrentTimeScale, targetTimeScale, Time.unscaledDeltaTime * 10f);

        CustomDeltaTime = Time.unscaledDeltaTime * CurrentTimeScale;

        if (AudioManager.instance && AudioManager.instance.sfxSource)
        {
            AudioManager.instance.sfxSource.pitch = CurrentTimeScale;
        }
        if (mainCamera != null)
        {
            float targetFOV = Mathf.Lerp(baseFOV + 3f, baseFOV, (CurrentTimeScale - slowTimeScale) / (normalTimeScale - slowTimeScale));
            mainCamera.fieldOfView = Mathf.MoveTowards(mainCamera.fieldOfView, targetFOV, Time.unscaledDeltaTime * 5f);
        }
        if (globalVolume != null)
        {
            // Hitung faktor kelambatan waktu (0 saat normal, 1 saat lambat total)
            float slowFactor = (normalTimeScale - CurrentTimeScale) / (normalTimeScale - slowTimeScale);
            slowFactor = Mathf.Clamp01(slowFactor); // Jaga nilai tetap di antara 0 dan 1

            // 1. Blend Chromatic Aberration
            if (chromaticAberration != null)
            {
                chromaticAberration.intensity.value = Mathf.Lerp(0f, maxChromaticAberration, slowFactor);
            }

            // 2. Blend Vignette
            if (vignette != null)
            {
                vignette.intensity.value = Mathf.Lerp(0f, maxVignetteIntensity, slowFactor);
            }

            // 3. Blend Saturation Warna
            if (colorAdjustments != null)
            {
                colorAdjustments.saturation.value = Mathf.Lerp(0f, slowMoSaturation, slowFactor);
            }
        }
    }

    public void SetPlayerState(bool isActive)
    {
        isPlayerMovingOrShooting = isActive;
    }
}
