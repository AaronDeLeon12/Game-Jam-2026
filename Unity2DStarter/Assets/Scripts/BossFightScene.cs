using UnityEngine;
using UnityEngine.Rendering.Universal;

/// <summary>
/// Level content for "boss_fight_scenario". Ensures systems exist, neutral
/// daylight, a player spawn, and spawns the final boss (the oruga). The floor
/// is the hand-placed scene object, so it is not generated here.
/// </summary>
public class BossFightScene : MonoBehaviour
{
    [SerializeField] private Vector3 playerSpawn = new Vector3(-9f, 0.5f, 0f);

    private void Awake()
    {
        DayManager.EnsureExists();

        // Bright neutral light before EnsureExists() so the systems' dim
        // global light is skipped (avoids two stacking).
        BuildDaylight();

        SystemsBootstrap.EnsureExists();
        PlayBossMusic();

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
        spawn.transform.position = playerSpawn;
        spawn.AddComponent<PlayerSpawnPoint>();

        SetupBoss();
    }

    // Wires the hand-placed "boss_2" object at runtime (no scene-file edit
    // needed). Adds the renderer/physics + FinalBoss (follow + 3000 HP) +
    // FinalBossAnimator (walk cycle, auto-loads its frames).
    private void SetupBoss()
    {
        GameObject boss = GameObject.Find("boss_2");
        if (boss == null)
        {
            boss = new GameObject("boss_2");
            boss.transform.position = new Vector3(8f, 2f, 0f);
        }

        Rigidbody2D body = GetOrAdd<Rigidbody2D>(boss);
        body.gravityScale = 3f;
        body.freezeRotation = true;

        BoxCollider2D col = GetOrAdd<BoxCollider2D>(boss);
        col.isTrigger = false;
        col.size = new Vector2(4f, 2f);

        GetOrAdd<FinalBoss>(boss);
        GetOrAdd<FinalBossAnimator>(boss);
    }

    private static T GetOrAdd<T>(GameObject go) where T : Component
    {
        T c = go.GetComponent<T>();
        return c != null ? c : go.AddComponent<T>();
    }

    private void BuildDaylight()
    {
        SceneLighting.ReplaceGlobalLight("Boss Fight Global Light", Color.white, 1f);
    }

    private static void PlayBossMusic()
    {
        GameAudio.PlayMusic("bossFight1", 0.5f);
    }
}
