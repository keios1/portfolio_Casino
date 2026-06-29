using UnityEngine;
using UnityEngine.EventSystems;

public class ExplainTooltipTrigger : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Tooltip Data")]
    [SerializeField] private Sprite explainImage;
    [SerializeField, TextArea(2, 6)] private string explainText;

    [Header("Icon Option")]
    [SerializeField] private Vector2 iconSize = new Vector2(64f, 64f);

    [Header("Tooltip Prefab")]
    [SerializeField] private ExplainTooltipView tooltipPrefab;

    private ExplainTooltipView spawnedTooltip;
    protected virtual string GetExplainText()
    {
        return explainText;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (tooltipPrefab == null) return;

        if (spawnedTooltip == null)
        {
            Canvas rootCanvas = GetComponentInParent<Canvas>();
            spawnedTooltip = Instantiate(tooltipPrefab, rootCanvas.transform);
        }

        spawnedTooltip.Show(
            explainImage,
            GetExplainText(),
            transform as RectTransform,
            iconSize
        );
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (spawnedTooltip != null)
            spawnedTooltip.Hide();
    }
}
