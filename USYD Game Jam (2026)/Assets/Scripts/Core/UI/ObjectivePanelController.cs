using UnityEngine;
using UnityEngine.UI;

public class ObjectivePanelController : MonoBehaviour
{
    public static ObjectivePanelController Instance { get; private set; }

    [Header("Lifecycle")]
    [Tooltip("Keeps this objective UI alive across scene loads so one copy can serve the whole demo.")]
    [SerializeField] private bool persistAcrossScenes = true;
    [Header("Optional Existing References")]
    [Tooltip("Optional root for the objective panel. If left empty, one is created automatically.")]
    [SerializeField] private RectTransform panelRoot;
    [Tooltip("Optional text component. If left empty, one is created automatically.")]
    [SerializeField] private Text objectiveText;

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
        ClearObjective();
    }

    public void SetObjective(string textToShow)
    {
        EnsureUi();
        panelRoot.gameObject.SetActive(true);
        objectiveText.text = textToShow;
    }

    public void ClearObjective()
    {
        if (panelRoot != null)
        {
            panelRoot.gameObject.SetActive(false);
        }

        if (objectiveText != null)
        {
            objectiveText.text = string.Empty;
        }
    }

    private void EnsureUi()
    {
        Canvas canvas = GetComponentInChildren<Canvas>(true);
        if (canvas == null)
        {
            GameObject canvasObject = new("ObjectiveCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvasObject.transform.SetParent(transform, false);

            canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
        }

        if (panelRoot == null)
        {
            GameObject panelObject = new("ObjectiveRoot", typeof(RectTransform), typeof(Image));
            panelObject.transform.SetParent(canvas.transform, false);

            panelRoot = panelObject.GetComponent<RectTransform>();
            panelRoot.anchorMin = new Vector2(0.02f, 0.98f);
            panelRoot.anchorMax = new Vector2(0.38f, 0.98f);
            panelRoot.pivot = new Vector2(0f, 1f);
            panelRoot.sizeDelta = new Vector2(0f, 80f);
            panelRoot.anchoredPosition = Vector2.zero;

            Image background = panelObject.GetComponent<Image>();
            background.color = new Color(0f, 0f, 0f, 0.72f);
        }

        if (objectiveText == null)
        {
            GameObject textObject = new("ObjectiveText", typeof(RectTransform), typeof(Text));
            textObject.transform.SetParent(panelRoot, false);

            RectTransform textRect = textObject.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = new Vector2(16f, 8f);
            textRect.offsetMax = new Vector2(-16f, -8f);

            objectiveText = textObject.GetComponent<Text>();
            objectiveText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            objectiveText.alignment = TextAnchor.MiddleLeft;
            objectiveText.color = Color.white;
            objectiveText.resizeTextForBestFit = true;
            objectiveText.resizeTextMinSize = 18;
            objectiveText.resizeTextMaxSize = 28;
            objectiveText.horizontalOverflow = HorizontalWrapMode.Wrap;
            objectiveText.verticalOverflow = VerticalWrapMode.Truncate;
        }
    }
}
