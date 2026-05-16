using System.Collections;
using UnityEngine;

public class HitFlash2D : MonoBehaviour
{
    private Coroutine activeFlash;

    public static void Play(GameObject owner, Color flashColor, float duration = 0.08f)
    {
        if (owner == null)
        {
            return;
        }

        HitFlash2D flash = owner.GetComponent<HitFlash2D>();
        if (flash == null)
        {
            flash = owner.AddComponent<HitFlash2D>();
        }

        flash.StartFlash(flashColor, duration);
    }

    private void StartFlash(Color flashColor, float duration)
    {
        if (activeFlash != null)
        {
            StopCoroutine(activeFlash);
        }

        activeFlash = StartCoroutine(FlashRoutine(flashColor, duration));
    }

    private IEnumerator FlashRoutine(Color flashColor, float duration)
    {
        SpriteRenderer[] renderers = GetComponentsInChildren<SpriteRenderer>();
        Color[] originalColors = new Color[renderers.Length];

        for (int i = 0; i < renderers.Length; i++)
        {
            originalColors[i] = renderers[i].color;
            renderers[i].color = flashColor;
        }

        yield return new WaitForSeconds(duration);

        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i] != null)
            {
                renderers[i].color = originalColors[i];
            }
        }

        activeFlash = null;
    }
}
