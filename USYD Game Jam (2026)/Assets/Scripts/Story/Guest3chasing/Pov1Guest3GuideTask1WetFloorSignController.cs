using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

public class Pov1Guest3GuideTask1WetFloorSignController : MonoBehaviour
{
    private enum OriginalRouteState
    {
        RunningRight,
        MovingWetFloorSign,
        RunningUp,
        Complete,
        Resetting
    }

    [Header("Scene References")]
    [SerializeField] private Transform guest3;
    [SerializeField] private Transform wetFloorSign;
    [SerializeField] private Transform wetFloorSignBlockPosition;
    [Tooltip("Guest3 must pass this object's visual right edge before the wet-floor sign can be clicked.")]
    [SerializeField] private Transform clickUnlockWall;
    [SerializeField] private Transform route2FailPoint;
    [SerializeField] private Transform route1SuccessPoint;

    [Header("Movement")]
    [SerializeField] private float guestMoveSpeed = 2.6f;
    [SerializeField] private float wetFloorSignMoveSpeed = 4f;
    [SerializeField] private float arriveDistance = 0.08f;

    [Header("Click Interaction")]
    [Tooltip("Extra clickable padding around the wet-floor sign placeholder sprite.")]
    [SerializeField] private float wetFloorSignClickPadding = 0.25f;
    [SerializeField] private float resetDelay = 0.8f;

    [Header("Success Transition")]
    [Tooltip("Scene loaded after Guest3 reaches the upper route end.")]
    [SerializeField] private string successSceneName = "Pov1_Guest3_Guide_Task2_PottedPlant";
    [Tooltip("Small pause before loading the next scene, so the success movement does not feel like a flash cut.")]
    [SerializeField] private float successTransitionDelay = 0.75f;

    private OriginalRouteState state = OriginalRouteState.RunningRight;
    private Vector3 guestStartPosition;
    private Vector3 wetFloorSignStartPosition;
    private bool hasCachedGuestStartPosition;
    private bool hasCachedWetFloorSignStartPosition;
    private bool hasWarnedMissingClickUnlockWall;
    private Coroutine resetRoutine;

    private void Start()
    {
        CacheStartPositions();
        ResetRound();
    }

    private void Update()
    {
        if (guest3 == null || wetFloorSign == null)
        {
            return;
        }

        switch (state)
        {
            case OriginalRouteState.RunningRight:
                RunGuestRight();
                break;

            case OriginalRouteState.MovingWetFloorSign:
                MoveWetFloorSignDown();
                break;

            case OriginalRouteState.RunningUp:
                RunGuestUp();
                break;
        }
    }

    private void RunGuestRight()
    {
        if (TryClickWetFloorSignInTimingWindow())
        {
            state = OriginalRouteState.MovingWetFloorSign;
            Debug.Log("Pov1 Guest3 Guide Task1: wet-floor sign clicked after Guest3 passed Wall_Left. Moving sign down.");
            return;
        }

        MoveGuestToward(GetRoute2EndPoint());

        if (HasGuestArrived(GetRoute2EndPoint()))
        {
            Debug.Log("Pov1 Guest3 Guide Task1: Guest3 reached the right end without sign click. Resetting to start.");
            HideGuestAndReset();
        }
    }

    private void MoveWetFloorSignDown()
    {
        if (wetFloorSignBlockPosition == null)
        {
            Debug.LogWarning("Pov1 Guest3 Guide Task1: WetFloorSign_BlockPosition is missing.", this);
            state = OriginalRouteState.RunningUp;
            return;
        }

        wetFloorSign.position = Vector3.MoveTowards(
            wetFloorSign.position,
            wetFloorSignBlockPosition.position,
            wetFloorSignMoveSpeed * Time.deltaTime);

        if (Vector3.Distance(wetFloorSign.position, wetFloorSignBlockPosition.position) <= arriveDistance)
        {
            state = OriginalRouteState.RunningUp;
            Debug.Log("Pov1 Guest3 Guide Task1: wet-floor sign dropped. Guest3 switches upward.");
        }
    }

    private void RunGuestUp()
    {
        MoveGuestToward(GetRoute1EndPoint());

        if (HasGuestArrived(GetRoute1EndPoint()))
        {
            state = OriginalRouteState.Complete;
            Debug.Log($"Pov1 Guest3 Guide Task1: Guest3 reached the upper route end. Loading '{successSceneName}' soon.");
            StartCoroutine(LoadSuccessSceneAfterDelay());
        }
    }

    private void HideGuestAndReset()
    {
        if (guest3 != null)
        {
            guest3.gameObject.SetActive(false);
        }

        state = OriginalRouteState.Resetting;

        if (resetRoutine != null)
        {
            StopCoroutine(resetRoutine);
        }

        resetRoutine = StartCoroutine(ResetAfterDelay());
    }

    private IEnumerator ResetAfterDelay()
    {
        yield return new WaitForSeconds(resetDelay);
        ResetRound();
    }

    private IEnumerator LoadSuccessSceneAfterDelay()
    {
        if (successTransitionDelay > 0f)
        {
            yield return new WaitForSeconds(successTransitionDelay);
        }

        LoadSuccessScene();
    }

    private void LoadSuccessScene()
    {
        if (string.IsNullOrWhiteSpace(successSceneName))
        {
            Debug.LogWarning("Pov1 Guest3 Guide Task1: success scene name is empty.", this);
            return;
        }

        if (SceneExistsInBuildSettings(successSceneName))
        {
            SceneTransitionManager.Instance.LoadScene(successSceneName, string.Empty);
            return;
        }

#if UNITY_EDITOR
        string scenePath = $"Assets/Scenes/{successSceneName}.unity";
        if (AssetDatabase.LoadAssetAtPath<SceneAsset>(scenePath) != null)
        {
            EditorSceneManager.LoadSceneInPlayMode(scenePath, new LoadSceneParameters(LoadSceneMode.Single));
            return;
        }
#endif

        Debug.LogWarning(
            $"Pov1 Guest3 Guide Task1: scene '{successSceneName}' is not in Build Settings. Add it before making a build.",
            this);
    }

    private void ResetRound()
    {
        CacheStartPositions();

        if (guest3 != null)
        {
            guest3.position = guestStartPosition;
            guest3.gameObject.SetActive(true);
        }

        if (wetFloorSign != null)
        {
            wetFloorSign.position = wetFloorSignStartPosition;
        }

        state = OriginalRouteState.RunningRight;
        resetRoutine = null;
    }

    private void MoveGuestToward(Vector3 target)
    {
        guest3.position = Vector3.MoveTowards(
            guest3.position,
            target,
            guestMoveSpeed * Time.deltaTime);
    }

    private bool HasGuestArrived(Vector3 target)
    {
        return Vector3.Distance(guest3.position, target) <= arriveDistance;
    }

    private bool TryClickWetFloorSignInTimingWindow()
    {
        return HasGuestPassedClickUnlockWall() && !HasGuestPassedWetFloorSign() && WasWetFloorSignClicked();
    }

    private bool HasGuestPassedClickUnlockWall()
    {
        if (guest3 == null)
        {
            return false;
        }

        if (clickUnlockWall == null)
        {
            if (!hasWarnedMissingClickUnlockWall)
            {
                Debug.LogWarning(
                    "Pov1 Guest3 Guide Task1: clickUnlockWall is missing. Falling back to allowing wet-floor sign clicks immediately.",
                    this);
                hasWarnedMissingClickUnlockWall = true;
            }

            return true;
        }

        return guest3.position.x >= GetVisualRightEdgeX(clickUnlockWall);
    }

    private float GetVisualRightEdgeX(Transform target)
    {
        SpriteRenderer renderer = target.GetComponent<SpriteRenderer>();
        if (renderer != null)
        {
            return renderer.bounds.max.x;
        }

        Collider2D collider = target.GetComponent<Collider2D>();
        if (collider != null)
        {
            return collider.bounds.max.x;
        }

        return target.position.x;
    }

    private bool HasGuestPassedWetFloorSign()
    {
        return guest3 != null && wetFloorSign != null && guest3.position.x >= wetFloorSign.position.x;
    }

    private bool WasWetFloorSignClicked()
    {
        if (Mouse.current == null || !Mouse.current.leftButton.wasPressedThisFrame)
        {
            return false;
        }

        Camera mainCamera = Camera.main;
        SpriteRenderer signRenderer = wetFloorSign.GetComponent<SpriteRenderer>();
        if (mainCamera == null || signRenderer == null)
        {
            return false;
        }

        Vector2 mousePosition = Mouse.current.position.ReadValue();
        Vector3 worldPoint = mainCamera.ScreenToWorldPoint(new Vector3(mousePosition.x, mousePosition.y, -mainCamera.transform.position.z));
        Bounds clickBounds = signRenderer.bounds;
        clickBounds.Expand(wetFloorSignClickPadding);
        return clickBounds.Contains(new Vector3(worldPoint.x, worldPoint.y, clickBounds.center.z));
    }

    private Vector3 GetRoute2EndPoint()
    {
        if (route2FailPoint != null)
        {
            return new Vector3(route2FailPoint.position.x, guestStartPosition.y, guestStartPosition.z);
        }

        return new Vector3(wetFloorSign.position.x + 5f, guestStartPosition.y, guestStartPosition.z);
    }

    private Vector3 GetRoute1EndPoint()
    {
        if (route1SuccessPoint != null)
        {
            return new Vector3(guest3.position.x, route1SuccessPoint.position.y, guestStartPosition.z);
        }

        return new Vector3(guest3.position.x, wetFloorSign.position.y + 5f, guestStartPosition.z);
    }

    private void CacheStartPositions()
    {
        if (guest3 != null && !hasCachedGuestStartPosition)
        {
            guestStartPosition = guest3.position;
            hasCachedGuestStartPosition = true;
        }

        if (wetFloorSign != null && !hasCachedWetFloorSignStartPosition)
        {
            wetFloorSignStartPosition = wetFloorSign.position;
            hasCachedWetFloorSignStartPosition = true;
        }
    }

    private bool SceneExistsInBuildSettings(string sceneName)
    {
        for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
        {
            string scenePath = SceneUtility.GetScenePathByBuildIndex(i);
            if (Path.GetFileNameWithoutExtension(scenePath) == sceneName)
            {
                return true;
            }
        }

        return false;
    }
}
