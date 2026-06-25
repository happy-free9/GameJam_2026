using System.Collections.Generic;
using UnityEngine;

public class Guest1LobbyFlow : MonoBehaviour
{
    [Header("Ingredient Visuals")]
    [SerializeField] private GameObject glassVisual;
    [SerializeField] private GameObject goldStrawVisual;
    [SerializeField] private GameObject houseSpecialSyrupVisual;
    [SerializeField] private GameObject umbrellaVisual;

    [Header("Ingredient Interactions")]
    [SerializeField] private InteractionTarget glassInteraction;
    [SerializeField] private InteractionTarget goldStrawInteraction;
    [SerializeField] private InteractionTarget houseSpecialSyrupInteraction;
    [SerializeField] private InteractionTarget umbrellaInteraction;

    [Header("Guest 1 Luggage")]
    [SerializeField] private GameObject suitcaseVisual;
    [SerializeField] private InteractionTarget suitcaseInteraction;

    [Header("Story Gates")]
    [SerializeField] private SceneTransitionTrigger elevatorTransition;

    private bool hasSpokenToGuest;
    private bool hasGlass;
    private bool hasGoldStraw;
    private bool hasHouseSpecialSyrup;
    private bool hasUmbrella;
    private bool hasShownDrinkReadyMessage;
    private bool hasServedDrink;
    private bool hasPickedUpSuitcase;

    private void Start()
    {
        if (Guest1RunProgress.LobbyCompleted)
        {
            RestoreCompletedLobbyFlow();
            return;
        }

        SetObjective("Welcome the new guest.");
        SetTransitionEnabled(false);
        SetSuitcaseAvailable(false);
    }

    public void InteractWithGuest()
    {
        if (!hasSpokenToGuest)
        {
            hasSpokenToGuest = true;
            ShowDialogue(
                ("Guest 1", "Wow. This place is fancy. I think I just need to check in and rest."),
                ("You", "Of course! But first, please enjoy a complimentary welcome drink."));
            SetObjective("Prepare the welcome drink.");
            return;
        }

        if (HasAllIngredients() && !hasServedDrink)
        {
            hasServedDrink = true;
            ShowDialogue(
                ("Guest 1", "I feel... really tired all of a sudden."),
                ("You", "Wonderful! That means the hotel is already helping you relax."));
            SetObjective("Pick up Guest 1's luggage.");
            SetTransitionEnabled(true);
            SetSuitcaseAvailable(true);
            Guest1RunProgress.LobbyCompleted = true;
            return;
        }

        if (!HasAllIngredients())
        {
            ShowMessage("Prepare the welcome drink.");
            return;
        }

        if (Guest1RunProgress.DepartureCartChosen)
        {
            ShowMessage("Follow the luggage route through the elevator.");
            return;
        }

        ShowMessage(Guest1RunProgress.SuitcaseCollected ?
            "Take Guest 1's luggage to the Departure Cart." :
            "Pick up Guest 1's luggage.");
    }

    public void PickUpGuest1Luggage()
    {
        if (!Guest1RunProgress.LobbyCompleted && !hasServedDrink)
        {
            ShowMessage("Serve Guest 1's welcome drink first.");
            return;
        }

        if (hasPickedUpSuitcase || Guest1RunProgress.SuitcaseCollected)
        {
            ShowMessage("Guest 1's luggage is already with you.");
            return;
        }

        hasPickedUpSuitcase = true;
        Guest1RunProgress.SuitcaseCollected = true;
        SetSuitcaseAvailable(false);
        SetObjective("Take Guest 1's luggage to the Departure Cart.");
        ShowMessage("You pick up Guest 1's luggage.");
    }

    public void CollectGlass()
    {
        CollectIngredient(ref hasGlass, glassVisual, glassInteraction, "Crystal glass collected.");
    }

    public void CollectGoldStraw()
    {
        CollectIngredient(ref hasGoldStraw, goldStrawVisual, goldStrawInteraction, "Gold straw collected.");
    }

    public void CollectHouseSpecialSyrup()
    {
        CollectIngredient(ref hasHouseSpecialSyrup, houseSpecialSyrupVisual, houseSpecialSyrupInteraction, "House Special added.");
    }

    public void CollectUmbrella()
    {
        CollectIngredient(ref hasUmbrella, umbrellaVisual, umbrellaInteraction, "Little umbrella collected.");
    }

    private void CollectIngredient(
        ref bool collected,
        GameObject visual,
        InteractionTarget interaction,
        string collectedMessage)
    {
        if (!hasSpokenToGuest)
        {
            ShowMessage("Speak to the guest first.");
            return;
        }

        if (collected)
        {
            return;
        }

        collected = true;

        if (visual != null)
        {
            visual.SetActive(false);
        }

        if (interaction != null)
        {
            interaction.enabled = false;
        }

        if (HasAllIngredients() && !hasShownDrinkReadyMessage)
        {
            hasShownDrinkReadyMessage = true;
            ShowDialogue(
                ("Hotel", collectedMessage),
                ("Hotel", "Perfect. A luxury stay must begin with a personal touch."));
            SetObjective("Serve the welcome drink.");
            return;
        }

        ShowMessage(collectedMessage);
    }

    private bool HasAllIngredients()
    {
        return hasGlass && hasGoldStraw && hasHouseSpecialSyrup && hasUmbrella;
    }

    private void RestoreCompletedLobbyFlow()
    {
        hasSpokenToGuest = true;
        hasGlass = true;
        hasGoldStraw = true;
        hasHouseSpecialSyrup = true;
        hasUmbrella = true;
        hasShownDrinkReadyMessage = true;
        hasServedDrink = true;
        hasPickedUpSuitcase = Guest1RunProgress.SuitcaseCollected;

        SetIngredientAvailable(glassVisual, glassInteraction, false);
        SetIngredientAvailable(goldStrawVisual, goldStrawInteraction, false);
        SetIngredientAvailable(houseSpecialSyrupVisual, houseSpecialSyrupInteraction, false);
        SetIngredientAvailable(umbrellaVisual, umbrellaInteraction, false);

        SetSuitcaseAvailable(!hasPickedUpSuitcase && !Guest1RunProgress.DepartureCartChosen);
        if (Guest1RunProgress.DepartureCartChosen)
        {
            SetObjective("Follow the luggage route through the elevator.");
        }
        else
        {
            SetObjective(hasPickedUpSuitcase ?
                "Take Guest 1's luggage to the Departure Cart." :
                "Pick up Guest 1's luggage.");
        }

        SetTransitionEnabled(true);
    }

    private void SetSuitcaseAvailable(bool available)
    {
        if (suitcaseVisual != null)
        {
            suitcaseVisual.SetActive(available);
        }

        if (suitcaseInteraction != null)
        {
            suitcaseInteraction.enabled = available;
        }
    }

    private void SetIngredientAvailable(GameObject visual, InteractionTarget interaction, bool available)
    {
        if (visual != null)
        {
            visual.SetActive(available);
        }

        if (interaction != null)
        {
            interaction.enabled = available;
        }
    }

    private void SetTransitionEnabled(bool enabled)
    {
        if (elevatorTransition != null)
        {
            elevatorTransition.enabled = enabled;
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

    private void ShowDialogue(params (string speakerName, string bodyText)[] lines)
    {
        if (DialoguePanelController.Instance == null)
        {
            return;
        }

        List<DialogueLine> dialogueLines = new();
        for (int i = 0; i < lines.Length; i++)
        {
            dialogueLines.Add(new DialogueLine
            {
                speakerName = lines[i].speakerName,
                bodyText = lines[i].bodyText
            });
        }

        DialoguePanelController.Instance.StartDialogue(dialogueLines);
    }
}
