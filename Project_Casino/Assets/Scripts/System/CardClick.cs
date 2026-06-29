using UnityEngine;
using UnityEngine.EventSystems;

public class CardClick : MonoBehaviour, IPointerClickHandler
{
    public int cardIndex;

    public void OnPointerClick(PointerEventData eventData)
    {
        GameWinnerUI.Instance.SelectCard(cardIndex);
    }
}
