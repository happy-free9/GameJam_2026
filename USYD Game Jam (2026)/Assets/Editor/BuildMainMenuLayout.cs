using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class BuildMainMenuLayout
{
    private const string MenuPath = "Tools/Hotel Hunger/Build Main Menu";
    private const string ScenePath = "Assets/Scenes/MainMenu_XW.unity";
    private const string MainMenuSceneName = "MainMenu_XW";
    private const string TitleScreenSpritePath = "Assets/Pixel Art/New_Title_Screen(The_Hotel).png";
    private const string CreditsBody =
        "USYD Game Jam 2026\n\n" +
        "Team Credits:\n" +
        "Linh Tran\n" +
        "Sam Kero\n" +
        "Zia Zheng\n" +
        "Xiangwen Guo";

    private static readonly Color BackgroundColor = new(0.5f, 0.78f, 0.76f, 1f);
    private static readonly Color PanelColor = new(0.08f, 0.045f, 0.025f, 0.82f);
    private static readonly Color MainMenuPanelColor = new(0.48f, 0.22f, 0.07f, 0f);
    private static readonly Color TransparentButtonColor = new(0.52f, 0.24f, 0.08f, 0f);
    private static readonly Color ButtonColor = new(0.52f, 0.24f, 0.08f, 0.22f);
    private static readonly Color ButtonHighlightColor = new(0.78f, 0.4f, 0.12f, 0.34f);
    private static readonly Color TextColor = new(1f, 0.9f, 0.58f, 1f);
    private static readonly Color MutedTextColor = new(0.82f, 0.7f, 0.48f, 1f);
    private static readonly Color ButtonOutlineColor = new(0.38f, 0.16f, 0.04f, 0.92f);

    [MenuItem(MenuPath)]
    public static void Build()
    {
        BuildInternal(promptToSaveOpenScenes: true, restoreOriginalScene: true, showDialog: true);
    }

    public static void BuildFromBatchmode()
    {
        BuildInternal(promptToSaveOpenScenes: false, restoreOriginalScene: false, showDialog: false);
    }

    private static void BuildInternal(bool promptToSaveOpenScenes, bool restoreOriginalScene, bool showDialog)
    {
        if (promptToSaveOpenScenes && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
        {
            Debug.LogWarning("Build Main Menu cancelled before opening or creating MainMenu_XW.");
            return;
        }

        string originalScenePath = SceneManager.GetActiveScene().path;
        ConfigureTitleScreenImporter();

        Scene menuScene = OpenOrCreateMainMenuScene();
        BuildSceneContents(menuScene);
        EditorSceneManager.MarkSceneDirty(menuScene);
        EditorSceneManager.SaveScene(menuScene, ScenePath);
        AssetDatabase.Refresh();
        InsertMainMenuFirstInBuildSettings();

        if (restoreOriginalScene &&
            !string.IsNullOrWhiteSpace(originalScenePath) &&
            originalScenePath != ScenePath &&
            AssetDatabase.LoadAssetAtPath<SceneAsset>(originalScenePath) != null)
        {
            EditorSceneManager.OpenScene(originalScenePath, OpenSceneMode.Single);
        }

        if (showDialog)
        {
            EditorUtility.DisplayDialog(
                "Build Main Menu",
                "MainMenu_XW was created or updated, saved, and inserted first in Build Settings.",
                "OK");
        }
    }

    private static Scene OpenOrCreateMainMenuScene()
    {
        SceneAsset existingScene = AssetDatabase.LoadAssetAtPath<SceneAsset>(ScenePath);
        if (existingScene != null)
        {
            return EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        }

        return EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
    }

    private static void ConfigureTitleScreenImporter()
    {
        TextureImporter importer = AssetImporter.GetAtPath(TitleScreenSpritePath) as TextureImporter;
        if (importer == null)
        {
            Debug.LogWarning($"Build Main Menu could not find title-screen art at {TitleScreenSpritePath}.");
            return;
        }

        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Single;
        importer.filterMode = FilterMode.Point;
        importer.mipmapEnabled = false;
        importer.textureCompression = TextureImporterCompression.Uncompressed;
        importer.alphaIsTransparency = true;
        importer.SaveAndReimport();
    }

    private static void BuildSceneContents(Scene scene)
    {
        ClearScene(scene);

        CreateCamera();
        CreateEventSystem();

        GameObject root = new("MainMenuRoot");
        SceneManager.MoveGameObjectToScene(root, scene);
        MainMenuController controller = root.AddComponent<MainMenuController>();

        GameObject canvasObject = new("MainMenuCanvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        canvasObject.transform.SetParent(root.transform, false);

        Canvas canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;

        CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);

        RectTransform canvasRect = canvasObject.GetComponent<RectTransform>();
        Stretch(canvasRect);

        GameObject background = CreatePanel(canvasRect, "SamTitleScreenBackground", Color.white);
        Stretch(background.GetComponent<RectTransform>());
        Image backgroundImage = background.GetComponent<Image>();
        backgroundImage.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(TitleScreenSpritePath);
        backgroundImage.type = Image.Type.Simple;
        backgroundImage.preserveAspect = false;
        backgroundImage.raycastTarget = false;

        Text titleText = null;

        GameObject mainPanel = CreateMainMenuPanel(canvasRect);

        ButtonParts startGame = CreateMainMenuButton(mainPanel.transform, "StartGameButton", "Start Game");
        ButtonParts settings = CreateMainMenuButton(mainPanel.transform, "SettingsButton", "Settings");
        ButtonParts credits = CreateMainMenuButton(mainPanel.transform, "CreditsButton", "Credits");
        ButtonParts quit = CreateMainMenuButton(mainPanel.transform, "QuitButton", "Quit Game");
        StyleMainMenuButton(startGame);
        StyleMainMenuButton(settings);
        StyleMainMenuButton(credits);
        StyleMainMenuButton(quit);

        GameObject settingsPanel = CreatePanel(canvasRect, "SettingsPanel", PanelColor);
        SetAnchored(settingsPanel.GetComponent<RectTransform>(), new Vector2(0.5f, 0.5f), new Vector2(980f, 820f), Vector2.zero);

        Text settingsTitle = CreateText(settingsPanel.transform, "SettingsTitle", "Settings", 44, TextAnchor.MiddleCenter, TextColor);
        SetAnchored(settingsTitle.rectTransform, new Vector2(0.5f, 0.92f), new Vector2(600f, 70f), Vector2.zero);

        GameObject settingsRows = CreateSettingsRowsContainer(settingsPanel.transform);
        Text masterVolumeLabel = CreateSettingsRowLabel(settingsRows.transform, "MasterVolumeLabel", "Master Volume");
        Slider volumeSlider = CreateSlider(masterVolumeLabel.transform.parent, "MasterVolumeSlider");

        Text languageLabel = CreateSettingsRowLabel(settingsRows.transform, "LanguageLabel", "Language");
        Dropdown languageDropdown = CreateDropdown(languageLabel.transform.parent, "LanguageDropdown");

        Text controlsTitle = CreateText(settingsPanel.transform, "ControlsTitle", "Controls", 30, TextAnchor.MiddleCenter, TextColor);
        SetAnchored(controlsTitle.rectTransform, new Vector2(0.5f, 0.58f), new Vector2(420f, 54f), Vector2.zero);

        ControlRow upRow = CreateControlRow(settingsPanel.transform, "UpControl", "Up", new Vector2(0f, 40f), "W");
        ControlRow downRow = CreateControlRow(settingsPanel.transform, "DownControl", "Down", new Vector2(0f, -25f), "S");
        ControlRow leftRow = CreateControlRow(settingsPanel.transform, "LeftControl", "Left", new Vector2(0f, -90f), "A");
        ControlRow rightRow = CreateControlRow(settingsPanel.transform, "RightControl", "Right", new Vector2(0f, -155f), "D");

        Text interactFixedText = CreateText(settingsPanel.transform, "InteractFixedText", "Interact: E (fixed)", 24, TextAnchor.MiddleCenter, MutedTextColor);
        SetAnchored(interactFixedText.rectTransform, new Vector2(0.5f, 0.23f), new Vector2(500f, 44f), Vector2.zero);

        Text captureStatusText = CreateText(settingsPanel.transform, "CaptureStatusText", string.Empty, 22, TextAnchor.MiddleCenter, MutedTextColor);
        SetAnchored(captureStatusText.rectTransform, new Vector2(0.5f, 0.18f), new Vector2(740f, 40f), Vector2.zero);

        ButtonParts resetControls = CreateButton(settingsPanel.transform, "ResetControlsButton", "Reset Controls", new Vector2(-190f, -320f), new Vector2(280f, 58f));
        ButtonParts resetProgress = CreateButton(settingsPanel.transform, "ResetProgressButton", "Reset Progress", new Vector2(190f, -320f), new Vector2(280f, 58f));
        ButtonParts settingsBack = CreateButton(settingsPanel.transform, "SettingsBackButton", "Back", new Vector2(0f, -380f), new Vector2(220f, 58f));

        GameObject creditsPanel = CreatePanel(canvasRect, "CreditsPanel", PanelColor);
        SetAnchored(creditsPanel.GetComponent<RectTransform>(), new Vector2(0.5f, 0.5f), new Vector2(760f, 560f), Vector2.zero);

        Text creditsTitle = CreateText(creditsPanel.transform, "CreditsTitle", "Credits", 44, TextAnchor.MiddleCenter, TextColor);
        SetAnchored(creditsTitle.rectTransform, new Vector2(0.5f, 0.82f), new Vector2(500f, 70f), Vector2.zero);

        Text creditsBody = CreateText(creditsPanel.transform, "CreditsBody", CreditsBody, 30, TextAnchor.MiddleCenter, TextColor);
        SetAnchored(creditsBody.rectTransform, new Vector2(0.5f, 0.52f), new Vector2(620f, 310f), Vector2.zero);

        ButtonParts creditsBack = CreateButton(creditsPanel.transform, "CreditsBackButton", "Back", new Vector2(0f, -210f), new Vector2(220f, 58f));

        GameObject resetProgressConfirmationPanel = CreatePanel(canvasRect, "ResetProgressConfirmationPanel", new Color(0f, 0f, 0f, 0.86f));
        SetAnchored(resetProgressConfirmationPanel.GetComponent<RectTransform>(), new Vector2(0.5f, 0.5f), new Vector2(620f, 300f), Vector2.zero);

        Text resetProgressConfirmationText = CreateText(resetProgressConfirmationPanel.transform, "ResetProgressConfirmationText", "Reset progress?", 28, TextAnchor.MiddleCenter, TextColor);
        SetAnchored(resetProgressConfirmationText.rectTransform, new Vector2(0.5f, 0.66f), new Vector2(520f, 80f), Vector2.zero);
        ButtonParts confirmResetProgress = CreateButton(resetProgressConfirmationPanel.transform, "ConfirmResetProgressButton", "Confirm", new Vector2(-130f, -70f), new Vector2(220f, 58f));
        ButtonParts cancelResetProgress = CreateButton(resetProgressConfirmationPanel.transform, "CancelResetProgressButton", "Cancel", new Vector2(130f, -70f), new Vector2(220f, 58f));

        settingsPanel.SetActive(false);
        creditsPanel.SetActive(false);
        resetProgressConfirmationPanel.SetActive(false);

        controller.Configure(
            mainPanel,
            settingsPanel,
            creditsPanel,
            resetProgressConfirmationPanel,
            titleText,
            startGame.Button,
            startGame.Label,
            settings.Button,
            settings.Label,
            credits.Button,
            credits.Label,
            quit.Button,
            quit.Label,
            settingsTitle,
            masterVolumeLabel,
            volumeSlider,
            languageLabel,
            languageDropdown,
            controlsTitle,
            upRow.Label,
            downRow.Label,
            leftRow.Label,
            rightRow.Label,
            upRow.Button,
            upRow.ButtonLabel,
            downRow.Button,
            downRow.ButtonLabel,
            leftRow.Button,
            leftRow.ButtonLabel,
            rightRow.Button,
            rightRow.ButtonLabel,
            interactFixedText,
            resetControls.Button,
            resetControls.Label,
            resetProgress.Button,
            resetProgress.Label,
            settingsBack.Button,
            settingsBack.Label,
            captureStatusText,
            creditsTitle,
            creditsBody,
            creditsBack.Button,
            creditsBack.Label,
            resetProgressConfirmationText,
            confirmResetProgress.Button,
            confirmResetProgress.Label,
            cancelResetProgress.Button,
            cancelResetProgress.Label);
    }

    private static void ClearScene(Scene scene)
    {
        GameObject[] roots = scene.GetRootGameObjects();
        for (int i = 0; i < roots.Length; i++)
        {
            Object.DestroyImmediate(roots[i]);
        }
    }

    private static void CreateCamera()
    {
        GameObject cameraObject = new("Main Camera", typeof(Camera), typeof(AudioListener));
        cameraObject.tag = "MainCamera";
        Camera camera = cameraObject.GetComponent<Camera>();
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = BackgroundColor;
        camera.orthographic = true;
        cameraObject.transform.position = new Vector3(0f, 0f, -10f);
    }

    private static void CreateEventSystem()
    {
        new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
    }

    private static GameObject CreatePanel(Transform parent, string name, Color color)
    {
        GameObject panel = new(name, typeof(RectTransform), typeof(Image));
        panel.transform.SetParent(parent, false);
        Image image = panel.GetComponent<Image>();
        image.color = color;
        return panel;
    }

    private static GameObject CreateMainMenuPanel(Transform parent)
    {
        GameObject panel = CreatePanel(parent, "MainPanel", MainMenuPanelColor);
        SetAnchored(panel.GetComponent<RectTransform>(), new Vector2(0.255f, 0.405f), new Vector2(560f, 310f), Vector2.zero);

        Image image = panel.GetComponent<Image>();
        image.raycastTarget = false;

        VerticalLayoutGroup layout = panel.AddComponent<VerticalLayoutGroup>();
        layout.childAlignment = TextAnchor.MiddleLeft;
        layout.childControlWidth = false;
        layout.childControlHeight = false;
        layout.childForceExpandWidth = false;
        layout.childForceExpandHeight = false;
        layout.spacing = 14f;

        return panel;
    }

    private static Text CreateText(Transform parent, string name, string value, int fontSize, TextAnchor alignment, Color color)
    {
        GameObject textObject = new(name, typeof(RectTransform), typeof(Text));
        textObject.transform.SetParent(parent, false);

        Text text = textObject.GetComponent<Text>();
        text.font = GetBuiltInFont();
        text.text = value;
        text.fontSize = fontSize;
        text.alignment = alignment;
        text.color = color;
        text.horizontalOverflow = HorizontalWrapMode.Wrap;
        text.verticalOverflow = VerticalWrapMode.Overflow;
        return text;
    }

    private static ButtonParts CreateButton(Transform parent, string name, string label, Vector2 anchoredPosition)
    {
        return CreateButton(parent, name, label, anchoredPosition, new Vector2(360f, 64f));
    }

    private static ButtonParts CreateButton(Transform parent, string name, string label, Vector2 anchoredPosition, Vector2 size)
    {
        GameObject buttonObject = new(name, typeof(RectTransform), typeof(Image), typeof(Button));
        buttonObject.transform.SetParent(parent, false);
        SetAnchored(buttonObject.GetComponent<RectTransform>(), new Vector2(0.5f, 0.5f), size, anchoredPosition);

        Image image = buttonObject.GetComponent<Image>();
        image.color = ButtonColor;

        Button button = buttonObject.GetComponent<Button>();
        ColorBlock colors = button.colors;
        colors.normalColor = ButtonColor;
        colors.highlightedColor = ButtonHighlightColor;
        colors.selectedColor = ButtonHighlightColor;
        colors.pressedColor = new Color(0.35f, 0.15f, 0.05f, 0.42f);
        colors.disabledColor = new Color(0.15f, 0.1f, 0.07f, 0.24f);
        button.colors = colors;

        Text text = CreateText(buttonObject.transform, "Text", label, 26, TextAnchor.MiddleCenter, TextColor);
        Stretch(text.rectTransform, new Vector2(18f, 8f), new Vector2(-18f, -8f));
        AddWarmTextEffects(text, new Vector2(1.5f, -1.5f));

        return new ButtonParts(button, text);
    }

    private static ButtonParts CreateMainMenuButton(Transform parent, string name, string label)
    {
        GameObject buttonObject = new(name, typeof(RectTransform), typeof(Image), typeof(Button), typeof(LayoutElement));
        buttonObject.transform.SetParent(parent, false);

        RectTransform rect = buttonObject.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(520f, 66f);

        LayoutElement layout = buttonObject.GetComponent<LayoutElement>();
        layout.minWidth = 520f;
        layout.preferredWidth = 520f;
        layout.minHeight = 66f;
        layout.preferredHeight = 66f;

        Image image = buttonObject.GetComponent<Image>();
        image.color = TransparentButtonColor;
        image.raycastTarget = true;

        Button button = buttonObject.GetComponent<Button>();
        button.targetGraphic = image;

        Text text = CreateText(buttonObject.transform, "Text", label, 42, TextAnchor.MiddleLeft, TextColor);
        Stretch(text.rectTransform, new Vector2(28f, 4f), new Vector2(-20f, -4f));

        return new ButtonParts(button, text);
    }

    private static void StyleMainMenuButton(ButtonParts buttonParts)
    {
        Image image = buttonParts.Button.GetComponent<Image>();
        image.color = TransparentButtonColor;

        ColorBlock colors = buttonParts.Button.colors;
        colors.normalColor = TransparentButtonColor;
        colors.highlightedColor = new Color(0.8f, 0.42f, 0.14f, 0.08f);
        colors.selectedColor = colors.highlightedColor;
        colors.pressedColor = new Color(0.36f, 0.16f, 0.05f, 0.14f);
        colors.disabledColor = new Color(0.12f, 0.08f, 0.05f, 0f);
        buttonParts.Button.colors = colors;

        buttonParts.Label.fontSize = 42;
        buttonParts.Label.fontStyle = FontStyle.Bold;
        buttonParts.Label.alignment = TextAnchor.MiddleLeft;
        buttonParts.Label.color = TextColor;
        Stretch(buttonParts.Label.rectTransform, new Vector2(28f, 4f), new Vector2(-20f, -4f));
        AddWarmTextEffects(buttonParts.Label, new Vector2(2f, -2f));
    }

    private static void AddWarmTextEffects(Text text, Vector2 distance)
    {
        Outline outline = text.gameObject.GetComponent<Outline>();
        if (outline == null)
        {
            outline = text.gameObject.AddComponent<Outline>();
        }

        outline.effectColor = ButtonOutlineColor;
        outline.effectDistance = distance;

        Shadow shadow = text.gameObject.GetComponent<Shadow>();
        if (shadow == null)
        {
            shadow = text.gameObject.AddComponent<Shadow>();
        }

        shadow.effectColor = new Color(0.16f, 0.06f, 0.02f, 0.72f);
        shadow.effectDistance = distance * 1.8f;
    }

    private static GameObject CreateSettingsRowsContainer(Transform parent)
    {
        GameObject container = new("SettingsRows", typeof(RectTransform), typeof(VerticalLayoutGroup));
        container.transform.SetParent(parent, false);
        SetAnchored(container.GetComponent<RectTransform>(), new Vector2(0.5f, 0.74f), new Vector2(720f, 136f), Vector2.zero);

        VerticalLayoutGroup layout = container.GetComponent<VerticalLayoutGroup>();
        layout.childAlignment = TextAnchor.MiddleCenter;
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = false;
        layout.childForceExpandHeight = false;
        layout.spacing = 18f;

        return container;
    }

    private static Text CreateSettingsRowLabel(Transform parent, string name, string value)
    {
        GameObject row = new(name.Replace("Label", "Row"), typeof(RectTransform), typeof(HorizontalLayoutGroup), typeof(LayoutElement));
        row.transform.SetParent(parent, false);

        LayoutElement rowLayoutElement = row.GetComponent<LayoutElement>();
        rowLayoutElement.minWidth = 720f;
        rowLayoutElement.preferredWidth = 720f;
        rowLayoutElement.minHeight = 58f;
        rowLayoutElement.preferredHeight = 58f;

        HorizontalLayoutGroup rowLayout = row.GetComponent<HorizontalLayoutGroup>();
        rowLayout.childAlignment = TextAnchor.MiddleCenter;
        rowLayout.childControlWidth = true;
        rowLayout.childControlHeight = true;
        rowLayout.childForceExpandWidth = false;
        rowLayout.childForceExpandHeight = false;
        rowLayout.spacing = 34f;

        Text label = CreateText(row.transform, name, value, 26, TextAnchor.MiddleLeft, TextColor);
        AddLayoutElement(label.gameObject, 240f, 58f);
        return label;
    }

    private static Slider CreateSlider(Transform parent, string name)
    {
        GameObject sliderObject = new(name, typeof(RectTransform), typeof(Slider), typeof(LayoutElement));
        sliderObject.transform.SetParent(parent, false);
        sliderObject.GetComponent<RectTransform>().sizeDelta = new Vector2(380f, 44f);
        AddLayoutElement(sliderObject, 380f, 44f);

        GameObject background = CreatePanel(sliderObject.transform, "Background", new Color(0.02f, 0.02f, 0.025f, 1f));
        Stretch(background.GetComponent<RectTransform>(), new Vector2(0f, 12f), new Vector2(0f, -12f));

        GameObject fillArea = new("Fill Area", typeof(RectTransform));
        fillArea.transform.SetParent(sliderObject.transform, false);
        Stretch(fillArea.GetComponent<RectTransform>(), new Vector2(8f, 12f), new Vector2(-8f, -12f));

        GameObject fill = CreatePanel(fillArea.transform, "Fill", new Color(0.72f, 0.58f, 0.28f, 1f));
        Stretch(fill.GetComponent<RectTransform>());

        GameObject handleArea = new("Handle Slide Area", typeof(RectTransform));
        handleArea.transform.SetParent(sliderObject.transform, false);
        Stretch(handleArea.GetComponent<RectTransform>(), new Vector2(8f, 0f), new Vector2(-8f, 0f));

        GameObject handle = CreatePanel(handleArea.transform, "Handle", TextColor);
        SetAnchored(handle.GetComponent<RectTransform>(), new Vector2(0.5f, 0.5f), new Vector2(28f, 38f), Vector2.zero);

        Slider slider = sliderObject.GetComponent<Slider>();
        slider.minValue = 0f;
        slider.maxValue = 1f;
        slider.value = 1f;
        slider.fillRect = fill.GetComponent<RectTransform>();
        slider.handleRect = handle.GetComponent<RectTransform>();
        slider.targetGraphic = handle.GetComponent<Image>();
        return slider;
    }

    private static Dropdown CreateDropdown(Transform parent, string name)
    {
        GameObject dropdownObject = new(name, typeof(RectTransform), typeof(Image), typeof(Dropdown), typeof(LayoutElement));
        dropdownObject.transform.SetParent(parent, false);
        dropdownObject.GetComponent<RectTransform>().sizeDelta = new Vector2(380f, 58f);
        AddLayoutElement(dropdownObject, 380f, 58f);

        Image image = dropdownObject.GetComponent<Image>();
        image.color = ButtonColor;

        Text label = CreateText(dropdownObject.transform, "Label", "English", 24, TextAnchor.MiddleLeft, TextColor);
        Stretch(label.rectTransform, new Vector2(18f, 6f), new Vector2(-56f, -6f));

        Text arrow = CreateText(dropdownObject.transform, "Arrow", "v", 22, TextAnchor.MiddleCenter, TextColor);
        SetAnchored(arrow.rectTransform, new Vector2(1f, 0.5f), new Vector2(44f, 44f), new Vector2(-28f, 0f));

        GameObject template = CreateDropdownTemplate(dropdownObject.transform);

        Dropdown dropdown = dropdownObject.GetComponent<Dropdown>();
        dropdown.targetGraphic = image;
        dropdown.captionText = label;
        dropdown.template = template.GetComponent<RectTransform>();
        dropdown.itemText = template.transform.Find("Viewport/Content/Item/Item Label").GetComponent<Text>();
        dropdown.options = new List<Dropdown.OptionData>
        {
            new("English"),
            new("简体中文")
        };
        template.SetActive(false);
        return dropdown;
    }

    private static void AddLayoutElement(GameObject target, float preferredWidth, float preferredHeight)
    {
        LayoutElement layout = target.GetComponent<LayoutElement>();
        if (layout == null)
        {
            layout = target.AddComponent<LayoutElement>();
        }

        layout.minWidth = preferredWidth;
        layout.preferredWidth = preferredWidth;
        layout.minHeight = preferredHeight;
        layout.preferredHeight = preferredHeight;
    }

    private static GameObject CreateDropdownTemplate(Transform parent)
    {
        GameObject template = new("Template", typeof(RectTransform), typeof(Image), typeof(ScrollRect));
        template.transform.SetParent(parent, false);
        SetAnchored(template.GetComponent<RectTransform>(), new Vector2(0.5f, 0f), new Vector2(420f, 140f), new Vector2(0f, -100f));
        template.GetComponent<Image>().color = new Color(0.06f, 0.055f, 0.07f, 1f);

        GameObject viewport = new("Viewport", typeof(RectTransform), typeof(Image), typeof(Mask));
        viewport.transform.SetParent(template.transform, false);
        Stretch(viewport.GetComponent<RectTransform>());
        viewport.GetComponent<Image>().color = new Color(0.02f, 0.02f, 0.025f, 1f);
        viewport.GetComponent<Mask>().showMaskGraphic = false;

        GameObject content = new("Content", typeof(RectTransform));
        content.transform.SetParent(viewport.transform, false);
        RectTransform contentRect = content.GetComponent<RectTransform>();
        contentRect.anchorMin = new Vector2(0f, 1f);
        contentRect.anchorMax = new Vector2(1f, 1f);
        contentRect.pivot = new Vector2(0.5f, 1f);
        contentRect.sizeDelta = new Vector2(0f, 64f);

        GameObject item = new("Item", typeof(RectTransform), typeof(Toggle));
        item.transform.SetParent(content.transform, false);
        RectTransform itemRect = item.GetComponent<RectTransform>();
        itemRect.anchorMin = new Vector2(0f, 0.5f);
        itemRect.anchorMax = new Vector2(1f, 0.5f);
        itemRect.sizeDelta = new Vector2(0f, 40f);

        GameObject itemBackground = CreatePanel(item.transform, "Item Background", ButtonColor);
        Stretch(itemBackground.GetComponent<RectTransform>());

        GameObject itemCheckmark = CreatePanel(item.transform, "Item Checkmark", TextColor);
        SetAnchored(itemCheckmark.GetComponent<RectTransform>(), new Vector2(0f, 0.5f), new Vector2(20f, 20f), new Vector2(20f, 0f));

        Text itemLabel = CreateText(item.transform, "Item Label", "Option", 22, TextAnchor.MiddleLeft, TextColor);
        Stretch(itemLabel.rectTransform, new Vector2(52f, 4f), new Vector2(-12f, -4f));

        Toggle toggle = item.GetComponent<Toggle>();
        toggle.targetGraphic = itemBackground.GetComponent<Image>();
        toggle.graphic = itemCheckmark.GetComponent<Image>();

        ScrollRect scrollRect = template.GetComponent<ScrollRect>();
        scrollRect.content = contentRect;
        scrollRect.viewport = viewport.GetComponent<RectTransform>();
        scrollRect.horizontal = false;
        scrollRect.movementType = ScrollRect.MovementType.Clamped;

        return template;
    }

    private static ControlRow CreateControlRow(Transform parent, string name, string label, Vector2 anchoredPosition, string binding)
    {
        GameObject row = new(name, typeof(RectTransform));
        row.transform.SetParent(parent, false);
        SetAnchored(row.GetComponent<RectTransform>(), new Vector2(0.5f, 0.43f), new Vector2(520f, 56f), anchoredPosition);

        Text labelText = CreateText(row.transform, "Label", label, 24, TextAnchor.MiddleLeft, TextColor);
        Stretch(labelText.rectTransform, new Vector2(0f, 4f), new Vector2(-250f, -4f));

        ButtonParts button = CreateButton(row.transform, "RebindButton", binding, new Vector2(150f, 0f), new Vector2(180f, 48f));
        return new ControlRow(labelText, button.Button, button.Label);
    }

    private static void InsertMainMenuFirstInBuildSettings()
    {
        List<EditorBuildSettingsScene> scenes = new()
        {
            new EditorBuildSettingsScene(ScenePath, true)
        };

        EditorBuildSettingsScene[] existingScenes = EditorBuildSettings.scenes;
        for (int i = 0; i < existingScenes.Length; i++)
        {
            if (NormalizePath(existingScenes[i].path) == NormalizePath(ScenePath))
            {
                continue;
            }

            scenes.Add(existingScenes[i]);
        }

        EditorBuildSettings.scenes = scenes.ToArray();
    }

    private static string NormalizePath(string path)
    {
        return path.Replace('\\', '/').Trim();
    }

    private static void SetAnchored(RectTransform rectTransform, Vector2 anchor, Vector2 size, Vector2 anchoredPosition)
    {
        rectTransform.anchorMin = anchor;
        rectTransform.anchorMax = anchor;
        rectTransform.pivot = new Vector2(0.5f, 0.5f);
        rectTransform.sizeDelta = size;
        rectTransform.anchoredPosition = anchoredPosition;
    }

    private static void Stretch(RectTransform rectTransform)
    {
        Stretch(rectTransform, Vector2.zero, Vector2.zero);
    }

    private static void Stretch(RectTransform rectTransform, Vector2 offsetMin, Vector2 offsetMax)
    {
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.one;
        rectTransform.offsetMin = offsetMin;
        rectTransform.offsetMax = offsetMax;
    }

    private static Font GetBuiltInFont()
    {
        return Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
    }

    private readonly struct ButtonParts
    {
        public ButtonParts(Button button, Text label)
        {
            Button = button;
            Label = label;
        }

        public Button Button { get; }
        public Text Label { get; }
    }

    private readonly struct ControlRow
    {
        public ControlRow(Text label, Button button, Text buttonLabel)
        {
            Label = label;
            Button = button;
            ButtonLabel = buttonLabel;
        }

        public Text Label { get; }
        public Button Button { get; }
        public Text ButtonLabel { get; }
    }
}
