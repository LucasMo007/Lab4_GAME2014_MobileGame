using UnityEngine;

/// <summary>Every sound effect in the game. Add a new name here + a clip in the Inspector.</summary>
public enum SoundFX
{
    Jump,
    Hurt,
    Checkpoint,
    Death
}

/// <summary>
/// One place that plays all music and sound effects.
/// Singleton: any script can call SoundManager.Instance.PlaySFX(SoundFX.Jump).
/// Uses two AudioSources: one looping for music, one for one-shot effects.
/// </summary>
public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance { get; private set; }

    [Header("Music")]
    public AudioClip backgroundMusic;
    [Range(0.0f, 1.0f)] public float musicVolume = 0.4f;

    [Header("Sound Effects")]
    public AudioClip jumpClip;
    public AudioClip hurtClip;
    public AudioClip checkpointClip;
    public AudioClip deathClip;
    [Range(0.0f, 1.0f)] public float sfxVolume = 0.8f;

    private AudioSource musicSource;
    private AudioSource sfxSource;

    void Awake()
    {
        // Only one SoundManager may exist (keeps music playing across scene loads)
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        musicSource = gameObject.AddComponent<AudioSource>();
        musicSource.loop = true;
        musicSource.playOnAwake = false;

        sfxSource = gameObject.AddComponent<AudioSource>();
        sfxSource.playOnAwake = false;
    }

    void Start()
    {
        PlayMusic(backgroundMusic);
    }

    public void PlayMusic(AudioClip clip)
    {
        if (clip == null || musicSource.clip == clip) return;
        musicSource.clip = clip;
        musicSource.volume = musicVolume;
        musicSource.Play();
    }

    public void StopMusic()
    {
        musicSource.Stop();
    }

    public void PlaySFX(SoundFX sound)
    {
        AudioClip clip = GetClip(sound);
        if (clip != null)
        {
            // PlayOneShot lets several effects overlap
            sfxSource.PlayOneShot(clip, sfxVolume);
        }
    }

    private AudioClip GetClip(SoundFX sound)
    {
        switch (sound)
        {
            case SoundFX.Jump: return jumpClip;
            case SoundFX.Hurt: return hurtClip;
            case SoundFX.Checkpoint: return checkpointClip;
            case SoundFX.Death: return deathClip;
            default: return null;
        }
    }
}
