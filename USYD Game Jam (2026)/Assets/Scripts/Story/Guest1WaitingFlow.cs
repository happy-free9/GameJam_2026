using UnityEngine;

public class Guest1WaitingFlow : MonoBehaviour
{
    [Header("Luggage")]
    [SerializeField] private GameObject luggageVisual;
    [SerializeField] private InteractionTarget luggageInteraction;

    [Header("Story Gates")]
    [SerializeField] private SceneTransitionTrigger waitingRoomTransition;

    private bool hasCarriedLuggage;

    private void Start()
    {
        SetObjective("Bring Guest 1's luggage to the Waiting Room.");
        SetTransitionEnabled(false);
    }

    public void InspectPrivateElevator()
    {
        ShowMessage("The luggage belongs in the Waiting Room.");
    }

    public void CarryLuggage()
    {
        if (hasCarriedLuggage)
        {
            return;
        }

        hasCarriedLuggage = true;
        ShowMessage("You carry the luggage to the Waiting Room.");
        SetObjective("Enter Waiting Room.");

        if (luggageVisual != null)
        {
            luggageVisual.SetActive(false);
        }

        if (luggageInteraction != null)
        {
            luggageInteraction.enabled = false;
        }

        SetTransitionEnabled(true);
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
