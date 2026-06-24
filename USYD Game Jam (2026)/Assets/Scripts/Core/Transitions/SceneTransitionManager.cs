using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneTransitionManager : MonoBehaviour
{
    private static SceneTransitionManager instance;

    public static SceneTransitionManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindFirstObjectByType<SceneTransitionManager>();
            }

            if (instance == null)
            {
                instance = new GameObject("SceneTransitionManager").AddComponent<SceneTransitionManager>();
            }

            return instance;
        }
        private set => instance = value;
    }

    private static string pendingSpawnPointId;

    [Header("Debug")]
    [Tooltip("If enabled, logs extra scene transition details to help with jam-time debugging.")]
    [SerializeField] private bool verboseLogging;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    public void LoadScene(string targetSceneName, string targetSpawnPointId)
    {
        if (string.IsNullOrWhiteSpace(targetSceneName))
        {
            Debug.LogWarning("SceneTransitionManager: targetSceneName is empty.");
            return;
        }

        if (!SceneExistsInBuildSettings(targetSceneName))
        {
            Debug.LogWarning($"SceneTransitionManager: scene '{targetSceneName}' is not in Build Settings.");
            return;
        }

        pendingSpawnPointId = targetSpawnPointId;

        if (verboseLogging)
        {
            Debug.Log($"SceneTransitionManager: loading '{targetSceneName}' with spawn id '{targetSpawnPointId}'.", this);
        }

        StartCoroutine(LoadSceneRoutine(targetSceneName));
    }

    private IEnumerator LoadSceneRoutine(string targetSceneName)
    {
        AsyncOperation loadOperation = SceneManager.LoadSceneAsync(targetSceneName);
        if (loadOperation == null)
        {
            Debug.LogWarning($"SceneTransitionManager: failed to start loading scene '{targetSceneName}'.");
            yield break;
        }

        while (!loadOperation.isDone)
        {
            yield return null;
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (string.IsNullOrWhiteSpace(pendingSpawnPointId))
        {
            return;
        }

        // Wait one frame so the newly loaded scene can finish creating its player and spawn objects.
        StartCoroutine(ApplyPendingSpawnRoutine(scene));
    }

    private IEnumerator ApplyPendingSpawnRoutine(Scene loadedScene)
    {
        yield return null;

        SpawnPoint[] spawnPoints = FindObjectsByType<SpawnPoint>(FindObjectsSortMode.None);
        PlayerInteractor player = FindFirstObjectByType<PlayerInteractor>();

        if (player == null)
        {
            Debug.LogWarning($"SceneTransitionManager: no PlayerInteractor found in scene '{loadedScene.name}'.");
            pendingSpawnPointId = null;
            yield break;
        }

        for (int i = 0; i < spawnPoints.Length; i++)
        {
            SpawnPoint spawnPoint = spawnPoints[i];
            if (spawnPoint == null || spawnPoint.gameObject.scene != loadedScene)
            {
                continue;
            }

            if (spawnPoint.SpawnPointId != pendingSpawnPointId)
            {
                continue;
            }

            player.transform.position = spawnPoint.transform.position;

            Rigidbody2D playerBody = player.GetComponent<Rigidbody2D>();
            if (playerBody != null)
            {
                playerBody.linearVelocity = Vector2.zero;
            }

            if (verboseLogging)
            {
                Debug.Log(
                    $"SceneTransitionManager: moved player to spawn point '{pendingSpawnPointId}' in scene '{loadedScene.name}'.",
                    this);
            }

            pendingSpawnPointId = null;
            yield break;
        }

        Debug.LogWarning(
            $"SceneTransitionManager: no SpawnPoint with id '{pendingSpawnPointId}' was found in scene '{loadedScene.name}'.");
        pendingSpawnPointId = null;
    }

    private bool SceneExistsInBuildSettings(string targetSceneName)
    {
        for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
        {
            string path = SceneUtility.GetScenePathByBuildIndex(i);
            string sceneName = Path.GetFileNameWithoutExtension(path);
            if (sceneName == targetSceneName)
            {
                return true;
            }
        }

        return false;
    }
}
