using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Collider2D))]
public class SimpleChaseAgent2D : MonoBehaviour
{
    [Header("Target")]
    [Tooltip("The other character in the two-person chase. If empty and auto-find is enabled, the agent looks for PlayerInteractor.")]
    [SerializeField] private Transform target;
    [Tooltip("If enabled, StartChase and Update will try to find the player automatically.")]
    [SerializeField] private bool autoFindPlayerTarget = true;

    [Header("Movement")]
    [Tooltip("Units per second while pursuing the other character.")]
    [SerializeField] private float moveSpeed = 3f;

    [Header("Catch Behavior")]
    [Tooltip("If enabled, the chasing character stops moving after catching the other character.")]
    [SerializeField] private bool stopOnPlayerCaught = true;
    [Tooltip("Invoked when the chasing character collides with the other character.")]
    [SerializeField] private UnityEvent onPlayerCaught = new();

    private bool isChasing;

    public UnityEvent OnPlayerCaught => onPlayerCaught;

    private void Reset()
    {
        Collider2D agentCollider = GetComponent<Collider2D>();
        agentCollider.isTrigger = true;
    }

    private void OnValidate()
    {
        if (moveSpeed < 0f)
        {
            moveSpeed = 0f;
        }
    }

    private void Update()
    {
        if (!isChasing)
        {
            return;
        }

        if (target == null && autoFindPlayerTarget)
        {
            PlayerInteractor player = FindFirstObjectByType<PlayerInteractor>();
            if (player == null)
            {
                return;
            }

            target = player.transform;
        }

        Vector3 nextPosition = Vector3.MoveTowards(
            transform.position,
            target.position,
            moveSpeed * Time.deltaTime);

        transform.position = nextPosition;
    }

    public void StartChase()
    {
        if (target == null && autoFindPlayerTarget)
        {
            PlayerInteractor player = FindFirstObjectByType<PlayerInteractor>();
            if (player != null)
            {
                target = player.transform;
            }
        }

        if (target == null)
        {
            Debug.LogWarning(
                $"SimpleChaseAgent2D on '{name}' has no target. Assign one in the Inspector or enable autoFindPlayerTarget.",
                this);
            return;
        }

        isChasing = true;
    }

    public void StopChase()
    {
        isChasing = false;
    }

    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.TryGetComponent(out PlayerInteractor _))
        {
            return;
        }

        if (stopOnPlayerCaught)
        {
            StopChase();
        }

        onPlayerCaught?.Invoke();
    }
}
