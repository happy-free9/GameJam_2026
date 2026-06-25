using UnityEngine;

public class Guest1WaitingFlow : MonoBehaviour
{
    [Header("Luggage")]
    [SerializeField] private GameObject luggageVisual;
    [SerializeField] private InteractionTarget luggageInteraction;

    [Header("Story Gates")]
    [SerializeField] private SceneTransitionTrigger waitingRoomTransition;

    private void Start()
    {
        DisableHallLuggage();
        SetObjective("Enter the Dining Room Door.");
        SetTransitionEnabled(true);
    }

    public void InspectPrivateElevator()
    {
        ShowMessage("Use the Dining Room Door.");
    }

    public void CarryLuggage()
    {
        DisableHallLuggage();
        ShowMessage("Guest 1's luggage is handled through the Lobby and Departure Cart.");
        SetObjective("Enter the Dining Room Door.");
        SetTransitionEnabled(true);
    }

    private void RestoreSuitcaseCollected()
    {
        DisableHallLuggage();
        SetObjective("Enter the Dining Room Door.");
        SetTransitionEnabled(true);
    }

    private void DisableHallLuggage()
    {
        if (luggageVisual != null)
        {
            luggageVisual.SetActive(false);
        }

        if (luggageInteraction != null)
        {
            luggageInteraction.enabled = false;
        }
    }

    private void SetTransitionEnabled(bool enabled)
    {
        if (waitingRoomTransition != null)
        {
            waitingRoomTransition.enabled = enabled;
        }
    }

    private void SetObjective(string text)
    {
        ObjectivePanelController.Instance?.SetObjective(text);
    }

    private void ShowMessage(string bodyText)
    {
        DialoguePanelController.Instance?.ShowSingleLine("Hotel", bodyText);
    }
}
