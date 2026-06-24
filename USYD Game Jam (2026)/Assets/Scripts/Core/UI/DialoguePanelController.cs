using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class DialoguePanelController : MonoBehaviour
{
    public static DialoguePanelController Instance { get; private set; }
    public static bool HasOpenDialogue => Instance != null && Instance.isDialogueOpen;

    [Header("Lifecycle")]
    [Tooltip("Keeps this dialogue UI alive across scene loads so one copy can serve the whole demo.")]
    [SerializeField] private bool persistAcrossScenes = true;
    [Header("Optional Existing References")]
    [Tooltip("Optional root for the dialogue panel. If left empty, one is created automatically.")]
    [SerializeField] private RectTransform panelRoot;
    [Tooltip("Optional speaker label. If left empty, one is created automatically.")]
    [SerializeField] private Text speakerText;
    [Tooltip("Optional body label. If left empty, one is created automatically.")]
    [SerializeField] private Text bodyText;
    [Tooltip("Invoked when the final dialogue line is dismissed.")]
    [SerializeField] private UnityEvent onDialogueFinished = new();

    private readonly Queue<DialogueLine> queuedLines = new();
    private bool isDialogueOpen;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        if (persistAcrossScenes)
        {
            DontDestroyOnLoad(gameObject);
        }

        EnsureUi();
        HidePanel();
    }

    private void Update()
    {
        if (!isDialogueOpen)
        {
            return;
        }

        bool pressedAdvance =
            (Keyboard.current != null &&
            (Keyboard.current.eKey.wasPressedThisFrame ||
            Keyboard.current.spaceKey.wasPressedThisFrame ||
            Keyboard.current.enterKey.wasPressedThisFrame ||
            Keyboard.current.numpadEnterKey.wasPressedThisFrame)) ||
            (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame);

        if (pressedAdvance)
        {
            AdvanceDialogue();
        }
    }

    public void StartDialogue(List<DialogueLine> lines)
    {
        if (lines == null || lines.Count == 0)
        {
            HidePanel();
            return;
        }

        queuedLines.Clear();
        for (int i = 0; i < lines.Count; i++)
        {
            queuedLines.Enqueue(lines[i]);
        }

        isDialogueOpen = true;
        panelRoot.gameObject.SetActive(true);
        ShowNextLine();
    }

    public void ShowSingleLine(string speakerNameValue, string bodyTextValue)
    {
        StartDialogue(new List<DialogueLine>
        {
            new() { speakerName = speakerNameValue, bodyText = bodyTextValue }
        });
    }

    public void AdvanceDialogue()
    {
        if (!isDialogueOpen)
        {
            return;
        }

        ShowNextLine();
    }

    public void HidePanel()
    {
        isDialogueOpen = false;
        queuedLines.Clear();

        if (panelRoot != null)
        {
            panelRoot.gameObject.SetActive(false);
        }
    }

    private void ShowNextLine()
    {
        if (queuedLines.Count == 0)
        {
            HidePanel();
            onDialogueFinished?.Invoke();
            return;
        }

        DialogueLine line = queuedLines.Dequeue();
        speakerText.text = line.speakerName;
        bodyText.text = line.bodyText;
    }

    private void EnsureUi()
    {
        Canvas canvas = GetComponentInChildren<Canvas>(true);
        if (canvas == null)
        {
            GameObject canvasObject = new("DialogueCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvasObject.transform.SetParent(transform, false);

            canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
        }

        if (panelRoot == null)
        {
            GameObject panelObject = new("DialogueRoot", typeof(RectTransform), typeof(Image));
            panelObject.transform.SetParent(canvas.transform, false);

            panelRoot = panelObject.GetComponent<RectTransform>();
            panelRoot.anchorMin = new Vector2(0.1f, 0.04f);
            panelRoot.anchorMax = new Vector2(0.9f, 0.24f);
            panelRoot.offsetMin = Vector2.zero;
            panelRoot.offsetMax = Vector2.zero;

            Image background = panelObject.GetComponent<Image>();
            background.color = new Color(0f, 0f, 0f, 0.82f);
        }

        if (speakerText == null)
        {
            speakerText = CreateText("SpeakerText", panelRoot, new Vector2(20f, -18f), new Vector2(-20f, -54f), 26, TextAnchor.UpperLeft);
            speakerText.color = new Color(1f, 0.93f, 0.68f, 1f);
        }

        if (bodyText == null)
        {
            bodyText = CreateText("BodyText", panelRoot, new Vector2(20f, -62f), new Vector2(-20f, -18f), 22, TextAnchor.UpperLeft);
            bodyText.horizontalOverflow = HorizontalWrapMode.Wrap;
            bodyText.verticalOverflow = VerticalWrapMode.Overflow;
        }
    }

    private Text CreateText(
        string objectName,
        RectTransform parent,
        Vector2 offsetMin,
        Vector2 offsetMax,
        int fontSize,
        TextAnchor anchor)
    {
        GameObject textObject = new(objectName, typeof(RectTransform), typeof(Text));
        textObject.transform.SetParent(parent, false);

        RectTransform rectTransform = textObject.GetComponent<RectTransform>();
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.one;
        rectTransform.offsetMin = new Vector2(offsetMin.x, offsetMax.y);
        rectTransform.offsetMax = new Vector2(offsetMax.x, offsetMin.y);

        Text text = textObject.GetComponent<Text>();
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.fontSize = fontSize;
        text.alignment = anchor;
        text.color = Color.white;
        return text;
    }
}
