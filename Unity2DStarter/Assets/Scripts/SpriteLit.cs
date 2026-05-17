using UnityEngine;

/// <summary>
/// Shared URP 2D "Sprite-Lit" material for procedurally created sprites.
/// Without this, code-created SpriteRenderers use the unlit Sprites/Default
/// material and ignore Light2D entirely.
/// </summary>
public static class SpriteLit
{
    private static Material material;

    public static Material Material
    {
        get
        {
            if (material == null)
            {
                Shader shader = Shader.Find("Universal Render Pipeline/2D/Sprite-Lit-Default");
                if (shader != null)
                {
                    material = new Material(shader);
                }
            }

            return material;
        }
    }

    public static void Apply(SpriteRenderer renderer)
    {
        if (renderer == null)
        {
            return;
        }

        Material m = Material;
        if (m != null)
        {
            renderer.sharedMaterial = m;
        }
    }
}
