using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Ending2Manager : MonoBehaviour
{
    public static Ending2Manager Instance;

    [Header("Evidence UI")]
    public GameObject redHairPopup;
    public Image redHairDarkBackground;
    public Image redHairImage;
    public Text redHairSubtitle;

    [TextArea]
    public string redHairText =
        "A chunk of red hair, still wet.\nIt looks like it was forcefully cut off.";

    [Header("Death UI")]
    public Image bloodSplatterImage;
    public Image blackScreen;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip discoverySound;
    public AudioClip deathSound;
    public AudioClip stabbingSound;

    [Header("Timing")]
    public float waitBeforeDeathSound = 2f;
    public float bloodStayTime = 1.5f;
    public float blackFadeDuration = 5f;

    [Header("Next Scene")]
    public string ending2SceneName = "Ending2Scene";

    public bool IsEnding2Active { get; private set; }

    private bool evidenceOpen = false;
    private bool deathSequenceStarted = false;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        IsEnding2Active = false;
        evidenceOpen = false;
        deathSequenceStarted = false;

        if (redHairPopup != null)
        {
            redHairPopup.SetActive(false);
        }

        if (redHairDarkBackground != null)
        {
            redHairDarkBackground.gameObject.SetActive(true);
            redHairDarkBackground.color = new Color(0f, 0f, 0f, 0.7f);
        }

        if (redHairImage != null)
        {
            redHairImage.gameObject.SetActive(true);
            redHairImage.color = new Color(1f, 1f, 1f, 1f);
        }

        if (redHairSubtitle != null)
        {
            redHairSubtitle.gameObject.SetActive(true);
            redHairSubtitle.text = redHairText;
            redHairSubtitle.color = Color.white;
            redHairSubtitle.fontSize = 28;
            redHairSubtitle.alignment = TextAnchor.MiddleCenter;
        }

        if (bloodSplatterImage != null)
        {
            bloodSplatterImage.gameObject.SetActive(false);
            SetImageAlpha(bloodSplatterImage, 1f);
        }

        if (blackScreen != null)
        {
            blackScreen.gameObject.SetActive(true);
            SetImageAlpha(blackScreen, 0f);
        }
    }

    void Update()
    {
        if (!IsEnding2Active)
            return;

        if (evidenceOpen && !deathSequenceStarted && Input.GetKeyDown(KeyCode.E))
        {
            StartCoroutine(AfterEvidenceSequence());
        }
    }

    public void ShowRedHairEvidence()
    {
        if (deathSequenceStarted)
            return;

        IsEnding2Active = true;
        evidenceOpen = true;

        if (blackScreen != null)
        {
            blackScreen.gameObject.SetActive(true);
            SetImageAlpha(blackScreen, 0f);
        }

        if (redHairPopup != null)
        {
            redHairPopup.SetActive(true);
            redHairPopup.transform.SetAsLastSibling();
        }

        if (redHairDarkBackground != null)
        {
            redHairDarkBackground.gameObject.SetActive(true);
            redHairDarkBackground.color = new Color(0f, 0f, 0f, 0.7f);
            redHairDarkBackground.transform.SetAsFirstSibling();
        }

        if (redHairImage != null)
        {
            redHairImage.gameObject.SetActive(true);
            redHairImage.color = new Color(1f, 1f, 1f, 1f);
        }

        if (redHairSubtitle != null)
        {
            redHairSubtitle.gameObject.SetActive(true);
            redHairSubtitle.text = redHairText;
            redHairSubtitle.color = Color.white;
            redHairSubtitle.fontSize = 28;
            redHairSubtitle.alignment = TextAnchor.MiddleCenter;
            redHairSubtitle.transform.SetAsLastSibling();
        }

        if (audioSource != null && discoverySound != null)
        {
            audioSource.PlayOneShot(discoverySound);
        }
    }

    IEnumerator AfterEvidenceSequence()
    {
        deathSequenceStarted = true;
        evidenceOpen = false;

        if (redHairPopup != null)
        {
            redHairPopup.SetActive(false);
        }

        yield return new WaitForSeconds(waitBeforeDeathSound);

        if (audioSource != null && deathSound != null)
        {
            audioSource.PlayOneShot(deathSound);
            yield return new WaitForSeconds(deathSound.length);
        }

        // Blood splatter appears WITH stabbing sound.
        if (audioSource != null && stabbingSound != null)
        {
            audioSource.PlayOneShot(stabbingSound);
        }

        if (bloodSplatterImage != null)
        {
            bloodSplatterImage.gameObject.SetActive(true);
            bloodSplatterImage.transform.SetAsLastSibling();
            SetImageAlpha(bloodSplatterImage, 1f);
        }

        yield return new WaitForSeconds(bloodStayTime);

        if (blackScreen != null)
        {
            blackScreen.gameObject.SetActive(true);
            blackScreen.transform.SetAsLastSibling();
            SetImageAlpha(blackScreen, 0f);

            yield return StartCoroutine(FadeImage(blackScreen, 0f, 1f, blackFadeDuration));
        }

        if (!string.IsNullOrEmpty(ending2SceneName))
        {
            SceneManager.LoadScene(ending2SceneName);
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

    void SetImageAlpha(Image image, float alpha)
    {
        Color color = image.color;
        color.a = alpha;
        image.color = color;
    }
}