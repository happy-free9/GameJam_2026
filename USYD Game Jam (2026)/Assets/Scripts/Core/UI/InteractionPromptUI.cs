using UnityEngine;
using UnityEngine.UI;

public class InteractionPromptUI : MonoBehaviour
{
    public static InteractionPromptUI Instance { get; private set; }

    [Header("Lifecycle")]
    [Tooltip("Keeps this prompt UI alive across scene loads so one copy can serve the whole demo.")]
    [SerializeField] private bool persistAcrossScenes = true;
    [Header("Optional Existing References")]
    [Tooltip("Optional root for the prompt panel. If left empty, one is created automatically.")]
    [SerializeField] private RectTransform promptRoot;
    [Tooltip("Optional text component. If left empty, one is created automatically.")]
    [SerializeField] private Text promptText;

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
        HidePrompt();
    }

    public void ShowPrompt(string textToShow)
    {
        EnsureUi();
        promptRoot.gameObject.SetActive(true);
        promptText.text = textToShow;
    }

    public void HidePrompt()
    {
        if (promptRoot != null)
        {
            promptRoot.gameObject.SetActive(false);
        }
    }

    private void EnsureUi()
    {
        Canvas canvas = GetComponentInChildren<Canvas>(true);
        if (canvas == null)
        {
            GameObject canvasObject = new("InteractionPromptCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvasObject.transform.SetParent(transform, false);

            canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
        }

        if (promptRoot == null)
        {
            GameObject panelObject = new("PromptRoot", typeof(RectTransform), typeof(Image));
            panelObject.transform.SetParent(canvas.transform, false);

            promptRoot = panelObject.GetComponent<RectTransform>();
            promptRoot.anchorMin = new Vector2(0.5f, 0.12f);
            promptRoot.anchorMax = new Vector2(0.5f, 0.12f);
            promptRoot.pivot = new Vector2(0.5f, 0.5f);
            promptRoot.sizeDelta = new Vector2(320f, 52f);
            promptRoot.anchoredPosition = Vector2.zero;

            Image background = panelObject.GetComponent<Image>();
            background.color = new Color(0f, 0f, 0f, 0.7f);
        }

        if (promptText == null)
        {
            GameObject textObject = new("PromptText", typeof(RectTransform), typeof(Text));
            textObject.transform.SetParent(promptRoot, false);

            RectTransform textRect = textObject.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = new Vector2(16f, 8f);
            textRect.offsetMax = new Vector2(-16f, -8f);

            promptText = textObject.GetComponent<Text>();
            promptText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            promptText.alignment = TextAnchor.MiddleCenter;
            promptText.color = Color.white;
            promptText.text = "Press E";
            promptText.resizeTextForBestFit = true;
            promptText.resizeTextMinSize = 18;
            promptText.resizeTextMaxSize = 28;
        }
    }
}
