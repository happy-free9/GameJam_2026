using UnityEngine;

public class CoreUIRoot : MonoBehaviour
{
    [Header("Lifecycle")]
    [Tooltip("Keeps the whole UI root alive across scene loads.")]
    [SerializeField] private bool persistAcrossScenes = true;

    [Header("Create Missing Systems")]
    [Tooltip("Creates the shared prompt UI if one is not already present.")]
    [SerializeField] private bool ensureInteractionPrompt = true;
    [Tooltip("Creates the shared objective UI if one is not already present.")]
    [SerializeField] private bool ensureObjectivePanel = true;
    [Tooltip("Creates the shared dialogue UI if one is not already present.")]
    [SerializeField] private bool ensureDialoguePanel = true;

    private void Awake()
    {
        if (persistAcrossScenes)
        {
            DontDestroyOnLoad(gameObject);
        }

        if (ensureInteractionPrompt)
        {
            EnsureChildUi<InteractionPromptUI>("InteractionPromptUI");
        }

        if (ensureObjectivePanel)
        {
            EnsureChildUi<ObjectivePanelController>("ObjectivePanelController");
        }

        if (ensureDialoguePanel)
        {
            EnsureChildUi<DialoguePanelController>("DialoguePanelController");
        }
    }

    private void EnsureChildUi<T>(string objectName) where T : Component
    {
        if (FindFirstObjectByType<T>() != null)
        {
            return;
        }

        GameObject child = new(objectName);
        child.AddComponent<T>();
    }
}
