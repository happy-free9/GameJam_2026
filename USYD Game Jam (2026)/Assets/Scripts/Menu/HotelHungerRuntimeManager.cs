using System;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public sealed class HotelHungerRuntimeManager : MonoBehaviour
{
    public enum MovementDirection
    {
        Up = 0,
        Down = 1,
        Left = 2,
        Right = 3
    }

    public const string MainMenuSceneName = "MainMenu_XW";
    public const string StartSceneName = "HotelLobby_XW";
    public const string EnglishLanguageCode = "en";
    public const string ChineseLanguageCode = "zh-Hans";

    private const string CheckpointKey = "HotelHunger.CheckpointScene";
    private const string VolumeKey = "HotelHunger.MasterVolume";
    private const string LanguageKey = "HotelHunger.Language";
    private const string BindingPrefix = "HotelHunger.Binding.";
    private const float DefaultFadeDuration = 0.6f;
    private const int FadeSortingOrder = 32767;

    private static readonly string[] ValidCheckpointScenes =
    {
        "HotelLobby_XW",
        "Guest1CartHallway_XW",
        "Guest1WaitingHallway_XW",
        "Guest3DiningRoom_XW",
        "Guest3HorrorChase_XW"
    };

    private static HotelHungerRuntimeManager instance;

    private CanvasGroup fadeGroup;
    private Image fadeImage;
    private GameObject runtimeCoreUiRoot;
    private bool isLoading;

    public static HotelHungerRuntimeManager Instance => instance;
    public bool IsLoading => isLoading;
    public event Action CheckpointChanged;
    public event Action SettingsChanged;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Bootstrap()
    {
        if (instance != null)
        {
            return;
        }

        GameObject managerObject = new("HotelHungerRuntimeManager");
        managerObject.AddComponent<HotelHungerRuntimeManager>();
    }

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);

        ApplySavedVolume();
        EnsureFadeOverlay();
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        if (instance == this)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            instance = null;
        }
    }

    public void StartNewGame()
    {
        Guest1RunProgress.ClearAll();
        ClearCheckpoint();
        LoadSceneWithFade(StartSceneName);
    }

    public void ContinueGame()
    {
        if (!TryGetValidCheckpoint(out string checkpointScene))
        {
            Debug.LogWarning("HotelHungerRuntimeManager: Continue requested, but no valid checkpoint exists.", this);
            return;
        }

        LoadSceneWithFade(checkpointScene);
    }

    public void ClearCheckpoint()
    {
        PlayerPrefs.DeleteKey(CheckpointKey);
        PlayerPrefs.Save();
        CheckpointChanged?.Invoke();
    }

    public bool HasValidCheckpoint()
    {
        return TryGetValidCheckpoint(out _);
    }

    public bool TryGetValidCheckpoint(out string checkpointScene)
    {
        checkpointScene = PlayerPrefs.GetString(CheckpointKey, string.Empty);
        if (string.IsNullOrWhiteSpace(checkpointScene))
        {
            return false;
        }

        if (IsValidCheckpointScene(checkpointScene) && SceneExistsInBuildSettings(checkpointScene))
        {
            return true;
        }

        Debug.LogWarning($"HotelHungerRuntimeManager: clearing invalid checkpoint '{checkpointScene}'.", this);
        ClearCheckpoint();
        checkpointScene = string.Empty;
        return false;
    }

    public void LoadSceneWithFade(string sceneName)
    {
        if (isLoading)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(sceneName))
        {
            Debug.LogWarning("HotelHungerRuntimeManager: cannot load an empty scene name.", this);
            return;
        }

        if (!SceneExistsInBuildSettings(sceneName))
        {
            Debug.LogWarning($"HotelHungerRuntimeManager: scene '{sceneName}' is not in Build Settings.", this);
            return;
        }

        StartCoroutine(LoadSceneWithFadeRoutine(sceneName));
    }

    public float GetMasterVolume()
    {
        return AudioListener.volume;
    }

    public void SetMasterVolume(float volume)
    {
        float clampedVolume = Mathf.Clamp01(volume);
        AudioListener.volume = clampedVolume;
        PlayerPrefs.SetFloat(VolumeKey, clampedVolume);
        PlayerPrefs.Save();
        SettingsChanged?.Invoke();
    }

    public string GetLanguage()
    {
        string language = PlayerPrefs.GetString(LanguageKey, EnglishLanguageCode);
        return language == ChineseLanguageCode ? ChineseLanguageCode : EnglishLanguageCode;
    }

    public void SetLanguage(string languageCode)
    {
        string normalizedLanguage = languageCode == ChineseLanguageCode ? ChineseLanguageCode : EnglishLanguageCode;
        PlayerPrefs.SetString(LanguageKey, normalizedLanguage);
        PlayerPrefs.Save();
        SettingsChanged?.Invoke();
    }

    public static Key GetMovementBinding(MovementDirection direction)
    {
        string key = GetBindingKey(direction);
        int defaultValue = (int)GetDefaultMovementBinding(direction);
        int savedValue = PlayerPrefs.GetInt(key, defaultValue);
        if (!Enum.IsDefined(typeof(Key), savedValue))
        {
            return GetDefaultMovementBinding(direction);
        }

        return (Key)savedValue;
    }

    public static void SetMovementBinding(MovementDirection direction, Key key)
    {
        PlayerPrefs.SetInt(GetBindingKey(direction), (int)key);
        PlayerPrefs.Save();
        instance?.SettingsChanged?.Invoke();
    }

    public static void ResetMovementBindings()
    {
        foreach (MovementDirection direction in Enum.GetValues(typeof(MovementDirection)))
        {
            PlayerPrefs.DeleteKey(GetBindingKey(direction));
        }

        PlayerPrefs.Save();
        instance?.SettingsChanged?.Invoke();
    }

    public static Key GetDefaultMovementBinding(MovementDirection direction)
    {
        return direction switch
        {
            MovementDirection.Up => Key.W,
            MovementDirection.Down => Key.S,
            MovementDirection.Left => Key.A,
            MovementDirection.Right => Key.D,
            _ => Key.None
        };
    }

    public static bool IsArrowKey(Key key)
    {
        return key == Key.UpArrow ||
            key == Key.DownArrow ||
            key == Key.LeftArrow ||
            key == Key.RightArrow;
    }

    public static bool IsValidPrimaryMovementBinding(Key key)
    {
        return key != Key.None &&
            key != Key.Escape &&
            key != Key.E &&
            !IsArrowKey(key);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == MainMenuSceneName)
        {
            return;
        }

        if (IsValidCheckpointScene(scene.name))
        {
            EnsureGameplayCoreUi();
            SaveCheckpoint(scene.name);
        }
    }

    private IEnumerator LoadSceneWithFadeRoutine(string sceneName)
    {
        isLoading = true;
        EnsureFadeOverlay();

        yield return FadeTo(1f, DefaultFadeDuration);

        AsyncOperation loadOperation = SceneManager.LoadSceneAsync(sceneName);
        if (loadOperation == null)
        {
            Debug.LogWarning($"HotelHungerRuntimeManager: failed to start loading scene '{sceneName}'.", this);
            yield return FadeTo(0f, DefaultFadeDuration);
            isLoading = false;
            yield break;
        }

        while (!loadOperation.isDone)
        {
            yield return null;
        }

        yield return null;
        yield return FadeTo(0f, DefaultFadeDuration);
        isLoading = false;
    }

    private IEnumerator FadeTo(float targetAlpha, float duration)
    {
        EnsureFadeOverlay();

        fadeGroup.blocksRaycasts = true;
        fadeGroup.interactable = true;

        float startAlpha = fadeGroup.alpha;
        if (duration <= 0f)
        {
            fadeGroup.alpha = targetAlpha;
        }
        else
        {
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                fadeGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, t);
                yield return null;
            }
        }

        fadeGroup.alpha = targetAlpha;

        if (Mathf.Approximately(targetAlpha, 0f))
        {
            fadeGroup.blocksRaycasts = false;
            fadeGroup.interactable = false;
        }
    }

    private void EnsureFadeOverlay()
    {
        if (fadeGroup != null && fadeImage != null)
        {
            return;
        }

        GameObject canvasObject = new("HotelHungerFadeCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster), typeof(CanvasGroup));
        canvasObject.transform.SetParent(transform, false);

        Canvas canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = FadeSortingOrder;

        CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);

        fadeGroup = canvasObject.GetComponent<CanvasGroup>();
        fadeGroup.alpha = 0f;
        fadeGroup.blocksRaycasts = false;
        fadeGroup.interactable = false;

        GameObject imageObject = new("FadeImage", typeof(RectTransform), typeof(Image));
        imageObject.transform.SetParent(canvasObject.transform, false);

        RectTransform imageRect = imageObject.GetComponent<RectTransform>();
        imageRect.anchorMin = Vector2.zero;
        imageRect.anchorMax = Vector2.one;
        imageRect.offsetMin = Vector2.zero;
        imageRect.offsetMax = Vector2.zero;

        fadeImage = imageObject.GetComponent<Image>();
        fadeImage.color = Color.black;
        fadeImage.raycastTarget = true;
    }

    private void EnsureGameplayCoreUi()
    {
        bool needsPrompt = InteractionPromptUI.Instance == null;
        bool needsObjective = ObjectivePanelController.Instance == null;
        bool needsDialogue = DialoguePanelController.Instance == null;
        if (!needsPrompt && !needsObjective && !needsDialogue)
        {
            return;
        }

        if (runtimeCoreUiRoot == null)
        {
            runtimeCoreUiRoot = new GameObject("RuntimeCoreUIHelpers");
            DontDestroyOnLoad(runtimeCoreUiRoot);
        }

        if (needsPrompt)
        {
            CreateRuntimeCoreUi<InteractionPromptUI>("RuntimeInteractionPromptUI");
        }

        if (needsObjective)
        {
            CreateRuntimeCoreUi<ObjectivePanelController>("RuntimeObjectivePanelController");
        }

        if (needsDialogue)
        {
            CreateRuntimeCoreUi<DialoguePanelController>("RuntimeDialoguePanelController");
        }
    }

    private void CreateRuntimeCoreUi<T>(string objectName) where T : Component
    {
        GameObject helper = new(objectName);
        helper.transform.SetParent(runtimeCoreUiRoot.transform, false);
        helper.AddComponent<T>();
    }

    private void SaveCheckpoint(string sceneName)
    {
        if (!SceneExistsInBuildSettings(sceneName))
        {
            Debug.LogWarning($"HotelHungerRuntimeManager: valid checkpoint scene '{sceneName}' is not in Build Settings.", this);
            return;
        }

        PlayerPrefs.SetString(CheckpointKey, sceneName);
        PlayerPrefs.Save();
        CheckpointChanged?.Invoke();
    }

    private void ApplySavedVolume()
    {
        AudioListener.volume = Mathf.Clamp01(PlayerPrefs.GetFloat(VolumeKey, 1f));
    }

    private static string GetBindingKey(MovementDirection direction)
    {
        return BindingPrefix + direction;
    }

    private static bool IsValidCheckpointScene(string sceneName)
    {
        for (int i = 0; i < ValidCheckpointScenes.Length; i++)
        {
            if (ValidCheckpointScenes[i] == sceneName)
            {
                return true;
            }
        }

        return false;
    }

    private static bool SceneExistsInBuildSettings(string sceneName)
    {
        for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
        {
            string path = SceneUtility.GetScenePathByBuildIndex(i);
            string buildSceneName = Path.GetFileNameWithoutExtension(path);
            if (buildSceneName == sceneName)
            {
                return true;
            }
        }

        return false;
    }
}
