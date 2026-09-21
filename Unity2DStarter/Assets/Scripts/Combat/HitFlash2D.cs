using System.Collections;
using UnityEngine;

public class HitFlash2D : MonoBehaviour
{
    private Coroutine activeFlash;
    private SpriteRenderer[] flashingRenderers;
    private Color[] originalColors;

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
            RestoreOriginalColors();
        }

        activeFlash = StartCoroutine(FlashRoutine(flashColor, duration));
    }

    private IEnumerator FlashRoutine(Color flashColor, float duration)
    {
        flashingRenderers = GetComponentsInChildren<SpriteRenderer>();
        originalColors = new Color[flashingRenderers.Length];

        for (int i = 0; i < flashingRenderers.Length; i++)
        {
            originalColors[i] = flashingRenderers[i].color;
            flashingRenderers[i].color = flashColor;
        }

        yield return new WaitForSeconds(duration);

        RestoreOriginalColors();
        activeFlash = null;
    }

    private void RestoreOriginalColors()
    {
        if (flashingRenderers == null || originalColors == null)
        {
            return;
        }

        for (int i = 0; i < flashingRenderers.Length; i++)
        {
            if (flashingRenderers[i] != null && i < originalColors.Length)
            {
                flashingRenderers[i].color = originalColors[i];
            }
        }

        flashingRenderers = null;
        originalColors = null;
    }
}
