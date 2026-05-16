using UnityEngine;

[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(DialogueBox))]
public class DialogueTrigger : MonoBehaviour, IInteractable
{
    [SerializeField] private string promptText = "Press E to talk";

    private DialogueBox dialogueBox;

    private void Awake()
    {
        dialogueBox = GetComponent<DialogueBox>();
        GetComponent<Collider2D>().isTrigger = true;
    }

    public void Interact()
    {
        dialogueBox.Open();
    }

    public string GetPromptText() => promptText;
}
