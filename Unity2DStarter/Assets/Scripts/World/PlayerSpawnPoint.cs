using UnityEngine;

/// <summary>
/// Marker placed in a level scene. LevelManager moves the persistent player
/// to this transform's position when the level becomes active.
/// </summary>
public class PlayerSpawnPoint : MonoBehaviour
{
    private void OnDrawGizmos()
    {
        Gizmos.color = new Color(0.3f, 0.9f, 1f, 0.8f);
        Gizmos.DrawWireSphere(transform.position, 0.5f);
        Gizmos.DrawLine(transform.position, transform.position + Vector3.up);
    }
}
