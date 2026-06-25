using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class Ending1Manager : MonoBehaviour
{
    public static Ending1Manager Instance;

    [Header("Ending Images")]
    public Image blackScreen;
    public Image endingImage;
    public Image endingBloodImage;
    public Image creditImage;

    [Header("Ending Dialogue UI")]
    public GameObject endingDialogueBox;
    public Text endingDialogueText;

    [Header("Other UI")]
    public Text objectiveText;
    public GameObject crosshair;

    [Header("Route Check")]
    public RouteEventManager routeEventManager;

    [Header("Timing")]
    public float blackScreenDuration = 10f;
    public float blackFadeDuration = 1.5f;
    public float dialogueFadeDuration = 1.2f;
    public float bloodToBlackDuration = 5f;
    public float creditFadeDuration = 5f;

    [Header("Dialogue Style")]
    public Color dialogueTextColor = Color.white;
    public int dialogueFontSize = 30;

    public bool IsEndingPlaying { get; private set; }

    private bool endingLocked = false;
    private bool waitingForDialogueInput = false;
    private bool waitingForCreditInput = false;
    private int dialogueIndex = 0;

    private CanvasGroup dialogueCanvasGroup;

    private string[] dialogueLines =
    {
        "There are only two courses.",
        "A feast must be complete.",
        "And you are already here."
    };

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        IsEndingPlaying = false;
        waitingForDialogueInput = false;
        waitingForCreditInput = false;

        if (endingDialogueBox != null)
        {
            dialogueCanvasGroup = endingDialogueBox.GetComponent<CanvasGroup>();

            if (dialogueCanvasGroup == null)
            {
                dialogueCanvasGroup = endingDialogueBox.AddComponent<CanvasGroup>();
            }

            dialogueCanvasGroup.alpha = 1f;
            endingDialogueBox.SetActive(false);
        }

        if (endingDialogueText != null)
        {
            endingDialogueText.text = "";
            endingDialogueText.color = dialogueTextColor;
            endingDialogueText.fontSize = dialogueFontSize;
            endingDialogueText.alignment = TextAnchor.MiddleCenter;
        }

        if (blackScreen != null)
        {
            blackScreen.gameObject.SetActive(true);
            SetImageAlpha(blackScreen, 0f);
        }

        if (endingImage != null)
            endingImage.gameObject.SetActive(false);

        if (endingBloodImage != null)
        {
            endingBloodImage.gameObject.SetActive(false);
            SetImageAlpha(endingBloodImage, 1f);
        }

        if (creditImage != null)
        {
            creditImage.gameObject.SetActive(false);
            SetImageAlpha(creditImage, 0f);
        }
    }

    void Update()
    {
        if (!IsEndingPlaying)
            return;

        if (waitingForDialogueInput && Input.GetKeyDown(KeyCode.E))
        {
            AdvanceDialogue();
        }

        if (waitingForCreditInput && Input.GetKeyDown(KeyCode.E))
        {
            StartCoroutine(FadeInCredits());
        }
    }

    public void LockEnding1()
    {
        endingLocked = true;
    }

    public void StartEnding1()
    {
        if (endingLocked)
            return;

        if (IsEndingPlaying)
            return;

        if (routeEventManager != null && !routeEventManager.TreeHasFallen)
        {
            Debug.Log("Ending 1 cannot start yet. Tree has not fallen.");
            return;
        }

        StartCoroutine(Ending1Sequence());
    }

    IEnumerator Ending1Sequence()
    {
        IsEndingPlaying = true;
        waitingForDialogueInput = false;
        waitingForCreditInput = false;
        dialogueIndex = 0;

        if (GameManager.Instance != null)
        {
            GameManager.Instance.StopChaseForEnding1();
        }

        if (objectiveText != null)
            objectiveText.text = "";

        if (crosshair != null)
            crosshair.SetActive(false);

        if (endingDialogueBox != null)
            endingDialogueBox.SetActive(false);

        if (endingDialogueText != null)
            endingDialogueText.text = "";

        // Black screen appears and stays for 10 seconds.
        if (blackScreen != null)
        {
            blackScreen.gameObject.SetActive(true);
            blackScreen.transform.SetAsLastSibling();
            SetImageAlpha(blackScreen, 1f);
        }

        yield return new WaitForSeconds(blackScreenDuration);

        // Ending background image appears.
        if (endingImage != null)
        {
            endingImage.gameObject.SetActive(true);
            endingImage.transform.SetAsLastSibling();
        }

        // Black screen fades away to reveal ending image.
        if (blackScreen != null)
        {
            blackScreen.transform.SetAsLastSibling();
            yield return StartCoroutine(FadeImage(blackScreen, 1f, 0f, blackFadeDuration));
        }

        // Dialogue box appears on top.
        if (endingDialogueBox != null)
        {
            endingDialogueBox.SetActive(true);
            endingDialogueBox.transform.SetAsLastSibling();

            if (dialogueCanvasGroup != null)
            {
                dialogueCanvasGroup.alpha = 0f;
            }
        }

        if (endingDialogueText != null)
        {
            endingDialogueText.gameObject.SetActive(true);
            endingDialogueText.text = dialogueLines[0];
            endingDialogueText.color = dialogueTextColor;
            endingDialogueText.fontSize = dialogueFontSize;
            endingDialogueText.alignment = TextAnchor.MiddleCenter;
        }

        if (dialogueCanvasGroup != null)
        {
            yield return StartCoroutine(FadeCanvasGroup(dialogueCanvasGroup, 0f, 1f, dialogueFadeDuration));
        }

        waitingForDialogueInput = true;
    }

    void AdvanceDialogue()
    {
        dialogueIndex++;

        if (dialogueIndex < dialogueLines.Length)
        {
            if (endingDialogueText != null)
            {
                endingDialogueText.text = dialogueLines[dialogueIndex];
                endingDialogueText.color = dialogueTextColor;
                endingDialogueText.fontSize = dialogueFontSize;
            }
        }
        else
        {
            StartCoroutine(ShowBloodThenBlack());
        }
    }

    IEnumerator ShowBloodThenBlack()
    {
        waitingForDialogueInput = false;

        if (endingDialogueBox != null)
            endingDialogueBox.SetActive(false);

        if (endingDialogueText != null)
            endingDialogueText.text = "";

        // Blood splatter appears first.
        if (endingBloodImage != null)
        {
            endingBloodImage.gameObject.SetActive(true);
            endingBloodImage.transform.SetAsLastSibling();
            SetImageAlpha(endingBloodImage, 1f);
        }

        // Then black screen slowly fades over the blood.
        if (blackScreen != null)
        {
            blackScreen.gameObject.SetActive(true);
            blackScreen.transform.SetAsLastSibling();
            SetImageAlpha(blackScreen, 0f);

            yield return StartCoroutine(FadeImage(blackScreen, 0f, 1f, bloodToBlackDuration));
        }

        // Hide blood after black fully covers it.
        if (endingBloodImage != null)
        {
            endingBloodImage.gameObject.SetActive(false);
        }

        // Now wait for E to show credits.
        waitingForCreditInput = true;
    }

    IEnumerator FadeInCredits()
    {
        waitingForCreditInput = false;

        // Credit fades in on top of black screen.
        if (creditImage != null)
        {
            creditImage.gameObject.SetActive(true);
            creditImage.transform.SetAsLastSibling();
            SetImageAlpha(creditImage, 0f);

            yield return StartCoroutine(FadeImage(creditImage, 0f, 1f, creditFadeDuration));
        }
    }

    IEnumerator FadeImage(Image image, float startAlpha, float targetAlpha, float duration)
    {
        float timer = 0f;

        while (timer < duration)
        {
            timer += Time.deltaTime;
            float alpha = Mathf.Lerp(startAlpha, targetAlpha, timer / duration);
            SetImageAlpha(image, alpha);
            yield return null;
        }

        SetImageAlpha(image, targetAlpha);
    }

    IEnumerator FadeCanvasGroup(CanvasGroup group, float startAlpha, float targetAlpha, float duration)
    {
        float timer = 0f;
        group.alpha = startAlpha;

        while (timer < duration)
        {
            timer += Time.deltaTime;
            group.alpha = Mathf.Lerp(startAlpha, targetAlpha, timer / duration);
            yield return null;
        }

        group.alpha = targetAlpha;
    }

    void SetImageAlpha(Image image, float alpha)
    {
        Color color = image.color;
        color.a = alpha;
        image.color = color;
    }
}