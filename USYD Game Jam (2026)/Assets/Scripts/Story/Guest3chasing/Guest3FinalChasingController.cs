using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class Guest3FinalChasingController : MonoBehaviour
{
    private enum FinalChaseState
    {
        RunningToElevator,
        ReachedElevator,
        ShowingOwnerDialogue,
        BlackScreen
    }

    [Header("Scene References")]
    [SerializeField] private Transform guest3;
    [Tooltip("Top target. In Guest3_Final_Chasing this should be the Elevator object.")]
    [SerializeField] private Transform elevatorTarget;

    [Header("Movement")]
    [Tooltip("Guest3 movement speed while running upward to the elevator.")]
    [SerializeField] private float guestMoveSpeed = 2.8f;
    [SerializeField] private float arriveDistance = 0.08f;

    [Header("Result")]
    [Tooltip("Hide Guest3 after reaching the elevator.")]
    [SerializeField] private bool hideGuestOnArrival = true;

    [Header("Ending Dialogue")]
    [Tooltip("Optional existing Canvas. If empty, a simple runtime UI is created.")]
    [SerializeField] private Canvas endingCanvas;
    [Tooltip("Optional dialogue panel GameObject. If empty, a simple runtime panel is created.")]
    [SerializeField] private GameObject dialoguePanel;
    [Tooltip("Optional dialogue text. If empty, a simple runtime text object is created.")]
    [SerializeField] private Text dialogueText;
    [Tooltip("Optional fullscreen black Image. If empty, a simple runtime black screen is created.")]
    [SerializeField] private Image blackScreen;
    [TextArea(2, 4)]
    [SerializeField] private string ownerDialogue = "Hotel Owner:\n“There we are. Safe and sound.”";
    [SerializeField] private float dialogueDelayAfterGuestDisappears = 0.15f;
    [SerializeField] private float dialogueDisplaySeconds = 2.5f;
    [SerializeField] private float blackFadeSeconds = 0.75f;

    private FinalChaseState state = FinalChaseState.RunningToElevator;
    private Coroutine endingRoutine;

    private void Awake()
    {
        if (elevatorTarget == null)
        {
            GameObject elevator = GameObject.Find("Elevator");
            if (elevator != null)
            {
                elevatorTarget = elevator.transform;
            }
        }

        EnsureEndingUi();
    }

    private void Update()
    {
        if (state != FinalChaseState.RunningToElevator || guest3 == null || elevatorTarget == null)
        {
            return;
        }

        Vector3 targetPosition = new Vector3(
            guest3.position.x,
            elevatorTarget.position.y,
            guest3.position.z);

        guest3.position = Vector3.MoveTowards(
            guest3.position,
            targetPosition,
            guestMoveSpeed * Time.deltaTime);

        if (Vector3.Distance(guest3.position, targetPosition) <= arriveDistance)
        {
            state = FinalChaseState.ReachedElevator;
            Debug.Log("Guest3 Final Chasing: Guest3 reached the elevator.", this);

            if (hideGuestOnArrival)
            {
                guest3.gameObject.SetActive(false);
            }

            if (endingRoutine != null)
            {
                StopCoroutine(endingRoutine);
            }

            endingRoutine = StartCoroutine(PlayOwnerEndingRoutine());
        }
    }

    private IEnumerator PlayOwnerEndingRoutine()
    {
        state = FinalChaseState.ShowingOwnerDialogue;

        if (dialogueDelayAfterGuestDisappears > 0f)
        {
            yield return new WaitForSeconds(dialogueDelayAfterGuestDisappears);
        }

        ShowOwnerDialogue();

        if (dialogueDisplaySeconds > 0f)
        {
            yield return new WaitForSeconds(dialogueDisplaySeconds);
        }

        HideOwnerDialogue();
        yield return FadeToBlack();
        state = FinalChaseState.BlackScreen;
    }

    private void ShowOwnerDialogue()
    {
        EnsureEndingUi();

        if (dialogueText != null)
        {
            ApplyDialogueTextSettings(dialogueText);
            dialogueText.text = ownerDialogue;
        }

        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(true);
        }
    }

    private void HideOwnerDialogue()
    {
        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(false);
        }
    }

    private IEnumerator FadeToBlack()
    {
        EnsureEndingUi();

        if (blackScreen == null)
        {
            yield break;
        }

        blackScreen.gameObject.SetActive(true);

        if (blackFadeSeconds <= 0f)
        {
            SetBlackScreenAlpha(1f);
            yield break;
        }

        float elapsed = 0f;
        while (elapsed < blackFadeSeconds)
        {
            elapsed += Time.deltaTime;
            SetBlackScreenAlpha(Mathf.Clamp01(elapsed / blackFadeSeconds));
            yield return null;
        }

        SetBlackScreenAlpha(1f);
    }

    private void SetBlackScreenAlpha(float alpha)
    {
        if (blackScreen == null)
        {
            return;
        }

        Color color = blackScreen.color;
        color.a = alpha;
        blackScreen.color = color;
    }

    private void EnsureEndingUi()
    {
        if (endingCanvas == null)
        {
            GameObject canvasObject = new GameObject("Guest3FinalEndingUI");
            endingCanvas = canvasObject.AddComponent<Canvas>();
            endingCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObject.AddComponent<CanvasScaler>();
            canvasObject.AddComponent<GraphicRaycaster>();
        }

        if (dialoguePanel == null)
        {
            dialoguePanel = CreateDialoguePanel(endingCanvas.transform);
        }

        if (dialogueText == null && dialoguePanel != null)
        {
            dialogueText = dialoguePanel.GetComponentInChildren<Text>(true);
        }

        if (dialogueText != null)
        {
            ApplyDialogueTextSettings(dialogueText);
        }

        if (blackScreen == null)
        {
            blackScreen = CreateBlackScreen(endingCanvas.transform);
        }

        HideOwnerDialogue();

        if (blackScreen != null)
        {
            SetBlackScreenAlpha(0f);
            blackScreen.gameObject.SetActive(false);
        }
    }

    private GameObject CreateDialoguePanel(Transform parent)
    {
        GameObject panelObject = new GameObject("OwnerDialoguePanel");
        panelObject.transform.SetParent(parent, false);

        RectTransform panelRect = panelObject.AddComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.1f, 0.07f);
        panelRect.anchorMax = new Vector2(0.9f, 0.38f);
        panelRect.offsetMin = Vector2.zero;
        panelRect.offsetMax = Vector2.zero;

        Image panelImage = panelObject.AddComponent<Image>();
        panelImage.color = new Color(0f, 0f, 0f, 0.82f);

        GameObject textObject = new GameObject("OwnerDialogueText");
        textObject.transform.SetParent(panelObject.transform, false);

        RectTransform textRect = textObject.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = new Vector2(24f, 14f);
        textRect.offsetMax = new Vector2(-24f, -14f);

        Text text = textObject.AddComponent<Text>();
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        ApplyDialogueTextSettings(text);
        text.text = ownerDialogue;

        panelObject.SetActive(false);
        return panelObject;
    }

    private void ApplyDialogueTextSettings(Text text)
    {
        text.fontSize = 26;
        text.alignment = TextAnchor.MiddleLeft;
        text.color = Color.white;
        text.horizontalOverflow = HorizontalWrapMode.Wrap;
        text.verticalOverflow = VerticalWrapMode.Overflow;
    }

    private Image CreateBlackScreen(Transform parent)
    {
        GameObject blackObject = new GameObject("FinalBlackScreen");
        blackObject.transform.SetParent(parent, false);

        RectTransform blackRect = blackObject.AddComponent<RectTransform>();
        blackRect.anchorMin = Vector2.zero;
        blackRect.anchorMax = Vector2.one;
        blackRect.offsetMin = Vector2.zero;
        blackRect.offsetMax = Vector2.zero;

        Image image = blackObject.AddComponent<Image>();
        image.color = new Color(0f, 0f, 0f, 0f);
        blackObject.SetActive(false);
        return image;
    }
}
