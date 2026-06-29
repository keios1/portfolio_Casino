using UnityEngine;
using UnityEngine.EventSystems;

public class CardUseDropArea : MonoBehaviour, IDropHandler
{
    public static CardUseDropArea CurrentHoveredArea { get; private set; }

    public void OnDrop(PointerEventData eventData)
    {
        CardUI cardUI = eventData.pointerDrag?.GetComponent<CardUI>();

        if (cardUI == null)
            return;

        cardUI.TryUseByDropArea();
    }
}
