using UnityEditor;
using UnityEngine;
public class SpriteImportSettings : AssetPostprocessor
{
    private void OnPreprocessTexture()
    {
        if (!assetImporter.importSettingsMissing) return;
        if (!assetPath.StartsWith("Assets/Art/") && !assetPath.StartsWith("Assets/Resources/Player/") && !assetPath.StartsWith("Assets/Resources/Enemies/")) return;
        var importer = (TextureImporter)assetImporter;
        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Single;
        importer.spritePixelsPerUnit = 256;
        importer.filterMode = FilterMode.Point;
        importer.textureCompression = TextureImporterCompression.Uncompressed;
        importer.mipmapEnabled = false;
        importer.alphaIsTransparency = true;
        importer.wrapMode = TextureWrapMode.Clamp;
        importer.isReadable = !assetPath.StartsWith("Assets/Art/Authored/");
    }
}
