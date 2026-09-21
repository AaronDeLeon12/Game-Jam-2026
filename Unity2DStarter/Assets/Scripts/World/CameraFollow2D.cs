using UnityEngine;

public class CameraFollow2D : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private float followSpeed = 4.5f;
    [SerializeField] private Vector3 offset = new Vector3(0f, 2.5f, -10f);

    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
        Snap();
    }

    public void SetOffset(Vector3 newOffset)
    {
        offset = newOffset;
        Snap();
    }

    public Vector3 Offset => offset;

    public void Snap()
    {
        if (target != null)
        {
            transform.position = ClampToBounds(target.position + offset);
        }
    }

    private void LateUpdate()
    {
        if (target == null)
        {
            return;
        }

        Vector3 desiredPosition = ClampToBounds(target.position + offset);
        transform.position = Vector3.Lerp(transform.position, desiredPosition, followSpeed * Time.deltaTime);
    }

    // Keeps the camera inside the scenario limits (defined by the invisible
    // walls via CameraBounds2D) so the floor/scene edges are never visible.
    // When the player nears an edge the clamped target stops moving, so the
    // camera locks on that axis instead of following.
    private Vector3 ClampToBounds(Vector3 desired)
    {
        CameraBounds2D bounds = CameraBounds2D.Active;
        if (bounds == null)
        {
            return desired;
        }

        Camera cam = GetComponent<Camera>();
        if (cam == null)
        {
            cam = Camera.main;
        }

        return bounds.Clamp(desired, cam);
    }
}
