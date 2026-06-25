using System.Collections;
using UnityEngine;

public class DiningRoomChaseCutscene : MonoBehaviour
{
    [Header("Player")]
    [SerializeField] private GameObject playerObject;
    [SerializeField] private SidePlayerController playerController;
    [SerializeField] private PlayerInteractor playerInteractor;
    [SerializeField] private Rigidbody2D playerBody;
    [SerializeField] private SpriteRenderer[] playerRenderers;

    [Header("Actors")]
    [SerializeField] private Transform guest2Actor;
    [SerializeField] private Transform staffActor;

    [Header("Routes")]
    [SerializeField] private Transform[] staffEntryWaypoints;
    [SerializeField] private Transform[] guestRunWaypoints;
    [SerializeField] private Transform[] staffChaseWaypoints;

    [Header("Timing")]
    [SerializeField] private float staffEntryDuration = 1.6f;
    [SerializeField] private float surprisePauseDuration = 0.7f;
    [SerializeField] private float staffChaseDelay = 0.45f;
    [SerializeField] private float guestRunDuration = 4.65f;
    [SerializeField] private float staffRunDuration = 4.45f;
    [SerializeField] private float transitionDelay = 0.35f;

    [Header("Transition")]
    [SerializeField] private bool loadNextSceneOnComplete;
    [SerializeField] private string nextSceneName = string.Empty;
    [SerializeField] private string nextSpawnId = string.Empty;

    private bool hasStarted;
    private Coroutine runningRoutine;

    private void Start()
    {
        StartCutscene();
    }

    private void OnDisable()
    {
        if (runningRoutine != null)
        {
            StopCoroutine(runningRoutine);
            runningRoutine = null;
        }
    }

    public void Configure(
        GameObject newPlayerObject,
        SidePlayerController newPlayerController,
        PlayerInteractor newPlayerInteractor,
        Rigidbody2D newPlayerBody,
        SpriteRenderer[] newPlayerRenderers,
        Transform newGuest2Actor,
        Transform newStaffActor,
        Transform[] newStaffEntryWaypoints,
        Transform[] newGuestRunWaypoints,
        Transform[] newStaffChaseWaypoints,
        bool newLoadNextSceneOnComplete,
        string newNextSceneName,
        string newNextSpawnId)
    {
        playerObject = newPlayerObject;
        playerController = newPlayerController;
        playerInteractor = newPlayerInteractor;
        playerBody = newPlayerBody;
        playerRenderers = newPlayerRenderers;
        guest2Actor = newGuest2Actor;
        staffActor = newStaffActor;
        staffEntryWaypoints = newStaffEntryWaypoints;
        guestRunWaypoints = newGuestRunWaypoints;
        staffChaseWaypoints = newStaffChaseWaypoints;
        loadNextSceneOnComplete = newLoadNextSceneOnComplete;
        nextSceneName = newNextSceneName ?? string.Empty;
        nextSpawnId = newNextSpawnId ?? string.Empty;
    }

    public void StartCutscene()
    {
        if (hasStarted || !isActiveAndEnabled)
        {
            return;
        }

        hasStarted = true;
        runningRoutine = StartCoroutine(PlayCutsceneRoutine());
    }

    private IEnumerator PlayCutsceneRoutine()
    {
        HideAndDisablePlayer();

        if (!HasRequiredReferences())
        {
            Debug.LogWarning("DiningRoomChaseCutscene is missing actor or waypoint references.", this);
            runningRoutine = null;
            yield break;
        }

        SnapToFirstWaypoint(staffActor, staffEntryWaypoints);
        SnapToFirstWaypoint(guest2Actor, guestRunWaypoints);
        SetActorActive(staffActor, true);
        SetActorActive(guest2Actor, true);

        yield return MoveAlong(staffActor, staffEntryWaypoints, staffEntryDuration);
        yield return WaitForSecondsSafe(surprisePauseDuration);

        Coroutine guestRoutine = StartCoroutine(MoveAlongThenDeactivate(guest2Actor, guestRunWaypoints, guestRunDuration));
        yield return WaitForSecondsSafe(staffChaseDelay);
        Coroutine staffRoutine = StartCoroutine(MoveAlongThenDeactivate(staffActor, staffChaseWaypoints, staffRunDuration));

        yield return guestRoutine;
        yield return staffRoutine;
        yield return WaitForSecondsSafe(transitionDelay);

        TryLoadNextScene();
        runningRoutine = null;
    }

    private void HideAndDisablePlayer()
    {
        if (playerObject == null &&
            playerController == null &&
            playerInteractor == null &&
            playerBody == null &&
            (playerRenderers == null || playerRenderers.Length == 0))
        {
            return;
        }

        if (playerController != null)
        {
            playerController.enabled = false;
        }

        if (playerInteractor != null)
        {
            playerInteractor.enabled = false;
        }

        if (playerRenderers != null)
        {
            for (int i = 0; i < playerRenderers.Length; i++)
            {
                if (playerRenderers[i] != null)
                {
                    playerRenderers[i].enabled = false;
                }
            }
        }

        if (playerBody != null)
        {
            playerBody.linearVelocity = Vector2.zero;
            playerBody.angularVelocity = 0f;
            playerBody.simulated = false;
        }
    }

    private void TryLoadNextScene()
    {
        if (!loadNextSceneOnComplete)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(nextSceneName) || string.IsNullOrWhiteSpace(nextSpawnId))
        {
            Debug.LogWarning(
                "DiningRoomChaseCutscene was configured to load the next scene, but the scene name or spawn id is empty.",
                this);
            return;
        }

        SceneTransitionManager.Instance?.LoadScene(nextSceneName, nextSpawnId);
    }

    private bool HasRequiredReferences()
    {
        return guest2Actor != null &&
            staffActor != null &&
            HasEnoughWaypoints(staffEntryWaypoints) &&
            HasEnoughWaypoints(guestRunWaypoints) &&
            HasEnoughWaypoints(staffChaseWaypoints);
    }

    private static bool HasEnoughWaypoints(Transform[] waypoints)
    {
        int validCount = 0;
        if (waypoints == null)
        {
            return false;
        }

        for (int i = 0; i < waypoints.Length; i++)
        {
            if (waypoints[i] != null)
            {
                validCount++;
            }
        }

        return validCount >= 2;
    }

    private static void SnapToFirstWaypoint(Transform actor, Transform[] waypoints)
    {
        if (actor == null || waypoints == null)
        {
            return;
        }

        for (int i = 0; i < waypoints.Length; i++)
        {
            if (waypoints[i] != null)
            {
                actor.position = waypoints[i].position;
                return;
            }
        }
    }

    private static void SetActorActive(Transform actor, bool isActive)
    {
        if (actor != null)
        {
            actor.gameObject.SetActive(isActive);
        }
    }

    private static IEnumerator MoveAlongThenDeactivate(Transform actor, Transform[] waypoints, float duration)
    {
        yield return MoveAlong(actor, waypoints, duration);
        SetActorActive(actor, false);
    }

    private static IEnumerator MoveAlong(Transform actor, Transform[] waypoints, float duration)
    {
        Transform[] validWaypoints = GetValidWaypoints(waypoints);
        if (actor == null || validWaypoints.Length < 2)
        {
            yield break;
        }

        float totalDistance = GetTotalDistance(validWaypoints);
        if (Mathf.Approximately(totalDistance, 0f) || duration <= 0f)
        {
            actor.position = validWaypoints[validWaypoints.Length - 1].position;
            yield break;
        }

        actor.position = validWaypoints[0].position;

        for (int i = 1; i < validWaypoints.Length; i++)
        {
            Vector3 start = validWaypoints[i - 1].position;
            Vector3 end = validWaypoints[i].position;
            float segmentDistance = Vector3.Distance(start, end);
            if (Mathf.Approximately(segmentDistance, 0f))
            {
                actor.position = end;
                continue;
            }

            FaceTravelDirection(actor, end.x - start.x);

            float segmentDuration = duration * (segmentDistance / totalDistance);
            float elapsed = 0f;
            while (elapsed < segmentDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / segmentDuration);
                float smoothedT = Mathf.SmoothStep(0f, 1f, t);
                actor.position = Vector3.Lerp(start, end, smoothedT);
                yield return null;
            }

            actor.position = end;
        }
    }

    private static Transform[] GetValidWaypoints(Transform[] waypoints)
    {
        if (waypoints == null || waypoints.Length == 0)
        {
            return new Transform[0];
        }

        int validCount = 0;
        for (int i = 0; i < waypoints.Length; i++)
        {
            if (waypoints[i] != null)
            {
                validCount++;
            }
        }

        Transform[] validWaypoints = new Transform[validCount];
        int writeIndex = 0;
        for (int i = 0; i < waypoints.Length; i++)
        {
            if (waypoints[i] != null)
            {
                validWaypoints[writeIndex] = waypoints[i];
                writeIndex++;
            }
        }

        return validWaypoints;
    }

    private static float GetTotalDistance(Transform[] waypoints)
    {
        float totalDistance = 0f;
        for (int i = 1; i < waypoints.Length; i++)
        {
            totalDistance += Vector3.Distance(waypoints[i - 1].position, waypoints[i].position);
        }

        return totalDistance;
    }

    private static void FaceTravelDirection(Transform actor, float deltaX)
    {
        if (Mathf.Abs(deltaX) < 0.01f)
        {
            return;
        }

        Vector3 scale = actor.localScale;
        scale.x = Mathf.Abs(scale.x) * Mathf.Sign(deltaX);
        actor.localScale = scale;
    }

    private static IEnumerator WaitForSecondsSafe(float duration)
    {
        if (duration > 0f)
        {
            yield return new WaitForSeconds(duration);
        }
    }
}
