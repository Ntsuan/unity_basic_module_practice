using UnityEngine;

public class PracticeFeedback : MonoBehaviour
{
    private static PracticeFeedback instance;

    [SerializeField] private float masterVolume = 0.6f;
    [SerializeField] private float hitShakeDuration = 0.14f;
    [SerializeField] private float hitShakeStrength = 0.24f;

    private AudioSource audioSource;
    private AudioClip shootClip;
    private AudioClip reloadClip;
    private AudioClip hurtClip;
    private AudioClip pickupClip;
    private AudioClip enemyDefeatedClip;
    private AudioClip enemyRespawnClip;
    private float shakeEndTime;
    private float shakeStrength;
    private float shakeSeed;

    public static PracticeFeedback Instance
    {
        get
        {
            if (instance == null)
            {
                GameObject feedbackObject = new GameObject("Practice Feedback");
                instance = feedbackObject.AddComponent<PracticeFeedback>();
            }

            return instance;
        }
    }

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 0f;
        audioSource.volume = Mathf.Clamp01(Mathf.Max(masterVolume, 0.55f));

        EnsureAudioListener();

        shootClip = CreateToneClip("Practice Shoot", 780f, 520f, 0.07f, 0.48f);
        reloadClip = CreateToneClip("Practice Reload", 340f, 480f, 0.18f, 0.42f);
        hurtClip = CreateToneClip("Practice Hurt", 170f, 95f, 0.2f, 0.62f);
        pickupClip = CreateToneClip("Practice Pickup", 960f, 1280f, 0.1f, 0.38f);
        enemyDefeatedClip = CreateToneClip("Practice Enemy Defeated", 240f, 70f, 0.24f, 0.58f);
        enemyRespawnClip = CreateToneClip("Practice Enemy Respawn", 320f, 760f, 0.22f, 0.45f);
    }

    public static void PlayShoot()
    {
        Instance.PlayClip(Instance.shootClip, 0.78f);
    }

    public static void PlayReload()
    {
        Instance.PlayClip(Instance.reloadClip, 0.78f);
    }

    public static void PlayHurt()
    {
        PracticeFeedback feedback = Instance;
        feedback.PlayClip(feedback.hurtClip, 1f);
        feedback.TriggerShake(feedback.hitShakeDuration, feedback.hitShakeStrength);
    }

    public static void PlayPickup()
    {
        Instance.PlayClip(Instance.pickupClip, 0.68f);
    }

    public static void PlayEnemyDefeated()
    {
        Instance.PlayClip(Instance.enemyDefeatedClip, 0.9f);
    }

    public static void PlayEnemyRespawn()
    {
        Instance.PlayClip(Instance.enemyRespawnClip, 0.78f);
    }

    public static Vector3 GetCameraShakeOffset()
    {
        if (instance == null || Time.time >= instance.shakeEndTime)
        {
            return Vector3.zero;
        }

        float remaining = Mathf.Clamp01((instance.shakeEndTime - Time.time) / instance.hitShakeDuration);
        float strength = instance.shakeStrength * remaining;
        float x = (Mathf.PerlinNoise(instance.shakeSeed, Time.time * 38f) - 0.5f) * 2f * strength;
        float z = (Mathf.PerlinNoise(instance.shakeSeed + 12.34f, Time.time * 38f) - 0.5f) * 2f * strength;
        return new Vector3(x, 0f, z);
    }

    private void PlayClip(AudioClip clip, float volumeScale)
    {
        if (audioSource == null || clip == null)
        {
            return;
        }

        audioSource.PlayOneShot(clip, volumeScale);
    }

    private void EnsureAudioListener()
    {
        if (FindObjectOfType<AudioListener>() != null)
        {
            return;
        }

        Camera mainCamera = Camera.main;
        if (mainCamera != null)
        {
            mainCamera.gameObject.AddComponent<AudioListener>();
            return;
        }

        gameObject.AddComponent<AudioListener>();
    }

    private void TriggerShake(float duration, float strength)
    {
        shakeEndTime = Time.time + duration;
        shakeStrength = strength;
        shakeSeed = Random.Range(0f, 1000f);
    }

    private AudioClip CreateToneClip(string clipName, float startFrequency, float endFrequency, float duration, float volume)
    {
        const int sampleRate = 44100;
        int sampleCount = Mathf.CeilToInt(sampleRate * duration);
        float[] samples = new float[sampleCount];

        for (int i = 0; i < sampleCount; i++)
        {
            float t = i / (float)sampleRate;
            float normalizedTime = Mathf.Clamp01(t / duration);
            float frequency = Mathf.Lerp(startFrequency, endFrequency, normalizedTime);
            float attack = Mathf.Clamp01(normalizedTime / 0.08f);
            float decay = 1f - normalizedTime;
            float envelope = attack * decay * decay;
            samples[i] = Mathf.Sin(2f * Mathf.PI * frequency * t) * envelope * volume;
        }

        AudioClip clip = AudioClip.Create(clipName, sampleCount, 1, sampleRate, false);
        clip.SetData(samples, 0);
        return clip;
    }
}
