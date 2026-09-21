#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

public static class SceneAuthoringTools
{
    private const string OutsideScenePath = "Assets/Scenes/Campaign/outside_1.unity";

    [MenuItem("Tools/Authoring/Materialize/outside_1 Scene Content")]
    public static void MaterializeOutsideScene()
    {
        if (System.IO.File.Exists("Assets/CampaignAuthoringComplete.txt"))
        { Debug.LogWarning("Campaign authoring is complete. Edit the saved prefabs and scenes directly; legacy generation is disabled."); return; }
        Scene scene = EditorSceneManager.OpenScene(OutsideScenePath, OpenSceneMode.Single);

        EnsureOutsideController();
        EnsureDaylight();
        EnsureSpawnPoint();
        EnsureVendorHouse();
        EnsureSaveVendor();
        EnsureReturnDoor();
        EnsureOutsideFloor();
        EnsureEnemySpawnMarker("Mantis Spawn Marker", new Vector3(3f, 1.5f, 0f));
        DisableRuntimeFallbacks();

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log("outside_1 is now materialized for editor authoring. Move objects in Edit Mode and save the scene.");
    }

    private static void EnsureOutsideController()
    {
        OutsideScene outside = Object.FindAnyObjectByType<OutsideScene>();
        if (outside == null)
        {
            GameObject obj = new GameObject("Outside Scene Controller");
            outside = obj.AddComponent<OutsideScene>();
        }
    }

    private static void DisableRuntimeFallbacks()
    {
        OutsideScene outside = Object.FindAnyObjectByType<OutsideScene>();
        if (outside == null)
        {
            return;
        }

        SerializedObject serialized = new SerializedObject(outside);
        SerializedProperty fallback = serialized.FindProperty("createMissingPrototypeContent");
        if (fallback != null)
        {
            fallback.boolValue = false;
            serialized.ApplyModifiedProperties();
        }
    }

    private static void EnsureDaylight()
    {
        Light2D[] lights = Object.FindObjectsByType<Light2D>(FindObjectsInactive.Exclude);
        foreach (Light2D light in lights)
        {
            if (light.lightType == Light2D.LightType.Global)
            {
                light.name = "Daylight Global Light";
                light.color = Color.white;
                light.intensity = 1f;
                return;
            }
        }

        GameObject obj = new GameObject("Daylight Global Light");
        Light2D daylight = obj.AddComponent<Light2D>();
        daylight.lightType = Light2D.LightType.Global;
        daylight.color = Color.white;
        daylight.intensity = 1f;
    }

    private static void EnsureSpawnPoint()
    {
        if (Object.FindAnyObjectByType<PlayerSpawnPoint>() != null)
        {
            return;
        }

        GameObject spawn = new GameObject("Player Spawn Point");
        spawn.transform.position = new Vector3(-8f, 0.5f, 0f);
        spawn.AddComponent<PlayerSpawnPoint>();
        spawn.AddComponent<EditorPlaceable>();
    }

    private static void EnsureVendorHouse()
    {
        if (GameObject.Find("Casa del Vendedor Completa") != null)
        {
            return;
        }

        GameObject house = new GameObject("Casa del Vendedor Completa");
        house.transform.position = new Vector3(14f, 0.65f, 0f);
        house.AddComponent<EditorPlaceable>();

        SpriteRenderer renderer = house.AddComponent<SpriteRenderer>();
        renderer.sprite = RuntimeSpriteCropper.LoadTrimmedSprite("Home/completeHouse", 256f, 6);
        renderer.sortingOrder = 4;
        SpriteLit.Apply(renderer);

        if (renderer.sprite != null)
        {
            float scale = 5.8f / Mathf.Max(0.01f, renderer.sprite.bounds.size.y);
            house.transform.localScale = new Vector3(scale, scale, 1f);
        }
    }

    private static void EnsureSaveVendor()
    {
        if (GameObject.Find("Vendedor Save Machine") != null)
        {
            return;
        }

        GameObject vendor = new GameObject("Vendedor Save Machine");
        vendor.transform.position = new Vector3(14f, 1.1f, 0f);
        vendor.AddComponent<EditorPlaceable>();
        vendor.AddComponent<SpriteRenderer>();
        vendor.AddComponent<VendorSpriteAnimator>();

        BoxCollider2D collider = vendor.AddComponent<BoxCollider2D>();
        collider.size = new Vector2(1.6f, 2.2f);
        collider.isTrigger = true;
        vendor.AddComponent<SaveVendor>();
    }

    private static void EnsureReturnDoor()
    {
        if (GameObject.Find("Door Back To House") != null)
        {
            return;
        }

        GameObject door = new GameObject("Door Back To House");
        door.transform.position = new Vector3(-10.5f, -1.4f, 0f);
        door.transform.localScale = new Vector3(1.1f, 2f, 1f);
        door.AddComponent<EditorPlaceable>();

        SpriteRenderer renderer = PlaceholderSprites.MakeSquare(door, new Color(0.38f, 0.22f, 0.11f), 4);
        renderer.drawMode = SpriteDrawMode.Sliced;
        renderer.size = Vector2.one;

        BoxCollider2D collider = door.AddComponent<BoxCollider2D>();
        collider.isTrigger = true;
        collider.size = Vector2.one;

        DoorInteractable interactable = door.AddComponent<DoorInteractable>();
        interactable.Configure("home_day_1", "Press E to go home", "Go back inside the house?");
    }

    private static void EnsureOutsideFloor()
    {
        if (GameObject.Find("Floor") != null)
        {
            return;
        }

        TerrainBlock.Spawn(TerrainType.Floor, new Vector2(0f, -3f), new Vector2(60f, 1f), null, "Floor");
    }

    private static void EnsureEnemySpawnMarker(string name, Vector3 position)
    {
        if (GameObject.Find(name) != null)
        {
            return;
        }

        GameObject marker = new GameObject(name);
        marker.transform.position = position;
        marker.AddComponent<EditorPlaceable>();
    }
}
#endif
