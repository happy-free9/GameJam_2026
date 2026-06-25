using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class ElevatorDoorInteract : MonoBehaviour
{
    [Header("Hover")]
    public Color hoverColor = new Color(1f, 0.85f, 0.25f);

    [Header("Transition")]
    public Image blackScreen;
    public float fadeDuration = 1.5f;
    public string nextSceneName = "LevelB_Ending2Route";

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip elevatorSound;

    private Renderer[] renderers;
    private Color[] originalColors;
    private bool isEntering = false;

    void Start()
    {
        renderers = GetComponentsInChildren<Renderer>();
        originalColors = new Color[renderers.Length];

        for (int i = 0; i < renderers.Length; i++)
        {
            originalColors[i] = renderers[i].material.color;
        }

        if (blackScreen != null)
        {
            blackScreen.gameObject.SetActive(true);
            SetImageAlpha(blackScreen, 0f);
        }

        if (audioSource != null)
        {
            audioSource.playOnAwake = false;
            audioSource.loop = false;
        }
    }

    public void SetHighlight(bool isHighlighted)
    {
        for (int i = 0; i < renderers.Length; i++)
        {
            if (isHighlighted)
            {
                renderers[i].material.color = hoverColor;
            }
            else
            {
                renderers[i].material.color = originalColors[i];
            }
        }
    }

    public void TryEnterElevator()
    {
        if (isEntering)
            return;

        isEntering = true;

        if (Ending1Manager.Instance != null)
        {
            Ending1Manager.Instance.LockEnding1();
        }

        StartCoroutine(EnterElevatorSequence());
    }

    IEnumerator EnterElevatorSequence()
    {
        float soundLength = 0f;

        if (audioSource != null && elevatorSound != null)
        {
            audioSource.PlayOneShot(elevatorSound);
            soundLength = elevatorSound.length;
        }

        if (blackScreen != null)
        {
            blackScreen.gameObject.SetActive(true);
            blackScreen.transform.SetAsLastSibling();

            yield return StartCoroutine(FadeImage(blackScreen, 0f, 1f, fadeDuration));
        }

        if (soundLength > fadeDuration)
        {
            yield return new WaitForSeconds(soundLength - fadeDuration);
        }

        SceneManager.LoadScene(nextSceneName);
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