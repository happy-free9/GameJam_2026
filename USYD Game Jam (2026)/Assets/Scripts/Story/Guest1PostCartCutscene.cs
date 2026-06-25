using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public sealed class Guest1PostCartCutscene : MonoBehaviour
{
    [Header("Player")]
    [SerializeField] private GameObject playerObject;
    [SerializeField] private SidePlayerController playerController;
    [SerializeField] private PlayerInteractor playerInteractor;
    [SerializeField] private Rigidbody2D playerBody;

    [Header("Route")]
    [SerializeField] private Transform conciergeTransform;
    [SerializeField] private Transform elevatorTarget;
    [SerializeField] private float elevatorWalkDuration = 1.1f;

    [Header("Next Scene")]
    [SerializeField] private string basementSceneName = "BasementRoom_XW";

    private bool hasStarted;

    public bool HasStarted => hasStarted;

    public void Configure(
        GameObject newPlayerObject,
        SidePlayerController newPlayerController,
        PlayerInteractor newPlayerInteractor,
        Rigidbody2D newPlayerBody,
        Transform newConciergeTransform,
        Transform newElevatorTarget,
        string newBasementSceneName)
    {
        playerObject = newPlayerObject;
        playerController = newPlayerController;
        playerInteractor = newPlayerInteractor;
        playerBody = newPlayerBody;
        conciergeTransform = newConciergeTransform;
        elevatorTarget = newElevatorTarget;
        basementSceneName = string.IsNullOrWhiteSpace(newBasementSceneName) ? "BasementRoom_XW" : newBasementSceneName;
    }

    public void StartPostCartCutscene()
    {
        if (hasStarted || !isActiveAndEnabled)
        {
            return;
        }

        hasStarted = true;
        StartCoroutine(PlayRoutine());
    }

    private IEnumerator PlayRoutine()
    {
        Guest1RunProgress.DepartureCartChosen = true;
        DisablePlayerControl();

        Transform actor = conciergeTransform != null ? conciergeTransform : playerObject != null ? playerObject.transform : null;
        if (actor != null && elevatorTarget != null)
        {
            yield return MoveTo(actor, elevatorTarget.position, elevatorWalkDuration);
        }

        LoadBasementScene();
    }

    private void DisablePlayerControl()
    {
        if (playerController != null)
        {
            playerController.enabled = false;
        }

        if (playerInteractor != null)
        {
            playerInteractor.enabled = false;
        }

        if (playerBody != null)
        {
            playerBody.linearVelocity = Vector2.zero;
            playerBody.angularVelocity = 0f;
            playerBody.simulated = false;
        }
    }

    private static IEnumerator MoveTo(Transform actor, Vector3 targetPosition, float duration)
    {
        Vector3 startPosition = actor.position;
        FaceTravelDirection(actor, targetPosition.x - startPosition.x);

        if (duration <= 0f)
        {
            actor.position = targetPosition;
            yield break;
        }

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            actor.position = Vector3.Lerp(startPosition, targetPosition, Mathf.SmoothStep(0f, 1f, t));
            yield return null;
        }

        actor.position = targetPosition;
    }

    private void LoadBasementScene()
    {
        if (string.IsNullOrWhiteSpace(basementSceneName))
        {
            Debug.LogWarning("Guest1PostCartCutscene cannot load an empty basement scene name.", this);
            return;
        }

        if (HotelHungerRuntimeManager.Instance != null)
        {
            HotelHungerRuntimeManager.Instance.LoadSceneWithFade(basementSceneName);
            return;
        }

        if (!Application.CanStreamedLevelBeLoaded(basementSceneName))
        {
            Debug.LogWarning($"Guest1PostCartCutscene cannot load scene '{basementSceneName}' because it is not in Build Settings.", this);
            return;
        }

        SceneManager.LoadScene(basementSceneName);
    }

    private static void FaceTravelDirection(Transform actor, float deltaX)
    {
        if (actor == null || Mathf.Abs(deltaX) < 0.01f)
        {
            return;
        }

        Vector3 scale = actor.localScale;
        scale.x = Mathf.Abs(scale.x) * Mathf.Sign(deltaX);
        actor.localScale = scale;
    }
}
