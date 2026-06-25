using UnityEngine;

public class Guest1CartFlow : MonoBehaviour
{
    [Header("Story Gates")]
    [SerializeField] private InteractionTarget elevatorInspectionInteraction;
    [SerializeField] private SceneTransitionTrigger elevatorTransition;
    [SerializeField] private Guest1PostCartCutscene postCartCutscene;

    private bool hasChosenDepartureCart;

    private void Start()
    {
        if (Guest1RunProgress.DepartureCartChosen)
        {
            hasChosenDepartureCart = true;
            SetObjective("Follow the luggage route through the elevator.");
            SetElevatorReady(true);
            return;
        }

        SetObjective(Guest1RunProgress.SuitcaseCollected ?
            "Put Guest 1's luggage on the correct cart." :
            "Pick up Guest 1's luggage from the Lobby.");
        SetElevatorReady(false);
    }

    public void InspectArrivalCart()
    {
        ShowMessage("Arrival Cart is for new guests, not departures.");
    }

    public void InspectRoomServiceCart()
    {
        ShowMessage("Room Service Cart is not correct.");
    }

    public void InspectElevator()
    {
        if (hasChosenDepartureCart)
        {
            ShowMessage("The elevator route is ready.");
            return;
        }

        ShowMessage(Guest1RunProgress.SuitcaseCollected ?
            "Choose the correct cart first." :
            "Bring Guest 1's luggage from the Lobby first.");
    }

    public void ChooseDepartureCart()
    {
        if (!Guest1RunProgress.SuitcaseCollected)
        {
            ShowMessage("You need Guest 1's luggage before using the Departure Cart.");
            SetObjective("Return to the Lobby and pick up Guest 1's luggage.");
            SetElevatorReady(false);
            return;
        }

        if (hasChosenDepartureCart)
        {
            ShowMessage("The Departure Cart is ready.");
            if (postCartCutscene != null && !postCartCutscene.HasStarted)
            {
                postCartCutscene.StartPostCartCutscene();
            }

            return;
        }

        hasChosenDepartureCart = true;
        Guest1RunProgress.DepartureCartChosen = true;
        ShowMessage("You place Guest 1's luggage on the Departure Cart.");
        SetObjective("Follow the luggage route through the elevator.");
        SetElevatorReady(true);

        if (postCartCutscene != null)
        {
            postCartCutscene.StartPostCartCutscene();
        }
    }

    private void SetElevatorReady(bool ready)
    {
        if (elevatorInspectionInteraction != null)
        {
            elevatorInspectionInteraction.enabled = !ready;
        }

        if (elevatorTransition != null)
        {
            elevatorTransition.enabled = ready;
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
