using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class AutoScrollText : MonoBehaviour
{
    [Header("Settings")]
    public float scrollSpeed = 50f;
    public float startDelay = 1.5f;
    public float resetDelay = 1.5f;

    [Header("References")]
    public RectTransform maskRect;
    public TextMeshProUGUI textComponent;

    private RectTransform textRect;
    private float textWidth;
    private float maskWidth;
    private Vector2 initialPos;
    private Coroutine scrollCoroutine;

    void Awake()
    {
        if (textComponent == null) textComponent = GetComponent<TextMeshProUGUI>();
        textRect = textComponent.rectTransform;

        if (maskRect == null && transform.parent != null) maskRect = transform.parent.GetComponent<RectTransform>();

        initialPos = textRect.anchoredPosition;
    }

    void Start()
    {
        CheckAndScroll();
    }

    public void CheckAndScroll()
    {
        Canvas.ForceUpdateCanvases();

        textWidth = textComponent.preferredWidth;
        maskWidth = maskRect.rect.width;

        if (textWidth > maskWidth)
        {
            if (scrollCoroutine != null) StopCoroutine(scrollCoroutine);
            scrollCoroutine = StartCoroutine(ScrollRoutine());
        }
        else
        {
            textRect.anchoredPosition = initialPos;
        }
    }

    IEnumerator ScrollRoutine()
    {
        while (true)
        {
            textRect.anchoredPosition = initialPos;
            yield return new WaitForSeconds(startDelay);

            float distance = textWidth - maskWidth + 50f;
            float duration = distance / scrollSpeed;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float newX = Mathf.Lerp(initialPos.x, initialPos.x - distance, elapsed / duration);
                textRect.anchoredPosition = new Vector2(newX, initialPos.y);
                yield return null;
            }

            yield return new WaitForSeconds(resetDelay);
        }
    }
}