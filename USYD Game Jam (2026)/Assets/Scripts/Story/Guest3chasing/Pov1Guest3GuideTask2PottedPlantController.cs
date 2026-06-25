using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

public class Pov1Guest3GuideTask2PottedPlantController : MonoBehaviour
{
    private enum LayoutChaseState
    {
        RunningUpRoute1,
        RunningLeftRoute1,
        MovingPottedPlant,
        RunningRightRoute2,
        WaitingToReset
    }

    [Header("Scene References")]
    [SerializeField] private Transform guest3;
    [SerializeField] private Transform pottedPlant;
    [SerializeField] private Transform pottedPlantBlockPosition;
    [Tooltip("Guest3 must pass this object's visual top edge before the potted plant can be clicked.")]
    [SerializeField] private Transform clickUnlockWall;
    [SerializeField] private Transform guest3StartPoint;
    [SerializeField] private Transform route2FailPoint;
    [SerializeField] private Transform route1SuccessPoint;

    [Header("Movement")]
    [SerializeField] private float guestMoveSpeed = 2.6f;
    [SerializeField] private float pottedPlantMoveSpeed = 4f;
    [SerializeField] private float arriveDistance = 0.08f;

    [Header("Timing")]
    [SerializeField] private float plantClickPadding = 0.25f;
    [SerializeField] private float resetDelay = 0.8f;

    [Header("Failure Transition")]
    [Tooltip("Scene loaded when Guest3 reaches the left end without a successful plant click.")]
    [SerializeField] private string failureSceneName = "Pov1_Guest3_Guide_Task1_WetFloorSign";

    [Header("Success Transition")]
    [Tooltip("Scene loaded when Guest3 completes this chase after a successful plant click.")]
    [SerializeField] private string successSceneName = "Pov1_Guest3_Guide_Task2_LuggageCart";

    private LayoutChaseState state = LayoutChaseState.RunningUpRoute1;
    private Vector3 guestStartPosition;
    private Vector3 pottedPlantStartPosition;
    private bool hasCachedGuestStartPosition;
    private bool hasCachedPottedPlantStartPosition;
    private bool hasWarnedMissingClickUnlockWall;
    private Coroutine resetRoutine;

    private void Start()
    {
        CacheStartPositions();
        ResetLayoutRound();
    }

    private void Update()
    {
        if (guest3 == null || pottedPlant == null)
        {
            return;
        }

        switch (state)
        {
            case LayoutChaseState.RunningUpRoute1:
                RunGuestUpRoute1();
                break;

            case LayoutChaseState.RunningLeftRoute1:
                RunGuestLeftRoute1();
                break;

            case LayoutChaseState.MovingPottedPlant:
                MovePottedPlantDown();
                break;

            case LayoutChaseState.RunningRightRoute2:
                RunGuestRightRoute2();
                break;
        }
    }

    private void RunGuestUpRoute1()
    {
        MoveGuestToward(GetRoute1TurnPoint());

        if (HasGuestArrived(GetRoute1TurnPoint()))
        {
            state = LayoutChaseState.RunningLeftRoute1;
        }
    }

    private void RunGuestLeftRoute1()
    {
        if (TryClickPlantInTimingWindow())
        {
            state = LayoutChaseState.MovingPottedPlant;
            Debug.Log("Pov1 Guest3 Guide Task2 PottedPlant: plant clicked after Guest3 passed Wall_Left top. Moving plant down.");
            return;
        }

        MoveGuestToward(GetRoute1LeftEndPoint());

        if (HasGuestArrived(GetRoute1LeftEndPoint()))
        {
            Debug.Log($"Pov1 Guest3 Guide Task2 PottedPlant: Guest3 followed Route 1 to the left end. Loading '{failureSceneName}'.");
            LoadFailureScene();
        }
    }

    private void MovePottedPlantDown()
    {
        if (pottedPlantBlockPosition == null)
        {
            Debug.LogWarning("Pov1 Guest3 Guide Task2 PottedPlant: PottedPlant_BlockPosition is missing.", this);
            state = LayoutChaseState.RunningRightRoute2;
            return;
        }

        pottedPlant.position = Vector3.MoveTowards(
            pottedPlant.position,
            pottedPlantBlockPosition.position,
            pottedPlantMoveSpeed * Time.deltaTime);

        if (Vector3.Distance(pottedPlant.position, pottedPlantBlockPosition.position) <= arriveDistance)
        {
            state = LayoutChaseState.RunningRightRoute2;
            Debug.Log("Pov1 Guest3 Guide Task2 PottedPlant: plant dropped. Guest3 switches to Route 2.");
        }
    }

    private void RunGuestRightRoute2()
    {
        MoveGuestToward(GetRoute2RightEndPoint());

        if (HasGuestArrived(GetRoute2RightEndPoint()))
        {
            Debug.Log($"Pov1 Guest3 Guide Task2 PottedPlant: Guest3 completed the success route. Loading '{successSceneName}'.");
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
            Debug.LogWarning("Pov1 Guest3 Guide Task2 PottedPlant: failure scene name is empty.", this);
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
            $"Pov1 Guest3 Guide Task2 PottedPlant: scene '{failureSceneName}' is not in Build Settings. Add it before making a build.",
            this);
    }

    private void LoadSuccessScene()
    {
        state = LayoutChaseState.WaitingToReset;

        if (string.IsNullOrWhiteSpace(successSceneName))
        {
            Debug.LogWarning("Pov1 Guest3 Guide Task2 PottedPlant: success scene name is empty.", this);
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
            $"Pov1 Guest3 Guide Task2 PottedPlant: scene '{successSceneName}' is not in Build Settings. Add it before making a build.",
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

        if (pottedPlant != null)
        {
            pottedPlant.position = pottedPlantStartPosition;
        }

        state = LayoutChaseState.RunningUpRoute1;
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

    private bool TryClickPlantInTimingWindow()
    {
        return HasGuestPassedClickUnlockWallTop() && !HasGuestReachedPlantOnRoute1() && WasPottedPlantClicked();
    }

    private bool HasGuestPassedClickUnlockWallTop()
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
                    "Pov1 Guest3 Guide Task2 PottedPlant: clickUnlockWall is missing. Falling back to allowing plant clicks immediately.",
                    this);
                hasWarnedMissingClickUnlockWall = true;
            }

            return true;
        }

        return guest3.position.y >= GetVisualTopEdgeY(clickUnlockWall);
    }

    private float GetVisualTopEdgeY(Transform target)
    {
        SpriteRenderer renderer = target.GetComponent<SpriteRenderer>();
        if (renderer != null)
        {
            return renderer.bounds.max.y;
        }

        Collider2D collider = target.GetComponent<Collider2D>();
        if (collider != null)
        {
            return collider.bounds.max.y;
        }

        return target.position.y;
    }

    private bool HasGuestReachedPlantOnRoute1()
    {
        return guest3 != null && pottedPlant != null && guest3.position.x <= pottedPlant.position.x;
    }

    private bool WasPottedPlantClicked()
    {
        if (Mouse.current == null || !Mouse.current.leftButton.wasPressedThisFrame)
        {
            return false;
        }

        Camera mainCamera = Camera.main;
        SpriteRenderer plantRenderer = pottedPlant.GetComponent<SpriteRenderer>();
        if (mainCamera == null || plantRenderer == null)
        {
            return false;
        }

        Vector2 mousePosition = Mouse.current.position.ReadValue();
        Vector3 worldPoint = mainCamera.ScreenToWorldPoint(new Vector3(mousePosition.x, mousePosition.y, -mainCamera.transform.position.z));
        Bounds clickBounds = plantRenderer.bounds;
        clickBounds.Expand(plantClickPadding);
        return clickBounds.Contains(new Vector3(worldPoint.x, worldPoint.y, clickBounds.center.z));
    }

    private Vector3 GetRoute1TurnPoint()
    {
        float routeY = route1SuccessPoint != null ? route1SuccessPoint.position.y : pottedPlant.position.y;
        return new Vector3(guestStartPosition.x, routeY, guestStartPosition.z);
    }

    private Vector3 GetRoute1LeftEndPoint()
    {
        float routeY = route1SuccessPoint != null ? route1SuccessPoint.position.y : pottedPlant.position.y;
        float leftX = guest3StartPoint != null ? guest3StartPoint.position.x : pottedPlant.position.x - 2f;
        return new Vector3(leftX, routeY, guestStartPosition.z);
    }

    private Vector3 GetRoute2RightEndPoint()
    {
        float routeY = route1SuccessPoint != null ? route1SuccessPoint.position.y : pottedPlant.position.y;
        float rightX = route2FailPoint != null ? route2FailPoint.position.x : pottedPlant.position.x + 8f;
        return new Vector3(rightX, routeY, guestStartPosition.z);
    }

    private void CacheStartPositions()
    {
        if (guest3 != null && !hasCachedGuestStartPosition)
        {
            guestStartPosition = guest3.position;
            hasCachedGuestStartPosition = true;
        }

        if (pottedPlant != null && !hasCachedPottedPlantStartPosition)
        {
            pottedPlantStartPosition = pottedPlant.position;
            hasCachedPottedPlantStartPosition = true;
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
