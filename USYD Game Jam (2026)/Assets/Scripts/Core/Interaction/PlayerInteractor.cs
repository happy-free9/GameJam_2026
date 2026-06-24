using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Collider2D))]
public class PlayerInteractor : MonoBehaviour
{
    [Header("Detection")]
    [Tooltip("Maximum distance from the player to the chosen interaction target while inside valid triggers.")]
    [SerializeField] private float interactionRange = 3f;

    private readonly List<IInteractable> nearbyTargets = new();
    private IInteractable currentTarget;
    private bool hasWarnedAboutMissingPromptUi;

    private void Update()
    {
        if (DialoguePanelController.HasOpenDialogue)
        {
            currentTarget = null;
            InteractionPromptUI.Instance?.HidePrompt();
            return;
        }

        currentTarget = FindBestTarget();
        UpdatePrompt();

        if (currentTarget != null && Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
        {
            currentTarget.Interact(this);
            currentTarget = FindBestTarget();
            UpdatePrompt();
        }
    }

    private void OnDisable()
    {
        nearbyTargets.Clear();
        currentTarget = null;
        InteractionPromptUI.Instance?.HidePrompt();
    }

    public void RegisterTarget(IInteractable target)
    {
        if (target == null || nearbyTargets.Contains(target))
        {
            return;
        }

        nearbyTargets.Add(target);
    }

    public void UnregisterTarget(IInteractable target)
    {
        if (target == null)
        {
            return;
        }

        nearbyTargets.Remove(target);

        if (currentTarget == target)
        {
            currentTarget = null;
            UpdatePrompt();
        }
    }

    private IInteractable FindBestTarget()
    {
        // The nearest valid trigger wins so overlapping prompts stay predictable.
        nearbyTargets.RemoveAll(target => target == null);

        IInteractable bestTarget = null;
        float bestDistance = float.MaxValue;

        for (int i = 0; i < nearbyTargets.Count; i++)
        {
            IInteractable target = nearbyTargets[i];
            if (!target.CanInteract(this))
            {
                continue;
            }

            float distance = Vector2.Distance(transform.position, target.GetTransform().position);
            if (distance > interactionRange || distance >= bestDistance)
            {
                continue;
            }

            bestDistance = distance;
            bestTarget = target;
        }

        return bestTarget;
    }

    private void UpdatePrompt()
    {
        if (currentTarget == null)
        {
            InteractionPromptUI.Instance?.HidePrompt();
            return;
        }

        if (InteractionPromptUI.Instance == null)
        {
            if (!hasWarnedAboutMissingPromptUi)
            {
                Debug.LogWarning(
                    "PlayerInteractor could not find an InteractionPromptUI in the scene. Add CoreUIRoot or InteractionPromptUI before testing prompts.",
                    this);
                hasWarnedAboutMissingPromptUi = true;
            }

            return;
        }

        hasWarnedAboutMissingPromptUi = false;
        InteractionPromptUI.Instance.ShowPrompt(currentTarget.GetPrompt(this));
    }
}
