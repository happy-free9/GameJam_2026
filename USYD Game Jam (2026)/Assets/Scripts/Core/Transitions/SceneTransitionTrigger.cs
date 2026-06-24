using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class SceneTransitionTrigger : MonoBehaviour, IInteractable
{
    [Header("Prompt")]
    [Tooltip("Short prompt shown while the player is inside the trigger.")]
    [SerializeField] private string promptText = "Press E";

    [Header("Scene Target")]
    [Tooltip("Name of the destination scene. This scene must be included in Build Settings.")]
    [SerializeField] private string targetSceneName;
    [Tooltip("SpawnPoint id to use after loading the destination scene.")]
    [SerializeField] private string targetSpawnPointId;

    private void Reset()
    {
        Collider2D trigger = GetComponent<Collider2D>();
        if (trigger != null)
        {
            trigger.isTrigger = true;
        }
    }

    private void OnValidate()
    {
        if (Application.isPlaying)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(targetSceneName))
        {
            Debug.LogWarning($"SceneTransitionTrigger on '{name}' has no target scene configured.", this);
        }

        if (string.IsNullOrWhiteSpace(targetSpawnPointId))
        {
            Debug.LogWarning($"SceneTransitionTrigger on '{name}' has no target spawn point id configured.", this);
        }
    }

    public void Configure(string newTargetSceneName, string newTargetSpawnPointId)
    {
        targetSceneName = newTargetSceneName;
        targetSpawnPointId = newTargetSpawnPointId;
    }

    public void SetPromptText(string newPromptText)
    {
        promptText = newPromptText;
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
            interactor.RegisterTarget(this);
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
            interactor.UnregisterTarget(this);
        }
    }

    public bool CanInteract(PlayerInteractor interactor)
    {
        return CanTransition();
    }

    public string GetPrompt(PlayerInteractor interactor)
    {
        return promptText;
    }

    public void Interact(PlayerInteractor interactor)
    {
        TriggerTransition();
    }

    public void TriggerTransition()
    {
        if (!CanTransition())
        {
            Debug.LogWarning($"SceneTransitionTrigger on '{name}' is missing target scene or spawn point id.", this);
            return;
        }

        SceneTransitionManager.Instance?.LoadScene(targetSceneName, targetSpawnPointId);
    }

    private bool CanTransition()
    {
        return isActiveAndEnabled &&
            !string.IsNullOrWhiteSpace(targetSceneName) &&
            !string.IsNullOrWhiteSpace(targetSpawnPointId);
    }

    public Transform GetTransform()
    {
        return transform;
    }
}
