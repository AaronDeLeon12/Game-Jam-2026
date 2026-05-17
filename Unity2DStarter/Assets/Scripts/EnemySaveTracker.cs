using UnityEngine;
using UnityEngine.SceneManagement;

public class EnemySaveTracker : MonoBehaviour
{
    [SerializeField] private string enemyId;

    private bool registeredDefeat;

    public string EnemyId
    {
        get
        {
            if (string.IsNullOrEmpty(enemyId))
            {
                enemyId = BuildEnemyId(gameObject);
            }

            return enemyId;
        }
    }

    private void Awake()
    {
        if (string.IsNullOrEmpty(enemyId))
        {
            enemyId = BuildEnemyId(gameObject);
        }

        if (GameSession.IsEnemyDefeated(enemyId))
        {
            Destroy(gameObject);
        }
    }

    public void RecordDefeat()
    {
        if (registeredDefeat || GameSession.IsApplyingLoad)
        {
            return;
        }

        if (!string.IsNullOrEmpty(enemyId))
        {
            GameSession.RecordDefeatedEnemy(enemyId);
            registeredDefeat = true;
        }
    }

    public static void PruneDefeatedEnemies()
    {
        EnemySaveTracker[] trackers = Object.FindObjectsByType<EnemySaveTracker>(FindObjectsSortMode.None);
        for (int i = 0; i < trackers.Length; i++)
        {
            if (trackers[i] != null && GameSession.IsEnemyDefeated(trackers[i].EnemyId))
            {
                Object.Destroy(trackers[i].gameObject);
            }
        }
    }

    private static string BuildEnemyId(GameObject owner)
    {
        string sceneName = owner.scene.IsValid() ? owner.scene.name : SceneManager.GetActiveScene().name;
        Vector3 p = owner.transform.position;
        return sceneName + ":" + owner.name + ":"
            + Mathf.RoundToInt(p.x * 10f) + ":"
            + Mathf.RoundToInt(p.y * 10f) + ":"
            + Mathf.RoundToInt(p.z * 10f);
    }
}
