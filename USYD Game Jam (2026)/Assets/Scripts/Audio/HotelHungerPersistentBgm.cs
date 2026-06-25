using System;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

public sealed class HotelHungerPersistentBgm : MonoBehaviour
{
    private const string GameObjectName = "HotelHungerPersistentBGM";
    private const string StreamingAudioRelativePath = "Audio/HotelHunger_BGM.ogg";
    private const float DefaultLocalVolume = 0.55f;

    private static HotelHungerPersistentBgm instance;

    [SerializeField, Range(0f, 1f)] private float localVolume = DefaultLocalVolume;

    private AudioSource audioSource;
    private Coroutine loadRoutine;

    public bool IsPlaying => audioSource != null && audioSource.isPlaying;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Bootstrap()
    {
        HotelHungerPersistentBgm existing = FindOrKeepSingleInstance();
        if (existing != null)
        {
            existing.EnsureConfigured();
            existing.Play();
            return;
        }

        GameObject existingObject = GameObject.Find(GameObjectName);
        if (existingObject != null)
        {
            HotelHungerPersistentBgm existingComponent = existingObject.GetComponent<HotelHungerPersistentBgm>();
            if (existingComponent == null)
            {
                existingComponent = existingObject.AddComponent<HotelHungerPersistentBgm>();
            }

            existingComponent.EnsureConfigured();
            existingComponent.Play();
            return;
        }

        GameObject bgmObject = new GameObject(GameObjectName);
        bgmObject.AddComponent<HotelHungerPersistentBgm>();
    }

    private static HotelHungerPersistentBgm FindOrKeepSingleInstance()
    {
        HotelHungerPersistentBgm[] players = FindObjectsByType<HotelHungerPersistentBgm>(
            FindObjectsInactive.Include,
            FindObjectsSortMode.None);

        HotelHungerPersistentBgm keeper = null;
        for (int i = 0; i < players.Length; i++)
        {
            HotelHungerPersistentBgm candidate = players[i];
            if (candidate == null)
            {
                continue;
            }

            if (keeper == null)
            {
                keeper = candidate;
                continue;
            }

            Destroy(candidate.gameObject);
        }

        if (keeper != null)
        {
            instance = keeper;
        }

        return keeper;
    }

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        EnsureConfigured();
        DontDestroyOnLoad(gameObject);
        Play();
    }

    private void OnDestroy()
    {
        if (instance == this)
        {
            instance = null;
        }
    }

    public void Play()
    {
        EnsureConfigured();

        if (audioSource.clip != null)
        {
            if (!audioSource.isPlaying)
            {
                audioSource.Play();
            }

            return;
        }

        if (loadRoutine == null)
        {
            loadRoutine = StartCoroutine(LoadAndPlay());
        }
    }

    public void Stop()
    {
        if (audioSource != null)
        {
            audioSource.Stop();
        }
    }

    public void SetLocalVolume(float volume)
    {
        localVolume = Mathf.Clamp01(volume);
        if (audioSource != null)
        {
            audioSource.volume = localVolume;
        }
    }

    private void EnsureConfigured()
    {
        if (gameObject.name != GameObjectName)
        {
            gameObject.name = GameObjectName;
        }

        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }
        }

        audioSource.loop = true;
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 0f;
        audioSource.volume = localVolume;
    }

    private IEnumerator LoadAndPlay()
    {
        string audioPath = GetStreamingAudioPath();
        if (!IsUriPath(audioPath) && !File.Exists(audioPath))
        {
            Debug.LogWarning($"Hotel Hunger BGM file is missing: {audioPath}", this);
            loadRoutine = null;
            yield break;
        }

        string audioUri = ToUnityWebRequestUri(audioPath);
        using UnityWebRequest request = UnityWebRequestMultimedia.GetAudioClip(audioUri, GetAudioType(audioPath));
        if (request.downloadHandler is DownloadHandlerAudioClip audioHandler)
        {
            audioHandler.streamAudio = false;
        }

        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogWarning($"Hotel Hunger BGM failed to load from '{audioUri}': {request.error}", this);
            loadRoutine = null;
            yield break;
        }

        AudioClip clip = DownloadHandlerAudioClip.GetContent(request);
        if (clip == null)
        {
            Debug.LogWarning($"Hotel Hunger BGM loaded no AudioClip from '{audioUri}'.", this);
            loadRoutine = null;
            yield break;
        }

        clip.name = "HotelHunger_BGM";
        audioSource.clip = clip;
        audioSource.Play();
        loadRoutine = null;
    }

    private static string GetStreamingAudioPath()
    {
        return Path.Combine(Application.streamingAssetsPath, StreamingAudioRelativePath);
    }

    private static string ToUnityWebRequestUri(string path)
    {
        string normalizedPath = path.Replace('\\', '/');
        if (IsUriPath(normalizedPath))
        {
            return normalizedPath;
        }

        return new Uri(normalizedPath).AbsoluteUri;
    }

    private static bool IsUriPath(string path)
    {
        return path.Contains("://");
    }

    private static AudioType GetAudioType(string path)
    {
        string extension = Path.GetExtension(path).ToLowerInvariant();
        return extension switch
        {
            ".ogg" => AudioType.OGGVORBIS,
            ".wav" => AudioType.WAV,
            ".mp3" => AudioType.MPEG,
            _ => AudioType.UNKNOWN
        };
    }
}
