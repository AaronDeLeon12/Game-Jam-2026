using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

[InitializeOnLoad]
public static class StarterSceneBuilder
{
    private const string ScenePath = "Assets/Scenes/StarterScene.unity";
    private const string PlatformSpritePath = "Assets/Art/ground_square.png";

    static StarterSceneBuilder()
    {
        EditorApplication.delayCall += CreateStarterSceneIfNeeded;
    }

    [MenuItem("Tools/Starter Template/Rebuild Starter Scene")]
    public static void RebuildStarterScene()
    {
        CreateStarterScene(forceRebuild: true);
    }

    public static void RebuildStarterSceneFromCommandLine()
    {
        CreateStarterScene(forceRebuild: false);
    }

    private static void CreateStarterSceneIfNeeded()
    {
        if (File.Exists(ScenePath) && SceneIsCurrentPlatformerTemplate())
        {
            return;
        }

        CreateStarterScene(forceRebuild: false);
    }

    private static void CreateStarterScene(bool forceRebuild)
    {
        Directory.CreateDirectory("Assets/Scenes");
        Directory.CreateDirectory("Assets/Art");

        CreateSpriteTexture(PlatformSpritePath, new Color32(74, 78, 92, 255));
        AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);

        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        scene.name = "StarterScene";

        GameObject player = CreateSpriteObject("Player", PlatformSpritePath, new Vector3(0f, -1.75f, 0f), new Vector3(1f, 1f, 1f), 10);
        player.GetComponent<SpriteRenderer>().color = Color.white;
        Rigidbody2D body = player.AddComponent<Rigidbody2D>();
        body.gravityScale = 5f;
        body.freezeRotation = true;
        player.AddComponent<BoxCollider2D>();
        player.AddComponent<PlayerMovement2D>();
        player.AddComponent<PlayerStats>();
        player.AddComponent<PlayerCombat>();

        CreatePlatform("Infinite Floor", new Vector3(0f, -3f, 0f), new Vector3(2000f, 1f, 1f));

        GameObject cameraObject = new GameObject("Main Camera");
        Camera camera = cameraObject.AddComponent<Camera>();
        camera.orthographic = true;
        camera.orthographicSize = 5f;
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = new Color32(18, 20, 27, 255);
        cameraObject.tag = "MainCamera";
        cameraObject.transform.position = new Vector3(0f, 0f, -10f);
        cameraObject.AddComponent<AudioListener>();
        cameraObject.AddComponent<CameraFollow2D>().SetTarget(player.transform);

        GameObject systems = new GameObject("Game Systems");
        systems.AddComponent<GameTemplateBootstrap>();

        GameObject light = new GameObject("Global Light 2D Placeholder");
        light.transform.position = Vector3.zero;

        EditorSceneManager.SaveScene(scene, ScenePath);

        EditorBuildSettings.scenes = new[]
        {
            new EditorBuildSettingsScene(ScenePath, true)
        };

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        if (forceRebuild)
        {
            EditorUtility.DisplayDialog("Starter scene rebuilt", "StarterScene.unity has been rebuilt.", "OK");
        }
    }

    private static GameObject CreateSpriteObject(string name, string spritePath, Vector3 position, Vector3 scale, int sortingOrder)
    {
        GameObject obj = new GameObject(name);
        obj.transform.position = position;
        obj.transform.localScale = scale;

        SpriteRenderer renderer = obj.AddComponent<SpriteRenderer>();
        renderer.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(spritePath);
        if (renderer.sprite == null)
        {
            AssetDatabase.ImportAsset(spritePath, ImportAssetOptions.ForceSynchronousImport);
            renderer.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(spritePath);
        }

        renderer.sortingOrder = sortingOrder;

        return obj;
    }

    private static void CreatePlatform(string name, Vector3 position, Vector3 scale)
    {
        GameObject platform = CreateSpriteObject(name, PlatformSpritePath, position, scale, 0);
        platform.AddComponent<BoxCollider2D>();
    }

    private static bool SceneIsCurrentPlatformerTemplate()
    {
        string sceneText = File.ReadAllText(ScenePath);
        return sceneText.Contains("m_Name: Infinite Floor")
            && sceneText.Contains("GameTemplateBootstrap")
            && !sceneText.Contains("m_Sprite: {fileID: 0}");
    }

    private static void CreateSpriteTexture(string path, Color32 color)
    {
        if (File.Exists(path))
        {
            return;
        }

        Texture2D texture = new Texture2D(32, 32);
        Color32[] pixels = new Color32[32 * 32];

        for (int i = 0; i < pixels.Length; i++)
        {
            pixels[i] = color;
        }

        texture.SetPixels32(pixels);
        texture.Apply();
        File.WriteAllBytes(path, texture.EncodeToPNG());
        Object.DestroyImmediate(texture);

        AssetDatabase.ImportAsset(path);
        TextureImporter importer = (TextureImporter)AssetImporter.GetAtPath(path);
        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Single;
        importer.spritePixelsPerUnit = 32f;
        importer.filterMode = FilterMode.Point;
        importer.textureCompression = TextureImporterCompression.Uncompressed;
        importer.SaveAndReimport();
    }
}
