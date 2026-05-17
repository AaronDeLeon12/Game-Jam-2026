using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Efectos de sonido")]
    public AudioClip spellCast;
    public AudioClip spellHit;
    public AudioClip playerHurt;

    [Header("Música")]
    public AudioClip backgroundMusic;

    private AudioSource _sfxSource;
    private AudioSource _musicSource;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        AudioSource[] sources = GetComponents<AudioSource>();
        _sfxSource   = sources[0];
        _musicSource = sources[1];

        if (backgroundMusic != null)
        {
            _musicSource.clip = backgroundMusic;
            _musicSource.loop = true;
            _musicSource.Play();
        }
    }

    public void PlaySFX(AudioClip clip)
    {
        if (clip != null)
            _sfxSource.PlayOneShot(clip);
    }

    public void PlaySpellCast()  => PlaySFX(spellCast);
    public void PlaySpellHit()   => PlaySFX(spellHit);
    public void PlayPlayerHurt() => PlaySFX(playerHurt);
}