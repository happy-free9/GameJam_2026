using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

public class Pov1Guest3GuideTask2LuggageCartController : MonoBehaviour
{
    private enum LayoutChaseState
    {
        RunningRightRoute1,
        MovingLuggageCart,
        RunningUpRoute2,
        WaitingToReset
    }

    [Header("Scene References")]
    [SerializeField] private Transform guest3;
    [SerializeField] private Transform luggageCart;
    [SerializeField] private Transform luggageCartBlockPosition;
    [Tooltip("Guest3 must pass this object's visual right edge before the luggage cart can be clicked.")]
    [SerializeField] private Transform clickUnlockWall;
    [SerializeField] private Transform guest3StartPoint;
    [SerializeField] private Transform route2FailPoint;
    [SerializeField] private Transform route1SuccessPoint;

    [Header("Movement")]
    [SerializeField] private float guestMoveSpeed = 2.6f;
    [SerializeField] private float luggageCartMoveSpeed = 4f;
    [SerializeField] private float arriveDistance = 0.08f;

    [Header("Timing")]
    [SerializeField] private float luggageCartClickPadding = 0.25f;
    [SerializeField] private float resetDelay = 0.8f;

    [Header("Failure Transition")]
    [Tooltip("Scene loaded when Guest3 reaches the right end without a successful luggage cart click.")]
    [SerializeField] private string failureSceneName = "Pov1_Guest3_Guide_Task1_WetFloorSign";

    [Header("Success Transition")]
    [Tooltip("Scene loaded when Guest3 reaches the upper route after a successful luggage cart click.")]
    [SerializeField] private string successSceneName = "Guest3_Final_Chasing";

    private LayoutChaseState state = LayoutChaseState.RunningRightRoute1;
    private Vector3 guestStartPosition;
    private Vector3 luggageCartStartPosition;
    private Vector3 route2UpEndPosition;
    private bool hasCachedGuestStartPosition;
    private bool hasCachedLuggageCartStartPosition;
    private bool hasRoute2UpEndPosition;
    private bool hasWarnedMissingClickUnlockWall;
    private Coroutine resetRoutine;

    private void Start()
    {
        CacheStartPositions();
        ResetLayoutRound();
    }

    private void Update()
    {
        if (guest3 == null || luggageCart == null)
        {
            return;
        }

        switch (state)
        {
            case LayoutChaseState.RunningRightRoute1:
                RunGuestRightRoute1();
                break;

            case LayoutChaseState.MovingLuggageCart:
                MoveLuggageCartDown();
                break;

            case LayoutChaseState.RunningUpRoute2:
                RunGuestUpRoute2();
                break;
        }
    }

    private void RunGuestRightRoute1()
    {
        if (TryClickLuggageCartInTimingWindow())
        {
            state = LayoutChaseState.MovingLuggageCart;
            Debug.Log("Pov1 Guest3 Guide Task2 LuggageCart: cart clicked after Guest3 passed Wall_Left. Moving cart down.");
            return;
        }

        MoveGuestToward(GetRoute1RightEndPoint());

        if (HasGuestArrived(GetRoute1RightEndPoint()))
        {
            Debug.Log($"Pov1 Guest3 Guide Task2 LuggageCart: Guest3 reached the right end without cart click. Loading '{failureSceneName}'.");
            LoadFailureScene();
        }
    }

    private void MoveLuggageCartDown()
    {
        if (luggageCartBlockPosition == null)
        {
            Debug.LogWarning("Pov1 Guest3 Guide Task2 LuggageCart: LuggageCart_BlockPosition is missing.", this);
            state = LayoutChaseState.RunningUpRoute2;
            return;
        }

        luggageCart.position = Vector3.MoveTowards(
            luggageCart.position,
            luggageCartBlockPosition.position,
            luggageCartMoveSpeed * Time.deltaTime);

        if (Vector3.Distance(luggageCart.position, luggageCartBlockPosition.position) <= arriveDistance)
        {
            route2UpEndPosition = GetRoute2UpEndPoint();
            hasRoute2UpEndPosition = true;
            state = LayoutChaseState.RunningUpRoute2;
            Debug.Log("Pov1 Guest3 Guide Task2 LuggageCart: cart moved. Guest3 switches upward.");
        }
    }

    private void RunGuestUpRoute2()
    {
        if (!hasRoute2UpEndPosition)
        {
            route2UpEndPosition = GetRoute2UpEndPoint();
            hasRoute2UpEndPosition = true;
        }

        MoveGuestToward(route2UpEndPosition);

        if (HasGuestArrived(route2UpEndPosition))
        {
            Debug.Log($"Pov1 Guest3 Guide Task2 LuggageCart: Guest3 followed Route 2 upward to the end. Loading '{successSceneName}'.");
            LoadSuccessScene();
        }
    }

    private void HideGuestAndReset()
    {
        if (guest3 != null)
        {
            guest3.gameObject.SetActive(false);
        }

        state = LayoutChaseState.WaitingToReset;

        if (resetRoutine != null)
        {
            StopCoroutine(resetRoutine);
        }

        resetRoutine = StartCoroutine(ResetAfterDelay());
    }

    private IEnumerator ResetAfterDelay()
    {
        yield return new WaitForSeconds(resetDelay);
        ResetLayoutRound();
    }

    private void LoadFailureScene()
    {
        state = LayoutChaseState.WaitingToReset;

        if (string.IsNullOrWhiteSpace(failureSceneName))
        {
            Debug.LogWarning("Pov1 Guest3 Guide Task2 LuggageCart: failure scene name is empty.", this);
            return;
        }

        if (SceneExistsInBuildSettings(failureSceneName))
        {
            SceneTransitionManager.Instance.LoadScene(failureSceneName, string.Empty);
            return;
        }

#if UNITY_EDITOR
        string scenePath = $"Assets/Scenes/{failureSceneName}.unity";
        if (AssetDatabase.LoadAssetAtPath<SceneAsset>(scenePath) != null)
        {
            EditorSceneManager.LoadSceneInPlayMode(scenePath, new LoadSceneParameters(LoadSceneMode.Single));
            return;
        }
#endif

        Debug.LogWarning(
            $"Pov1 Guest3 Guide Task2 LuggageCart: scene '{failureSceneName}' is not in Build Settings. Add it before making a build.",
            this);
    }

    private void LoadSuccessScene()
    {
        state = LayoutChaseState.WaitingToReset;

        if (string.IsNullOrWhiteSpace(successSceneName))
        {
            Debug.LogWarning("Pov1 Guest3 Guide Task2 LuggageCart: success scene name is empty.", this);
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
            $"Pov1 Guest3 Guide Task2 LuggageCart: scene '{successSceneName}' is not in Build Settings. Add it before making a build.",
            this);
    }

    private void ResetLayoutRound()
    {
        CacheStartPositions();

        if (guest3 != null)
        {
            guest3.position = guestStartPosition;
            guest3.gameObject.SetActive(true);
        }

        if (luggageCart != null)
        {
            luggageCart.position = luggageCartStartPosition;
        }

        state = LayoutChaseState.RunningRightRoute1;
        hasRoute2UpEndPosition = false;
        resetRoutine = null;
    }

    private void MoveGuestToward(Vector3 target)
    {
        if (guest3 == null)
        {
            return;
        }

        guest3.position = Vector3.MoveTowards(
            guest3.position,
            target,
            guestMoveSpeed * Time.deltaTime);
    }

    private bool HasGuestArrived(Vector3 target)
    {
        return guest3 != null
            && Vector3.Distance(guest3.position, target) <= arriveDistance;
    }

    private bool TryClickLuggageCartInTimingWindow()
    {
        return HasGuestPassedClickUnlockWall() && !HasGuestPassedLuggageCartOnRoute1() && WasLuggageCartClicked();
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
                    "Pov1 Guest3 Guide Task2 LuggageCart: clickUnlockWall is missing. Falling back to allowing cart clicks immediately.",
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

    private bool HasGuestPassedLuggageCartOnRoute1()
    {
        return guest3 != null && luggageCart != null && guest3.position.x >= luggageCart.position.x;
    }

    private bool WasLuggageCartClicked()
    {
        if (Mouse.current == null || !Mouse.current.leftButton.wasPressedThisFrame)
        {
            return false;
        }

        Camera mainCamera = Camera.main;
        SpriteRenderer cartRenderer = luggageCart.GetComponent<SpriteRenderer>();
        if (mainCamera == null || cartRenderer == null)
        {
            return false;
        }

        Vector2 mousePosition = Mouse.current.position.ReadValue();
        Vector3 worldPoint = mainCamera.ScreenToWorldPoint(new Vector3(mousePosition.x, mousePosition.y, -mainCamera.transform.position.z));
        Bounds clickBounds = cartRenderer.bounds;
        clickBounds.Expand(luggageCartClickPadding);
        return clickBounds.Contains(new Vector3(worldPoint.x, worldPoint.y, clickBounds.center.z));
    }

    private Vector3 GetRoute1RightEndPoint()
    {
        float rightX = route2FailPoint != null ? route2FailPoint.position.x : luggageCart.position.x + 3f;
        return new Vector3(rightX, guestStartPosition.y, guestStartPosition.z);
    }

    private Vector3 GetRoute2UpEndPoint()
    {
        float topY = route1SuccessPoint != null ? route1SuccessPoint.position.y : guestStartPosition.y + 2f;

        if (hasCachedLuggageCartStartPosition)
        {
            topY = Mathf.Max(topY, luggageCartStartPosition.y + 0.5f);
        }

        topY = Mathf.Max(topY, guest3.position.y + 1f);
        return new Vector3(guest3.position.x, topY, guestStartPosition.z);
    }

    private void CacheStartPositions()
    {
        if (guest3 != null && !hasCachedGuestStartPosition)
        {
            guestStartPosition = guest3.position;
            hasCachedGuestStartPosition = true;
        }

        if (luggageCart != null && !hasCachedLuggageCartStartPosition)
        {
            luggageCartStartPosition = luggageCart.position;
            hasCachedLuggageCartStartPosition = true;
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
