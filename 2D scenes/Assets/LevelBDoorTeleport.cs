using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class LevelBDoorTeleport : MonoBehaviour
{
    [Header("Teleport")]
    public Transform player;
    public CharacterController playerController;
    public Transform teleportPoint;

    [Header("UI")]
    public Image blackScreen;
    public float fadeDuration = 1.2f;
    public float blackHoldTime = 0.5f;

    [Header("One-Side Interaction")]
    public Transform allowedSideReference;
    public float allowedSideDot = 0f;

    [Header("Hover Highlight")]
    public Color hoverColor = new Color(1f, 0.85f, 0.25f);

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip interactSound;

    private Renderer[] renderers;
    private Color[] originalColors;
    private bool isTransitioning = false;

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

    public bool CanInteractFromPlayerPosition(Vector3 playerPosition)
    {
        // If this is empty, the door can be used from any side.
        if (allowedSideReference == null)
            return true;

        Vector3 directionToPlayer = (playerPosition - allowedSideReference.position).normalized;
        float dot = Vector3.Dot(allowedSideReference.forward, directionToPlayer);

        return dot > allowedSideDot;
    }

    public void TryTeleport()
    {
        if (isTransitioning)
            return;

        if (player == null || teleportPoint == null || blackScreen == null)
        {
            Debug.LogWarning("LevelBDoorTeleport is missing Player, Teleport Point, or Black Screen.");
            return;
        }

        if (!CanInteractFromPlayerPosition(player.position))
        {
            Debug.Log("Door cannot be interacted with from this side.");
            return;
        }

        StartCoroutine(TeleportSequence());
    }

    IEnumerator TeleportSequence()
    {
        isTransitioning = true;

        if (audioSource != null && interactSound != null)
        {
            audioSource.PlayOneShot(interactSound);
        }

        blackScreen.gameObject.SetActive(true);
        blackScreen.transform.SetAsLastSibling();

        yield return StartCoroutine(FadeImage(blackScreen, 0f, 1f, fadeDuration));

        yield return new WaitForSeconds(blackHoldTime);

        if (playerController != null)
            playerController.enabled = false;

        player.position = teleportPoint.position;
        player.rotation = teleportPoint.rotation;

        if (playerController != null)
            playerController.enabled = true;

        yield return StartCoroutine(FadeImage(blackScreen, 1f, 0f, fadeDuration));

        isTransitioning = false;
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