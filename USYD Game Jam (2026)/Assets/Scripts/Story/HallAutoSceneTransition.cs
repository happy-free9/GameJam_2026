using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

[RequireComponent(typeof(Collider2D))]
public class HallAutoSceneTransition : MonoBehaviour
{
    [SerializeField] private string targetSceneName;
    [FormerlySerializedAs("targetSpawnPointId")]
    [SerializeField] private string targetSpawnId;
    [SerializeField] private float activationDelay = 0.45f;
    [SerializeField] private string requiredPlayerTag = "Player";
    [SerializeField] private bool requireExitBeforeReentry = true;

    private static bool sceneLoadInProgress;
    private bool hasTriggered;
    private bool canActivate;
    private bool waitingForExitBeforeReentry;

    private void Reset()
    {
        Collider2D trigger = GetComponent<Collider2D>();
        if (trigger != null)
        {
            trigger.isTrigger = true;
        }
    }

    private void OnValidate()
    {
        Collider2D trigger = GetComponent<Collider2D>();
        if (trigger != null && !trigger.isTrigger)
        {
            trigger.isTrigger = true;
        }
    }

    private void OnEnable()
    {
        sceneLoadInProgress = false;
        hasTriggered = false;
        canActivate = false;
        waitingForExitBeforeReentry = false;
        SceneManager.sceneLoaded += HandleSceneLoaded;
        StartCoroutine(EnableAfterDelay());
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= HandleSceneLoaded;
        StopAllCoroutines();
    }

    public void Configure(string newTargetSceneName, string newTargetSpawnPointId)
    {
        targetSceneName = newTargetSceneName;
        targetSpawnId = newTargetSpawnPointId;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (hasTriggered || sceneLoadInProgress || string.IsNullOrWhiteSpace(targetSceneName))
        {
            return;
        }

        if (!IsPlayer(other))
        {
            return;
        }

        if (!canActivate)
        {
            if (requireExitBeforeReentry)
            {
                waitingForExitBeforeReentry = true;
            }

            return;
        }

        if (waitingForExitBeforeReentry)
        {
            return;
        }

        TriggerTransition();
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (IsPlayer(other))
        {
            waitingForExitBeforeReentry = false;
        }
    }

    private void TriggerTransition()
    {
        hasTriggered = true;
        sceneLoadInProgress = true;

        if (!string.IsNullOrWhiteSpace(targetSpawnId))
        {
            SceneTransitionManager.Instance.LoadScene(targetSceneName, targetSpawnId);
            return;
        }

        SceneManager.LoadScene(targetSceneName);
    }

    private IEnumerator EnableAfterDelay()
    {
        float delay = Mathf.Max(0f, activationDelay);
        if (delay > 0f)
        {
            yield return new WaitForSeconds(delay);
        }

        canActivate = true;
    }

    private void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        sceneLoadInProgress = false;
    }

    private bool IsPlayer(Collider2D other)
    {
        if (other == null)
        {
            return false;
        }

        if (!string.IsNullOrWhiteSpace(requiredPlayerTag))
        {
            bool matchesRequiredTag = other.CompareTag(requiredPlayerTag) ||
                (other.transform.root != null && other.transform.root.CompareTag(requiredPlayerTag));
            if (!matchesRequiredTag)
            {
                return false;
            }
        }

        return other.GetComponent<PlayerInteractor>() != null ||
            other.GetComponentInParent<PlayerInteractor>() != null;
    }
}
