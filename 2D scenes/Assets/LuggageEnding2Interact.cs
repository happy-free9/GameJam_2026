using UnityEngine;

public class LuggageEnding2Interact : MonoBehaviour
{
    [Header("Hover")]
    public Color hoverColor = new Color(1f, 0.85f, 0.25f);

    private Renderer[] renderers;
    private Color[] originalColors;

    void Start()
    {
        renderers = GetComponentsInChildren<Renderer>();
        originalColors = new Color[renderers.Length];

        for (int i = 0; i < renderers.Length; i++)
        {
            originalColors[i] = renderers[i].material.color;
        }
    }

    public void SetHighlight(bool isHighlighted)
    {
        for (int i = 0; i < renderers.Length; i++)
        {
            if (isHighlighted)
                renderers[i].material.color = hoverColor;
            else
                renderers[i].material.color = originalColors[i];
        }
    }

    public void Interact()
    {
        if (Ending2Manager.Instance != null)
        {
            Ending2Manager.Instance.ShowRedHairEvidence();
        }
        else
        {
            Debug.LogWarning("No Ending2Manager found in scene.");
        }
    }
}