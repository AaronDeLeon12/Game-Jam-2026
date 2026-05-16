using UnityEngine;

/// <summary>
/// Trigger volume that loads another level when the player enters it.
/// Leave targetSceneName empty to advance to the next level in Build Settings.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class LevelExit : MonoBehaviour
{
    [SerializeField] private string targetSceneName = "";

    private void Awake()
    {
        GetComponent<Collider2D>().isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (LevelManager.Instance == null)
        {
            return;
        }

        if (SystemsBootstrap.Instance != null && other.gameObject != SystemsBootstrap.Instance.Player)
        {
            return;
        }

        if (string.IsNullOrEmpty(targetSceneName))
        {
            LevelManager.Instance.LoadNextLevel();
        }
        else
        {
            LevelManager.Instance.LoadLevel(targetSceneName);
        }
    }
}
