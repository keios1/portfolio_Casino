using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ExplainTooltipView : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Image iconImage;
    [SerializeField] private TMP_Text explainText;

    [Header("Layout")]
    [SerializeField] private LayoutElement iconLayoutElement;
    [SerializeField] private LayoutElement textLayoutElement;
    [SerializeField] private float maxTextWidth = 190f;
    [SerializeField] private Vector2 padding = new Vector2(32f, 24f);
    [Header("Position")]
    [SerializeField] private Vector2 offset = new Vector2(0f, 120f);

    private RectTransform rectTransform;

    private void Awake()
    {
        rectTransform = transform as RectTransform;
        gameObject.SetActive(false);
    }

    public void Show(
        Sprite icon,
        string text,
        RectTransform target,
        Vector2 iconSize
    )
    {
        if (explainText != null)
        {
            explainText.text = text;
            explainText.enableWordWrapping = true;
        }

        if (textLayoutElement != null)
        {
            textLayoutElement.preferredWidth = maxTextWidth;
        }
        bool hasIcon = icon != null;

        if (iconImage != null)
        {
            iconImage.gameObject.SetActive(hasIcon);
            iconImage.sprite = icon;
            iconImage.preserveAspect = true;
        }

        if (iconLayoutElement != null)
        {
            if (hasIcon)
            {
                iconLayoutElement.preferredWidth = iconSize.x;
                iconLayoutElement.preferredHeight = iconSize.y;
            }
            else
            {
                iconLayoutElement.preferredWidth = 0;
                iconLayoutElement.preferredHeight = 0;
            }
        }

        gameObject.SetActive(true);

        LayoutRebuilder.ForceRebuildLayoutImmediate(rectTransform);

        if (target != null)
            rectTransform.position = target.position + (Vector3)offset;
        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(rectTransform);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }
}
