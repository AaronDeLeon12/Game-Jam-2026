using System.Collections.Generic;
using UnityEngine;

public class PlayerInteract : MonoBehaviour
{
    [SerializeField] private float interactRadius = 1.85f;
    [SerializeField] private KeyCode interactKey = KeyCode.E;

    private IInteractable currentTarget;
    private IInteractable[] interactables;

    private void Start()
    {
        RefreshInteractables();
    }

    public void RefreshInteractables()
    {
        var allMono = FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None);
        var list = new List<IInteractable>();
        foreach (MonoBehaviour mono in allMono)
            if (mono is IInteractable i)
                list.Add(i);
        interactables = list.ToArray();
    }

    private void Update()
    {
        if (GameModal.IsOpen)
        {
            return;
        }

        currentTarget = FindClosestInteractable();

        if (currentTarget != null && Input.GetKeyDown(interactKey))
        {
            currentTarget.Interact();
        }
    }

    private IInteractable FindClosestInteractable()
    {
        if (interactables == null) return null;

        IInteractable closest = null;
        float closestDistance = float.MaxValue;

        foreach (IInteractable interactable in interactables)
        {
            MonoBehaviour mono = interactable as MonoBehaviour;
            if (mono == null || mono.gameObject == gameObject) continue;

            float distance = Vector2.Distance(transform.position, mono.transform.position);
            if (distance <= interactRadius && distance < closestDistance)
            {
                closestDistance = distance;
                closest = interactable;
            }
        }

        return closest;
    }

    public string GetCurrentPrompt() => currentTarget?.GetPromptText();

    public bool HasTarget() => currentTarget != null;

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactRadius);
    }
}
