using System.Collections.Generic;
using UnityEngine;

public static class GameAudio
{
    private static readonly Dictionary<string, AudioClip> ClipCache = new Dictionary<string, AudioClip>();

    public static void PlaySfx(string clipName, Vector3 position, float volume = 1f)
    {
        AudioClip clip = LoadClip(clipName);
        if (clip == null)
        {
            return;
        }

        AudioManager.EnsureExists().PlaySFX(clip, volume);
    }

    public static void PlayMusic(string clipName, float volume = 0.45f)
    {
        AudioClip clip = LoadClip(clipName);
        if (clip == null)
        {
            return;
        }

        AudioManager.EnsureExists().PlayMusic(clip, volume);
    }

    public static void PlayOneShotAtPoint(string clipName, Vector3 position, float volume = 1f)
    {
        AudioClip clip = LoadClip(clipName);
        if (clip == null)
        {
            return;
        }

        GameObject audioObject = new GameObject("One Shot SFX");
        audioObject.transform.position = position;

        AudioSource source = audioObject.AddComponent<AudioSource>();
        source.clip = clip;
        source.volume = volume;
        source.spatialBlend = 0f;
        source.ignoreListenerPause = true;
        source.Play();

        Object.Destroy(audioObject, clip.length + 0.1f);
    }

    public static AudioClip LoadClip(string clipName)
    {
        if (ClipCache.TryGetValue(clipName, out AudioClip clip))
        {
            return clip;
        }

        clip = Resources.Load<AudioClip>("Audio/" + clipName);
        if (clip != null)
        {
            ClipCache.Add(clipName, clip);
        }

        return clip;
    }
}
