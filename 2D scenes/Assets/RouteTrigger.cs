using UnityEngine;

public class RouteTrigger : MonoBehaviour
{
    public RouteEventManager routeEventManager;
    public int triggerNumber = 1;

    private bool hasTriggered = false;

    void OnTriggerEnter(Collider other)
    {
        if (hasTriggered)
            return;

        PlayerMovement player = other.GetComponentInParent<PlayerMovement>();

        if (player == null)
            return;

        hasTriggered = true;

        if (routeEventManager != null)
        {
            routeEventManager.ActivateTrigger(triggerNumber);
        }
    }
}