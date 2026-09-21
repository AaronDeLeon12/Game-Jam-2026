using UnityEngine;

/// <summary>
/// Authoring-only marker for objects that are meant to be placed and moved by
/// hand in Unity. It does not run gameplay logic; it just stores editor notes.
/// </summary>
[DisallowMultipleComponent]
[AddComponentMenu("Tales of Ivory Moss/Authoring Marker")]
public class EditorPlaceable : MonoBehaviour
{
    [SerializeField] private string contentId;
    [TextArea(2, 5)]
    [SerializeField] private string notes = "Editor-authored object. Safe to move in Edit Mode.";

    public string ContentId => contentId;
    public string Notes => notes;
}
