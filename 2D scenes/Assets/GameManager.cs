using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("UI")]
    public GameObject dialogueBox;
    public Text dialogueText;
    public Text objectiveText;
    public Image bloodImage;

    [Header("Chase")]
    public GameObject invisibleChaser;
    public Transform player;
    public float chaserSpeed = 1.3f;
    public float killDistance = 1.2f;
    public float deathDelay = 1.5f;

    public bool DoorUnlocked { get; private set; }
    public bool IsDialogueActive { get; private set; }

    private bool chaseStarted = false;
    private bool playerDead = false;
    private bool menuSequenceDone = false;
    private int dialogueStep = 0;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        DoorUnlocked = false;
        IsDialogueActive = false;

        if (dialogueBox != null)
            dialogueBox.SetActive(false);

        if (dialogueText != null)
            dialogueText.text = "";

        if (objectiveText != null)
            objectiveText.text = "";

        if (bloodImage != null)
            bloodImage.gameObject.SetActive(false);

        if (invisibleChaser != null)
            invisibleChaser.SetActive(false);
    }

    void Update()
    {
        if (!chaseStarted || playerDead || invisibleChaser == null || player == null)
            return;

        invisibleChaser.transform.position = Vector3.MoveTowards(
            invisibleChaser.transform.position,
            player.position,
            chaserSpeed * Time.deltaTime
        );

        float distance = Vector3.Distance(invisibleChaser.transform.position, player.position);

        if (distance <= killDistance)
        {
            StartCoroutine(DeathSequence());
        }
    }

    void MakeDialogueBoxVisible()
    {
        if (dialogueBox == null)
            return;

        CanvasGroup group = dialogueBox.GetComponent<CanvasGroup>();

        if (group != null)
        {
            group.alpha = 1f;
            group.interactable = true;
            group.blocksRaycasts = true;
        }
    }

    public void OnMenuClosedAfterReading()
    {
        if (menuSequenceDone)
            return;

        menuSequenceDone = true;
        StartDialogueSequence();
    }

    void StartDialogueSequence()
    {
        IsDialogueActive = true;
        dialogueStep = 0;

        if (dialogueBox != null)
        {
            dialogueBox.SetActive(true);
            MakeDialogueBoxVisible();
        }

        if (dialogueText != null)
            dialogueText.text = "Excuse me? You are not supposed to be here.";
    }

    public void AdvanceDialogue()
    {
        if (!IsDialogueActive)
            return;

        dialogueStep++;

        if (dialogueStep == 1)
        {
            if (dialogueText != null)
                dialogueText.text = "You should be waiting in the Waiting Room with the other guests.";
        }
        else if (dialogueStep == 2)
        {
            EndDialogueAndStartRun();
        }
    }

    void EndDialogueAndStartRun()
    {
        IsDialogueActive = false;

        if (dialogueText != null)
            dialogueText.text = "";

        if (dialogueBox != null)
            dialogueBox.SetActive(false);

        if (objectiveText != null)
            objectiveText.text = "Objective: Run";

        DoorUnlocked = true;

        if (invisibleChaser != null)
            invisibleChaser.SetActive(true);

        chaseStarted = true;
        playerDead = false;
    }

    public void ShowTemporaryMessage(string message)
    {
        StartCoroutine(TemporaryMessageRoutine(message));
    }

    IEnumerator TemporaryMessageRoutine(string message)
    {
        if (dialogueBox != null)
        {
            dialogueBox.SetActive(true);
            MakeDialogueBoxVisible();
        }

        if (dialogueText != null)
            dialogueText.text = message;

        yield return new WaitForSeconds(2f);

        if (!IsDialogueActive)
        {
            if (dialogueText != null)
                dialogueText.text = "";

            if (dialogueBox != null)
                dialogueBox.SetActive(false);
        }
    }

    public void StopChaseForEnding1()
    {
        chaseStarted = false;
        playerDead = true;
        IsDialogueActive = false;

        if (invisibleChaser != null)
            invisibleChaser.SetActive(false);

        if (bloodImage != null)
            bloodImage.gameObject.SetActive(false);

        if (objectiveText != null)
            objectiveText.text = "";

        if (dialogueText != null)
            dialogueText.text = "";

        if (dialogueBox != null)
            dialogueBox.SetActive(false);
    }

    IEnumerator DeathSequence()
    {
        playerDead = true;

        if (bloodImage != null)
            bloodImage.gameObject.SetActive(true);

        yield return new WaitForSeconds(deathDelay);

        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}