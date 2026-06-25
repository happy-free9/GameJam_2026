using UnityEngine;

public class Ending2DInteractSpot : MonoBehaviour
{
    [Header("Visual Highlight")]
    public GameObject spotHighlight;

    private bool playerInside = false;
    private bool hasSat = false;

    void Start()
    {
        if (spotHighlight != null)
        {
            spotHighlight.SetActive(false);
        }
    }

    void Update()
    {
        if (!playerInside)
            return;

        if (hasSat)
            return;

        if (Input.GetKeyDown(KeyCode.E))
        {
            if (Ending2DManager.Instance != null)
            {
                bool sitWorked = Ending2DManager.Instance.TrySitAtSpot(transform);

                if (sitWorked)
                {
                    hasSat = true;

                    if (spotHighlight != null)
                    {
                        spotHighlight.SetActive(true);
                    }
                }
            }
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        Ending2DPlayerController player = other.GetComponent<Ending2DPlayerController>();

        if (player != null)
        {
            playerInside = true;

            if (!hasSat && spotHighlight != null)
            {
                spotHighlight.SetActive(true);
            }

            if (Ending2DManager.Instance != null)
            {
                Ending2DManager.Instance.ShowSitPrompt(true);
            }
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        Ending2DPlayerController player = other.GetComponent<Ending2DPlayerController>();

        if (player != null)
        {
            playerInside = false;

            if (!hasSat && spotHighlight != null)
            {
                spotHighlight.SetActive(false);
            }

            if (Ending2DManager.Instance != null)
            {
                Ending2DManager.Instance.ShowSitPrompt(false);
            }
        }
    }
}