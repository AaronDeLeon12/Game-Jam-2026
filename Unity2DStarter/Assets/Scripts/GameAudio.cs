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

        AudioSource.PlayClipAtPoint(clip, position, volume);
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
