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

    [Header("Story Gates")]
    [SerializeField] private SceneTransitionTrigger elevatorTransition;

    private bool hasSpokenToGuest;
    private bool hasGlass;
    private bool hasGoldStraw;
    private bool hasHouseSpecialSyrup;
    private bool hasUmbrella;
    private bool hasShownDrinkReadyMessage;
    private bool hasServedDrink;

    private void Start()
    {
        SetObjective("Welcome the new guest.");
        SetTransitionEnabled(false);
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
            SetObjective("Take Guest 1 to the carts.");
            SetTransitionEnabled(true);
            return;
        }

        if (!HasAllIngredients())
        {
            ShowMessage("Prepare the welcome drink.");
            return;
        }

        ShowMessage("Take Guest 1 to the carts.");
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
