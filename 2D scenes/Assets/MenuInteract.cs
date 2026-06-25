using UnityEngine;

public class MenuInteract : MonoBehaviour
{
    public Sprite menuSprite;
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
            {
                renderers[i].material.color = hoverColor;
            }
            else
            {
                renderers[i].material.color = originalColors[i];
            }
        }
    }

    public void ReadMenu()
    {
        if (MenuPopupUI.Instance != null)
        {
            MenuPopupUI.Instance.ToggleMenu(menuSprite);
        }
    }
}