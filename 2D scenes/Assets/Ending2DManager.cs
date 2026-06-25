using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class Ending2DManager : MonoBehaviour
{
    public static Ending2DManager Instance;

    [Header("Player")]
    public Transform player;
    public Ending2DPlayerController playerController;

    [Header("Player Visibility Fix")]
    public float playerVisibleZ = -1f;
    public int playerSortingOrder = 100;

    [Header("Runner")]
    public Transform runner;
    public Transform runnerStartPoint;
    public float runnerSpeed = 18f;

    [Header("UI")]
    public GameObject dialogueBox;
    public Text dialogueText;
    public Text objectiveText;
    public Image blackScreen;

    [Header("Audio")]
    public AudioSource musicSource;

    [Header("Dialogue Timing")]
    public float dialogueLineDuration = 2.2f;

    [Header("Input Safety")]
    public float eatInputDelay = 0.5f;

    private bool introFinished = false;
    private bool playerSitting = false;
    private bool waitingToEat = false;
    private bool endingStarted = false;

    private float canEatAfterTime = 0f;

    private string[] introLines =
    {
        "Excellent service.",
        "You have outdone yourself.",
        "You understand luxury better than most people born to it.",
        "Come. Sit with us.",
        "Tonight, you have earned it."
    };

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        ForcePlayerVisible();

        if (blackScreen != null)
        {
            blackScreen.gameObject.SetActive(true);
            SetImageAlpha(blackScreen, 0f);
        }

        if (objectiveText != null)
        {
            objectiveText.text = "";
            objectiveText.color = Color.white;
            objectiveText.fontSize = 24;
            objectiveText.alignment = TextAnchor.UpperLeft;
        }

        if (dialogueText != null)
        {
            dialogueText.color = Color.white;
            dialogueText.fontSize = 28;
            dialogueText.alignment = TextAnchor.MiddleCenter;
            dialogueText.text = "";
        }

        if (dialogueBox != null)
        {
            dialogueBox.SetActive(false);
        }

        if (runner != null)
        {
            runner.gameObject.SetActive(false);
        }

        if (musicSource != null)
        {
            musicSource.loop = true;
            musicSource.Play();
        }

        StartCoroutine(IntroDialogueSequence());
    }

    void Update()
    {
        ForcePlayerVisible();

        if (waitingToEat && Time.time >= canEatAfterTime && Input.GetKeyDown(KeyCode.E))
        {
            StartCoroutine(EatEndingSequence());
        }
    }

    IEnumerator IntroDialogueSequence()
    {
        if (dialogueBox != null)
        {
            dialogueBox.SetActive(true);
        }

        for (int i = 0; i < introLines.Length; i++)
        {
            if (dialogueText != null)
            {
                dialogueText.text = introLines[i];
            }

            yield return new WaitForSeconds(dialogueLineDuration);
        }

        if (dialogueText != null)
        {
            dialogueText.text = "";
        }

        if (dialogueBox != null)
        {
            dialogueBox.SetActive(false);
        }

        introFinished = true;

        if (objectiveText != null)
        {
            objectiveText.text = "Objective: Sit with them";
        }
    }

    public void ShowSitPrompt(bool show)
    {
        if (!introFinished || playerSitting)
            return;

        if (objectiveText != null)
        {
            objectiveText.text = show ? "Objective: Press E to sit" : "Objective: Sit with them";
        }
    }

    public bool TrySitAtSpot(Transform spot)
    {
        if (!introFinished)
            return false;

        if (playerSitting)
            return false;

        playerSitting = true;
        waitingToEat = true;
        canEatAfterTime = Time.time + eatInputDelay;

        if (player != null && spot != null)
        {
            Vector3 targetPosition = new Vector3(
                spot.position.x,
                spot.position.y,
                playerVisibleZ
            );

            player.position = targetPosition;

            Rigidbody2D rb = player.GetComponent<Rigidbody2D>();

            if (rb != null)
            {
                rb.velocity = Vector2.zero;
                rb.angularVelocity = 0f;
                rb.position = new Vector2(targetPosition.x, targetPosition.y);
            }
        }

        ForcePlayerVisible();

        if (playerController != null)
        {
            playerController.SetCanMove(false);
        }

        if (blackScreen != null)
        {
            blackScreen.gameObject.SetActive(true);
            SetImageAlpha(blackScreen, 0f);
        }

        if (objectiveText != null)
        {
            objectiveText.text = "Objective: Press E to eat";
        }

        return true;
    }

    void ForcePlayerVisible()
    {
        if (player == null)
            return;

        player.gameObject.SetActive(true);

        Vector3 pos = player.position;
        pos.z = playerVisibleZ;
        player.position = pos;

        SpriteRenderer[] sprites = player.GetComponentsInChildren<SpriteRenderer>(true);

        for (int i = 0; i < sprites.Length; i++)
        {
            sprites[i].gameObject.SetActive(true);
            sprites[i].enabled = true;
            sprites[i].sortingOrder = playerSortingOrder;
        }
    }

    IEnumerator EatEndingSequence()
    {
        if (endingStarted)
            yield break;

        endingStarted = true;
        waitingToEat = false;

        ForcePlayerVisible();

        if (objectiveText != null)
        {
            objectiveText.text = "";
        }

        if (musicSource != null)
        {
            musicSource.Stop();
        }

        if (runner != null)
        {
            runner.gameObject.SetActive(true);

            SpriteRenderer runnerSprite = runner.GetComponentInChildren<SpriteRenderer>(true);

            if (runnerSprite != null)
            {
                runnerSprite.enabled = true;
                runnerSprite.sortingOrder = playerSortingOrder + 1;
            }

            if (runnerStartPoint != null)
            {
                runner.position = new Vector3(
                    runnerStartPoint.position.x,
                    runnerStartPoint.position.y,
                    playerVisibleZ
                );
            }

            while (player != null && Vector2.Distance(runner.position, player.position) > 0.1f)
            {
                runner.position = Vector2.MoveTowards(
                    runner.position,
                    player.position,
                    runnerSpeed * Time.deltaTime
                );

                yield return null;
            }
        }

        if (blackScreen != null)
        {
            blackScreen.gameObject.SetActive(true);
            blackScreen.transform.SetAsLastSibling();
            SetImageAlpha(blackScreen, 1f);
        }
    }

    void SetImageAlpha(Image image, float alpha)
    {
        Color color = image.color;
        color.a = alpha;
        image.color = color;
    }
}