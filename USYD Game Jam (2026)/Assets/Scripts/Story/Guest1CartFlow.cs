using UnityEngine;

public class Guest1CartFlow : MonoBehaviour
{
    private void Start()
    {
        SetObjective("Put Guest 1's luggage on the correct cart.");
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
        ShowMessage("Choose the correct cart first.");
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
