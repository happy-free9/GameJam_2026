using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public sealed class BasementRoomCutscene : MonoBehaviour
{
    [Header("Player")]
    [SerializeField] private GameObject playerObject;
    [SerializeField] private SidePlayerController playerController;
    [SerializeField] private PlayerInteractor playerInteractor;
    [SerializeField] private Rigidbody2D playerBody;
    [SerializeField] private SpriteRenderer[] playerRenderers;

    [Header("Route")]
    [SerializeField] private Transform conciergeTransform;
    [SerializeField] private Transform waitingRoomTarget;
    [SerializeField] private float waitingRoomWalkDuration = 1.5f;
    [SerializeField] private bool playOnStart = true;
    [SerializeField] private bool requireDepartureCartProgress = true;

    [Header("Next Scene")]
    [SerializeField] private string chaseSceneName = "Guest3DiningRoom_XW";

    private bool hasStarted;

    public void Configure(
        GameObject newPlayerObject,
        SidePlayerController newPlayerController,
        PlayerInteractor newPlayerInteractor,
        Rigidbody2D newPlayerBody,
        SpriteRenderer[] newPlayerRenderers,
        Transform newConciergeTransform,
        Transform newWaitingRoomTarget,
        string newChaseSceneName)
    {
        playerObject = newPlayerObject;
        playerController = newPlayerController;
        playerInteractor = newPlayerInteractor;
        playerBody = newPlayerBody;
        playerRenderers = newPlayerRenderers;
        conciergeTransform = newConciergeTransform;
        waitingRoomTarget = newWaitingRoomTarget;
        chaseSceneName = string.IsNullOrWhiteSpace(newChaseSceneName) ? "Guest3DiningRoom_XW" : newChaseSceneName;
    }

    private void Start()
    {
        if (playOnStart && (!requireDepartureCartProgress || Guest1RunProgress.DepartureCartChosen))
        {
            StartBasementCutscene();
        }
    }

    public void StartBasementCutscene()
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
        DisablePlayerControl();

        Transform actor = conciergeTransform != null ? conciergeTransform : playerObject != null ? playerObject.transform : null;
        if (actor != null && waitingRoomTarget != null)
        {
            yield return MoveTo(actor, waitingRoomTarget.position, waitingRoomWalkDuration);
        }

        HidePlayer();
        LoadChaseScene();
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

    private void HidePlayer()
    {
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

        if (playerObject != null)
        {
            playerObject.SetActive(false);
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

    private void LoadChaseScene()
    {
        if (string.IsNullOrWhiteSpace(chaseSceneName))
        {
            Debug.LogWarning("BasementRoomCutscene cannot load an empty chase scene name.", this);
            return;
        }

        if (HotelHungerRuntimeManager.Instance != null)
        {
            HotelHungerRuntimeManager.Instance.LoadSceneWithFade(chaseSceneName);
            return;
        }

        if (!Application.CanStreamedLevelBeLoaded(chaseSceneName))
        {
            Debug.LogWarning($"BasementRoomCutscene cannot load scene '{chaseSceneName}' because it is not in Build Settings.", this);
            return;
        }

        SceneManager.LoadScene(chaseSceneName);
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
