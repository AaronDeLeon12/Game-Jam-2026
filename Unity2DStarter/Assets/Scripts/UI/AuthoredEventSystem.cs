using UnityEngine.EventSystems;

// Scene copies of the persistent rig must not register a second input system.
public class AuthoredEventSystem : EventSystem
{
    protected override void OnEnable()
    {
        if (current != null && current != this) { enabled = false; return; }
        base.OnEnable();
    }
}
