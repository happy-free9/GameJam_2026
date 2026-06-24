using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Collider2D))]
public class InteractionTarget : MonoBehaviour, IInteractable
{
    private enum ActivationMode
    {
        InteractButton = 0,
        TriggerEnter = 1
    }

    [Header("Activation")]
    [Tooltip("InteractButton uses PlayerInteractor + prompt + E. TriggerEnter fires immediately when the player enters.")]
    [SerializeField] private ActivationMode activationMode = ActivationMode.InteractButton;

    [Header("Prompt")]
    [Tooltip("Text shown by the shared interaction prompt while the player is inside this trigger.")]
    [SerializeField] private string promptText = "Interact";

    [Header("Behavior")]
    [Tooltip("If enabled, this target only fires once and then becomes unavailable.")]
    [SerializeField] private bool disableAfterInteract;

    [Header("Events")]
    // Designers can wire NPC, item, door, menu, or ending behavior here without editing code.
    [Tooltip("Raised when the player presses E while this target is the active interaction.")]
    [SerializeField] private UnityEvent onInteract = new();

    private bool hasInteracted;

    public UnityEvent OnInteract => onInteract;

    private void Reset()
    {
        Collider2D trigger = GetComponent<Collider2D>();
        trigger.isTrigger = true;
    }

    private void OnValidate()
    {
        Collider2D trigger = GetComponent<Collider2D>();
        if (trigger != null && !trigger.isTrigger)
        {
            Debug.LogWarning(
                $"InteractionTarget on '{name}' works best with a trigger collider. Enable Is Trigger on the attached Collider2D.",
                this);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        PlayerInteractor interactor = other.GetComponent<PlayerInteractor>();
        if (interactor == null)
        {
            interactor = other.GetComponentInParent<PlayerInteractor>();
        }

        if (interactor != null)
        {
            if (activationMode == ActivationMode.TriggerEnter)
            {
                Interact(interactor);
            }
            else
            {
                interactor.RegisterTarget(this);
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        PlayerInteractor interactor = other.GetComponent<PlayerInteractor>();
        if (interactor == null)
        {
            interactor = other.GetComponentInParent<PlayerInteractor>();
        }

        if (interactor != null)
        {
            if (activationMode == ActivationMode.InteractButton)
            {
                interactor.UnregisterTarget(this);
            }
        }
    }

    public bool CanInteract(PlayerInteractor interactor)
    {
        return isActiveAndEnabled && (!disableAfterInteract || !hasInteracted);
    }

    public string GetPrompt(PlayerInteractor interactor)
    {
        return activationMode == ActivationMode.InteractButton ? promptText : string.Empty;
    }

    public void Interact(PlayerInteractor interactor)
    {
        if (!CanInteract(interactor))
        {
            return;
        }

        hasInteracted = true;
        onInteract?.Invoke();
    }

    public Transform GetTransform()
    {
        return transform;
    }

    public void SetPromptText(string newPromptText)
    {
        promptText = newPromptText;
    }
}
