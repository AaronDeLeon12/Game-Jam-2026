using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Efectos de sonido")]
    public AudioClip spellCast;
    public AudioClip spellHit;
    public AudioClip playerHurt;

    [Header("Música")]
    public AudioClip backgroundMusic;

    private AudioSource _sfxSource;
    private AudioSource _musicSource;

    public static AudioManager EnsureExists()
    {
        if (Instance != null)
        {
            return Instance;
        }

        GameObject audioObject = new GameObject("Audio Manager");
        return audioObject.AddComponent<AudioManager>();
    }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            EnsureSources();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        EnsureSources();

        if (backgroundMusic != null)
        {
            PlayMusic(backgroundMusic);
        }
    }

    public void PlaySFX(AudioClip clip, float volume = 1f)
    {
        EnsureSources();

        if (clip != null)
        {
            _sfxSource.PlayOneShot(clip, volume);
        }
    }

    public void PlayNamedSFX(string clipName, float volume = 1f)
    {
        PlaySFX(GameAudio.LoadClip(clipName), volume);
    }

    public void PlayMusic(AudioClip clip, float volume = 0.45f)
    {
        EnsureSources();

        if (clip == null)
        {
            return;
        }

        if (_musicSource.clip == clip && _musicSource.isPlaying)
        {
            return;
        }

        _musicSource.clip = clip;
        _musicSource.loop = true;
        _musicSource.volume = volume;
        _musicSource.ignoreListenerPause = true;
        _musicSource.Play();
    }

    private void EnsureSources()
    {
        if (_sfxSource != null && _musicSource != null)
        {
            return;
        }

        AudioSource[] sources = GetComponents<AudioSource>();
        if (sources.Length >= 2)
        {
            _sfxSource = sources[0];
            _musicSource = sources[1];
        }
        else
        {
            _sfxSource = gameObject.AddComponent<AudioSource>();
            _musicSource = gameObject.AddComponent<AudioSource>();
        }

        _sfxSource.playOnAwake = false;
        _sfxSource.spatialBlend = 0f;
        _sfxSource.ignoreListenerPause = true;

        _musicSource.playOnAwake = false;
        _musicSource.spatialBlend = 0f;
        _musicSource.loop = true;
    }

    public void PlaySpellCast() => PlaySFX(spellCast);
    public void PlaySpellHit() => PlaySFX(spellHit);
    public void PlayPlayerHurt() => PlaySFX(playerHurt);
}
