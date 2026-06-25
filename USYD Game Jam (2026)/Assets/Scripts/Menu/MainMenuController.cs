using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class MainMenuController : MonoBehaviour
{
    private const string EnglishCreditsBody =
        "USYD Game Jam 2026\n\n" +
        "Team Credits:\n" +
        "Linh Tran\n" +
        "Sam Kero\n" +
        "Zia Zheng\n" +
        "Xiangwen Guo";

    private const string ChineseCreditsBody =
        "USYD Game Jam 2026\n\n" +
        "鍒朵綔浜哄憳锛?\n" +
        "Linh Tran\n" +
        "Sam Kero\n" +
        "Zia Zheng\n" +
        "Xiangwen Guo";

    [Header("Panels")]
    [SerializeField] private GameObject mainPanel;
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private GameObject creditsPanel;
    [SerializeField] private GameObject resetProgressConfirmationPanel;

    [Header("Main")]
    [SerializeField] private Text titleText;
    [SerializeField] private Button startGameButton;
    [SerializeField] private Text startGameButtonText;
    [SerializeField] private Button settingsButton;
    [SerializeField] private Text settingsButtonText;
    [SerializeField] private Button creditsButton;
    [SerializeField] private Text creditsButtonText;
    [SerializeField] private Button quitButton;
    [SerializeField] private Text quitButtonText;

    [Header("Settings")]
    [SerializeField] private Text settingsTitleText;
    [SerializeField] private Text masterVolumeLabelText;
    [SerializeField] private Slider masterVolumeSlider;
    [SerializeField] private Text languageLabelText;
    [SerializeField] private Dropdown languageDropdown;
    [SerializeField] private Text controlsTitleText;
    [SerializeField] private Text upLabelText;
    [SerializeField] private Text downLabelText;
    [SerializeField] private Text leftLabelText;
    [SerializeField] private Text rightLabelText;
    [SerializeField] private Button rebindUpButton;
    [SerializeField] private Text rebindUpButtonText;
    [SerializeField] private Button rebindDownButton;
    [SerializeField] private Text rebindDownButtonText;
    [SerializeField] private Button rebindLeftButton;
    [SerializeField] private Text rebindLeftButtonText;
    [SerializeField] private Button rebindRightButton;
    [SerializeField] private Text rebindRightButtonText;
    [SerializeField] private Text interactFixedText;
    [SerializeField] private Button resetControlsButton;
    [SerializeField] private Text resetControlsButtonText;
    [SerializeField] private Button resetProgressButton;
    [SerializeField] private Text resetProgressButtonText;
    [SerializeField] private Button settingsBackButton;
    [SerializeField] private Text settingsBackButtonText;
    [SerializeField] private Text captureStatusText;

    [Header("Credits")]
    [SerializeField] private Text creditsTitleText;
    [SerializeField] private Text creditsBodyText;
    [SerializeField] private Button creditsBackButton;
    [SerializeField] private Text creditsBackButtonText;

    [Header("Reset Progress Confirmation")]
    [SerializeField] private Text resetProgressConfirmationText;
    [SerializeField] private Button confirmResetProgressButton;
    [SerializeField] private Text confirmResetProgressButtonText;
    [SerializeField] private Button cancelResetProgressButton;
    [SerializeField] private Text cancelResetProgressButtonText;

    private HotelHungerRuntimeManager runtimeManager;
    private bool listenersBound;
    private bool suppressUiEvents;
    private bool isCapturingBinding;
    private HotelHungerRuntimeManager.MovementDirection captureDirection;

    private void Awake()
    {
        runtimeManager = HotelHungerRuntimeManager.Instance;
    }

    private void OnEnable()
    {
        runtimeManager = HotelHungerRuntimeManager.Instance;
        BindListeners();

        if (runtimeManager != null)
        {
            runtimeManager.SettingsChanged += RefreshAll;
        }

        ShowMainPanel();
        RefreshAll();
    }

    private void OnDisable()
    {
        if (runtimeManager != null)
        {
            runtimeManager.SettingsChanged -= RefreshAll;
        }

        UnbindListeners();
    }

    private void Update()
    {
        if (isCapturingBinding)
        {
            UpdateBindingCapture();
        }
    }

    public void Configure(
        GameObject newMainPanel,
        GameObject newSettingsPanel,
        GameObject newCreditsPanel,
        GameObject newResetProgressConfirmationPanel,
        Text newTitleText,
        Button newStartGameButton,
        Text newStartGameButtonText,
        Button newSettingsButton,
        Text newSettingsButtonText,
        Button newCreditsButton,
        Text newCreditsButtonText,
        Button newQuitButton,
        Text newQuitButtonText,
        Text newSettingsTitleText,
        Text newMasterVolumeLabelText,
        Slider newMasterVolumeSlider,
        Text newLanguageLabelText,
        Dropdown newLanguageDropdown,
        Text newControlsTitleText,
        Text newUpLabelText,
        Text newDownLabelText,
        Text newLeftLabelText,
        Text newRightLabelText,
        Button newRebindUpButton,
        Text newRebindUpButtonText,
        Button newRebindDownButton,
        Text newRebindDownButtonText,
        Button newRebindLeftButton,
        Text newRebindLeftButtonText,
        Button newRebindRightButton,
        Text newRebindRightButtonText,
        Text newInteractFixedText,
        Button newResetControlsButton,
        Text newResetControlsButtonText,
        Button newResetProgressButton,
        Text newResetProgressButtonText,
        Button newSettingsBackButton,
        Text newSettingsBackButtonText,
        Text newCaptureStatusText,
        Text newCreditsTitleText,
        Text newCreditsBodyText,
        Button newCreditsBackButton,
        Text newCreditsBackButtonText,
        Text newResetProgressConfirmationText,
        Button newConfirmResetProgressButton,
        Text newConfirmResetProgressButtonText,
        Button newCancelResetProgressButton,
        Text newCancelResetProgressButtonText)
    {
        mainPanel = newMainPanel;
        settingsPanel = newSettingsPanel;
        creditsPanel = newCreditsPanel;
        resetProgressConfirmationPanel = newResetProgressConfirmationPanel;
        titleText = newTitleText;
        startGameButton = newStartGameButton;
        startGameButtonText = newStartGameButtonText;
        settingsButton = newSettingsButton;
        settingsButtonText = newSettingsButtonText;
        creditsButton = newCreditsButton;
        creditsButtonText = newCreditsButtonText;
        quitButton = newQuitButton;
        quitButtonText = newQuitButtonText;
        settingsTitleText = newSettingsTitleText;
        masterVolumeLabelText = newMasterVolumeLabelText;
        masterVolumeSlider = newMasterVolumeSlider;
        languageLabelText = newLanguageLabelText;
        languageDropdown = newLanguageDropdown;
        controlsTitleText = newControlsTitleText;
        upLabelText = newUpLabelText;
        downLabelText = newDownLabelText;
        leftLabelText = newLeftLabelText;
        rightLabelText = newRightLabelText;
        rebindUpButton = newRebindUpButton;
        rebindUpButtonText = newRebindUpButtonText;
        rebindDownButton = newRebindDownButton;
        rebindDownButtonText = newRebindDownButtonText;
        rebindLeftButton = newRebindLeftButton;
        rebindLeftButtonText = newRebindLeftButtonText;
        rebindRightButton = newRebindRightButton;
        rebindRightButtonText = newRebindRightButtonText;
        interactFixedText = newInteractFixedText;
        resetControlsButton = newResetControlsButton;
        resetControlsButtonText = newResetControlsButtonText;
        resetProgressButton = newResetProgressButton;
        resetProgressButtonText = newResetProgressButtonText;
        settingsBackButton = newSettingsBackButton;
        settingsBackButtonText = newSettingsBackButtonText;
        captureStatusText = newCaptureStatusText;
        creditsTitleText = newCreditsTitleText;
        creditsBodyText = newCreditsBodyText;
        creditsBackButton = newCreditsBackButton;
        creditsBackButtonText = newCreditsBackButtonText;
        resetProgressConfirmationText = newResetProgressConfirmationText;
        confirmResetProgressButton = newConfirmResetProgressButton;
        confirmResetProgressButtonText = newConfirmResetProgressButtonText;
        cancelResetProgressButton = newCancelResetProgressButton;
        cancelResetProgressButtonText = newCancelResetProgressButtonText;
    }

    private void BindListeners()
    {
        if (listenersBound)
        {
            return;
        }

        AddClick(startGameButton, HandleStartGame);
        AddClick(settingsButton, ShowSettingsPanel);
        AddClick(creditsButton, ShowCreditsPanel);
        AddClick(quitButton, HandleQuitGame);
        AddClick(settingsBackButton, ShowMainPanel);
        AddClick(creditsBackButton, ShowMainPanel);
        AddClick(resetControlsButton, HandleResetControls);
        AddClick(resetProgressButton, ShowResetProgressConfirmation);
        AddClick(confirmResetProgressButton, HandleConfirmResetProgress);
        AddClick(cancelResetProgressButton, HideResetProgressConfirmation);
        AddClick(rebindUpButton, HandleRebindUp);
        AddClick(rebindDownButton, HandleRebindDown);
        AddClick(rebindLeftButton, HandleRebindLeft);
        AddClick(rebindRightButton, HandleRebindRight);

        if (masterVolumeSlider != null)
        {
            masterVolumeSlider.onValueChanged.AddListener(HandleVolumeChanged);
        }

        if (languageDropdown != null)
        {
            languageDropdown.onValueChanged.AddListener(HandleLanguageChanged);
        }

        listenersBound = true;
    }

    private void UnbindListeners()
    {
        if (!listenersBound)
        {
            return;
        }

        RemoveClick(startGameButton, HandleStartGame);
        RemoveClick(settingsButton, ShowSettingsPanel);
        RemoveClick(creditsButton, ShowCreditsPanel);
        RemoveClick(quitButton, HandleQuitGame);
        RemoveClick(settingsBackButton, ShowMainPanel);
        RemoveClick(creditsBackButton, ShowMainPanel);
        RemoveClick(resetControlsButton, HandleResetControls);
        RemoveClick(resetProgressButton, ShowResetProgressConfirmation);
        RemoveClick(confirmResetProgressButton, HandleConfirmResetProgress);
        RemoveClick(cancelResetProgressButton, HideResetProgressConfirmation);

        RemoveClick(rebindUpButton, HandleRebindUp);
        RemoveClick(rebindDownButton, HandleRebindDown);
        RemoveClick(rebindLeftButton, HandleRebindLeft);
        RemoveClick(rebindRightButton, HandleRebindRight);

        if (masterVolumeSlider != null)
        {
            masterVolumeSlider.onValueChanged.RemoveListener(HandleVolumeChanged);
        }

        if (languageDropdown != null)
        {
            languageDropdown.onValueChanged.RemoveListener(HandleLanguageChanged);
        }

        listenersBound = false;
    }

    private void ShowMainPanel()
    {
        SetActive(mainPanel, true);
        SetActive(settingsPanel, false);
        SetActive(creditsPanel, false);
        SetActive(resetProgressConfirmationPanel, false);
        isCapturingBinding = false;
        SetText(captureStatusText, string.Empty);
    }

    private void ShowSettingsPanel()
    {
        SetActive(mainPanel, false);
        SetActive(settingsPanel, true);
        SetActive(creditsPanel, false);
        SetActive(resetProgressConfirmationPanel, false);
        isCapturingBinding = false;
        RefreshAll();
    }

    private void ShowCreditsPanel()
    {
        SetActive(mainPanel, false);
        SetActive(settingsPanel, false);
        SetActive(creditsPanel, true);
        SetActive(resetProgressConfirmationPanel, false);
        isCapturingBinding = false;
        RefreshAll();
    }

    private void ShowResetProgressConfirmation()
    {
        SetActive(resetProgressConfirmationPanel, true);
    }

    private void HideResetProgressConfirmation()
    {
        SetActive(resetProgressConfirmationPanel, false);
    }

    private void HandleStartGame()
    {
        if (runtimeManager == null || runtimeManager.IsLoading)
        {
            return;
        }

        runtimeManager.StartNewGame();
    }

    private void HandleQuitGame()
    {
#if UNITY_EDITOR
        Debug.Log("Quit Game requested. Application.Quit only works in a built player.");
#else
        Application.Quit();
#endif
    }

    private void HandleVolumeChanged(float value)
    {
        if (suppressUiEvents || runtimeManager == null)
        {
            return;
        }

        runtimeManager.SetMasterVolume(value);
    }

    private void HandleLanguageChanged(int value)
    {
        if (suppressUiEvents || runtimeManager == null)
        {
            return;
        }

        runtimeManager.SetLanguage(value == 1
            ? HotelHungerRuntimeManager.ChineseLanguageCode
            : HotelHungerRuntimeManager.EnglishLanguageCode);
    }

    private void HandleResetControls()
    {
        HotelHungerRuntimeManager.ResetMovementBindings();
        isCapturingBinding = false;
        SetText(captureStatusText, string.Empty);
        RefreshControlBindings();
    }

    private void HandleRebindUp()
    {
        BeginBindingCapture(HotelHungerRuntimeManager.MovementDirection.Up);
    }

    private void HandleRebindDown()
    {
        BeginBindingCapture(HotelHungerRuntimeManager.MovementDirection.Down);
    }

    private void HandleRebindLeft()
    {
        BeginBindingCapture(HotelHungerRuntimeManager.MovementDirection.Left);
    }

    private void HandleRebindRight()
    {
        BeginBindingCapture(HotelHungerRuntimeManager.MovementDirection.Right);
    }

    private void HandleConfirmResetProgress()
    {
        runtimeManager?.ClearCheckpoint();
        HideResetProgressConfirmation();
    }

    private void BeginBindingCapture(HotelHungerRuntimeManager.MovementDirection direction)
    {
        isCapturingBinding = true;
        captureDirection = direction;
        SetText(captureStatusText, Localize("Press a new key. Escape cancels."));
    }

    private void UpdateBindingCapture()
    {
        Keyboard keyboard = Keyboard.current;
        if (keyboard == null)
        {
            return;
        }

        if (keyboard.escapeKey.wasPressedThisFrame)
        {
            isCapturingBinding = false;
            SetText(captureStatusText, string.Empty);
            return;
        }

        foreach (var keyControl in keyboard.allKeys)
        {
            if (!keyControl.wasPressedThisFrame)
            {
                continue;
            }

            TryApplyCapturedKey(keyControl.keyCode);
            return;
        }
    }

    private void TryApplyCapturedKey(Key key)
    {
        if (!HotelHungerRuntimeManager.IsValidPrimaryMovementBinding(key))
        {
            SetText(captureStatusText, Localize("Key unavailable."));
            return;
        }

        foreach (HotelHungerRuntimeManager.MovementDirection direction in
            System.Enum.GetValues(typeof(HotelHungerRuntimeManager.MovementDirection)))
        {
            if (direction != captureDirection &&
                HotelHungerRuntimeManager.GetMovementBinding(direction) == key)
            {
                SetText(captureStatusText, Localize("Key already used."));
                return;
            }
        }

        HotelHungerRuntimeManager.SetMovementBinding(captureDirection, key);
        isCapturingBinding = false;
        SetText(captureStatusText, string.Empty);
        RefreshControlBindings();
    }

    private void RefreshAll()
    {
        RefreshLocalizedLabels();
        RefreshSettingsControls();
        RefreshControlBindings();
    }

    private void RefreshLocalizedLabels()
    {
        SetText(titleText, string.Empty);
        SetText(startGameButtonText, Localize("Start Game"));
        SetText(settingsButtonText, Localize("Settings"));
        SetText(creditsButtonText, Localize("Credits"));
        SetText(quitButtonText, Localize("Quit Game"));
        SetText(settingsTitleText, Localize("Settings"));
        SetText(masterVolumeLabelText, Localize("Master Volume"));
        SetText(languageLabelText, Localize("Language"));
        SetText(controlsTitleText, Localize("Controls"));
        SetText(upLabelText, Localize("Up"));
        SetText(downLabelText, Localize("Down"));
        SetText(leftLabelText, Localize("Left"));
        SetText(rightLabelText, Localize("Right"));
        SetText(interactFixedText, Localize("Interact: E (fixed)"));
        SetText(resetControlsButtonText, Localize("Reset Controls"));
        SetText(resetProgressButtonText, Localize("Reset Progress"));
        SetText(settingsBackButtonText, Localize("Back"));
        SetText(creditsTitleText, Localize("Credits"));
        SetText(creditsBodyText, GetCreditsBody());
        SetText(creditsBackButtonText, Localize("Back"));
        SetText(resetProgressConfirmationText, Localize("Reset progress confirmation"));
        SetText(confirmResetProgressButtonText, Localize("Confirm"));
        SetText(cancelResetProgressButtonText, Localize("Cancel"));
    }

    private string GetCreditsBody()
    {
        bool chinese = runtimeManager != null &&
            runtimeManager.GetLanguage() == HotelHungerRuntimeManager.ChineseLanguageCode;
        return chinese ? ChineseCreditsBody : EnglishCreditsBody;
    }

    private void RefreshSettingsControls()
    {
        suppressUiEvents = true;

        if (masterVolumeSlider != null && runtimeManager != null)
        {
            masterVolumeSlider.SetValueWithoutNotify(runtimeManager.GetMasterVolume());
        }

        if (languageDropdown != null)
        {
            languageDropdown.ClearOptions();
            languageDropdown.AddOptions(new System.Collections.Generic.List<string> { "English", "简体中文" });
            int languageIndex = runtimeManager != null &&
                runtimeManager.GetLanguage() == HotelHungerRuntimeManager.ChineseLanguageCode ? 1 : 0;
            languageDropdown.SetValueWithoutNotify(languageIndex);
        }

        suppressUiEvents = false;
    }

    private void RefreshControlBindings()
    {
        SetText(rebindUpButtonText, KeyDisplay(HotelHungerRuntimeManager.GetMovementBinding(HotelHungerRuntimeManager.MovementDirection.Up)));
        SetText(rebindDownButtonText, KeyDisplay(HotelHungerRuntimeManager.GetMovementBinding(HotelHungerRuntimeManager.MovementDirection.Down)));
        SetText(rebindLeftButtonText, KeyDisplay(HotelHungerRuntimeManager.GetMovementBinding(HotelHungerRuntimeManager.MovementDirection.Left)));
        SetText(rebindRightButtonText, KeyDisplay(HotelHungerRuntimeManager.GetMovementBinding(HotelHungerRuntimeManager.MovementDirection.Right)));
    }

    private string Localize(string english)
    {
        bool chinese = runtimeManager != null &&
            runtimeManager.GetLanguage() == HotelHungerRuntimeManager.ChineseLanguageCode;
        if (!chinese)
        {
            return english;
        }

        return english switch
        {
            "Start Game" => "开始游戏",
            "Settings" => "设置",
            "Credits" => "制作名单",
            "Quit Game" => "退出游戏",
            "Back" => "返回",
            "Master Volume" => "主音量",
            "Language" => "语言",
            "Controls" => "按键设置",
            "Reset Controls" => "重置按键",
            "Reset Progress" => "重置进度",
            "Confirm" => "确认",
            "Cancel" => "取消",
            "Interact: E (fixed)" => "互动：E（固定）",
            "Up" => "上",
            "Down" => "下",
            "Left" => "左",
            "Right" => "右",
            "Press a new key. Escape cancels." => "按一个新按键，Escape 取消。",
            "Key unavailable." => "该按键不可用。",
            "Key already used." => "该按键已被使用。",
            "Reset progress confirmation" => "确定要重置进度吗？",
            _ => english
        };
    }

    private static string KeyDisplay(Key key)
    {
        return key.ToString();
    }

    private static void AddClick(Button button, UnityEngine.Events.UnityAction action)
    {
        if (button != null)
        {
            button.onClick.AddListener(action);
        }
    }

    private static void RemoveClick(Button button, UnityEngine.Events.UnityAction action)
    {
        if (button != null)
        {
            button.onClick.RemoveListener(action);
        }
    }

    private static void SetActive(GameObject target, bool active)
    {
        if (target != null)
        {
            target.SetActive(active);
        }
    }

    private static void SetText(Text text, string value)
    {
        if (text != null)
        {
            text.text = value;
        }
    }
}
