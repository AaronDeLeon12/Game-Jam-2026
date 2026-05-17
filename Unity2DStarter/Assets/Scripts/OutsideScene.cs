using UnityEngine;
using UnityEngine.Rendering.Universal;

/// <summary>
/// Level content for "outside_1". Ensures day state and game systems exist,
/// places a spawn point and a basic ground so the area is playable.
/// (HomeMode is automatically cleared when leaving the home scene, so the
/// HUD and full player abilities return here.)
/// </summary>
public class OutsideScene : MonoBehaviour
{
    private void Awake()
    {
        DayManager.EnsureExists();

        // Bright neutral daytime light. Created before EnsureExists() so the
        // systems' dim default global light is skipped (it only adds one when
        // no global light already exists), avoiding two lights stacking.
        BuildDaylight();

        SystemsBootstrap.EnsureExists();

        // The camera is persistent; reset the zoom in case we came from the
        // home scene (which zooms in).
        Camera cam = SystemsBootstrap.Instance != null && SystemsBootstrap.Instance.GameCamera != null
            ? SystemsBootstrap.Instance.GameCamera
            : Camera.main;
        if (cam != null)
        {
            cam.orthographicSize = 5f;
            CameraFollow2D follow = cam.GetComponent<CameraFollow2D>();
            if (follow != null)
            {
                follow.SetOffset(new Vector3(0f, 2.5f, -10f));
            }
        }

        SceneBackground.Show("Backgrounds/fondo_verde");

        GameObject spawn = new GameObject("Player Spawn Point");
        spawn.transform.position = new Vector3(-8f, 0.5f, 0f);
        spawn.AddComponent<PlayerSpawnPoint>();

        BuildSaveVendorArea();
        BuildReturnDoor();

        // Floor is now a hand-placed scene object ("Floor" with TerrainBlock)
        // in outside_1, so it is not generated here anymore.
    }

    private void BuildSaveVendorArea()
    {
        Vector3 housePosition = new Vector3(14f, 0.65f, 0f);

        GameObject vendorHouse = new GameObject("Casa del Vendedor Completa");
        vendorHouse.transform.SetParent(transform, false);
        vendorHouse.transform.position = housePosition;

        SpriteRenderer houseRenderer = vendorHouse.AddComponent<SpriteRenderer>();
        houseRenderer.sprite = RuntimeSpriteCropper.LoadTrimmedSprite("Home/completeHouse", 256f, 6);
        houseRenderer.sortingOrder = 4;
        SpriteLit.Apply(houseRenderer);
        if (houseRenderer.sprite != null)
        {
            float scale = 5.8f / Mathf.Max(0.01f, houseRenderer.sprite.bounds.size.y);
            vendorHouse.transform.localScale = new Vector3(scale, scale, 1f);
        }

        GameObject vendor = new GameObject("Vendedor Save Machine");
        vendor.transform.SetParent(transform, false);
        vendor.transform.position = new Vector3(14f, 1.1f, 0f);
        vendor.AddComponent<SpriteRenderer>();
        vendor.AddComponent<VendorSpriteAnimator>();

        BoxCollider2D collider = vendor.AddComponent<BoxCollider2D>();
        collider.size = new Vector2(1.6f, 2.2f);
        collider.isTrigger = true;
        vendor.AddComponent<SaveVendor>();

    }

    private void BuildReturnDoor()
    {
        GameObject door = new GameObject("Door Back To House");
        door.transform.SetParent(transform, false);
        door.transform.position = new Vector3(-10.5f, -1.4f, 0f);
        door.transform.localScale = new Vector3(1.1f, 2f, 1f);

        SpriteRenderer renderer = PlaceholderSprites.MakeSquare(door, new Color(0.38f, 0.22f, 0.11f), 4);
        renderer.drawMode = SpriteDrawMode.Sliced;
        renderer.size = Vector2.one;

        BoxCollider2D collider = door.AddComponent<BoxCollider2D>();
        collider.isTrigger = true;
        collider.size = Vector2.one;

        DoorInteractable interactable = door.AddComponent<DoorInteractable>();
        interactable.Configure("home_day_1", "Press E to go home", "Go back inside the house?");
    }

    private void BuildDaylight()
    {
        SceneLighting.ReplaceGlobalLight("Daylight Global Light", Color.white, 1f);
    }
}
