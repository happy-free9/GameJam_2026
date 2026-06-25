using UnityEngine;

public class Ending1Trigger : MonoBehaviour
{
    public RouteEventManager routeEventManager;

    private bool hasTriggered = false;

    void OnTriggerEnter(Collider other)
    {
        if (hasTriggered)
            return;

        PlayerMovement player = other.GetComponentInParent<PlayerMovement>();

        if (player == null)
            return;

        if (routeEventManager == null)
        {
            Debug.LogWarning("Ending1Trigger: Route Event Manager is not assigned.");
            return;
        }

        if (!routeEventManager.TreeHasFallen)
        {
            Debug.Log("Ending 1 trigger touched, but tree has not fallen yet.");
            return;
        }

        if (Ending1Manager.Instance == null)
        {
            Debug.LogWarning("Ending1Trigger: Ending1Manager not found.");
            return;
        }

        hasTriggered = true;

        Debug.Log("Ending 1 triggered.");
        Ending1Manager.Instance.StartEnding1();
    }
}