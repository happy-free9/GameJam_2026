using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class MenuPopupUI : MonoBehaviour
{
    public static MenuPopupUI Instance;

    public Vector2 hiddenPosition = new Vector2(1200f, 0f);
    public Vector2 shownPosition = new Vector2(0f, 0f);
    public float slideSpeed = 8f;

    public bool IsOpen { get; private set; }

    private RectTransform rectTransform;
    private Image menuImage;
    private Coroutine slideCoroutine;
    private bool hasBeenOpened = false;
    private bool closeEventSent = false;

    void Awake()
    {
        Instance = this;

        rectTransform = GetComponent<RectTransform>();
        menuImage = GetComponent<Image>();

        rectTransform.anchoredPosition = hiddenPosition;
        gameObject.SetActive(false);
    }

    public void ToggleMenu(Sprite sprite)
    {
        if (IsOpen)
        {
            HideMenu();
        }
        else
        {
            ShowMenu(sprite);
        }
    }

    public void ShowMenu(Sprite sprite)
    {
        if (sprite != null)
        {
            menuImage.sprite = sprite;
        }

        hasBeenOpened = true;
        gameObject.SetActive(true);
        IsOpen = true;

        SlideTo(shownPosition, false);
    }

    public void HideMenu()
    {
        IsOpen = false;
        SlideTo(hiddenPosition, true);
    }

    private void SlideTo(Vector2 targetPosition, bool hideAfterSlide)
    {
        if (slideCoroutine != null)
        {
            StopCoroutine(slideCoroutine);
        }

        slideCoroutine = StartCoroutine(SlideRoutine(targetPosition, hideAfterSlide));
    }

    private IEnumerator SlideRoutine(Vector2 targetPosition, bool hideAfterSlide)
    {
        while (Vector2.Distance(rectTransform.anchoredPosition, targetPosition) > 0.5f)
        {
            rectTransform.anchoredPosition = Vector2.Lerp(
                rectTransform.anchoredPosition,
                targetPosition,
                Time.deltaTime * slideSpeed
            );

            yield return null;
        }

        rectTransform.anchoredPosition = targetPosition;

        if (hideAfterSlide)
        {
            gameObject.SetActive(false);

            if (hasBeenOpened && !closeEventSent)
            {
                closeEventSent = true;

                if (GameManager.Instance != null)
                {
                    GameManager.Instance.OnMenuClosedAfterReading();
                }
            }
        }
    }
}