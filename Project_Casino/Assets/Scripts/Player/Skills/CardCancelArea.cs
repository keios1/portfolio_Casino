using UnityEngine;
using UnityEngine.EventSystems;
/// <summary>
/// 카드사용을 취소하기 위한 영역입니다. 카드매니저가 대기중인 상태에서 이 영역을 클릭하면 카드사용이 취소됩니다.
/// </summary>
public class CardCancelArea : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private CardManager cardManager;
    
    private void Awake()
    {
        if (cardManager == null)
            cardManager = FindObjectOfType<CardManager>();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (cardManager != null && cardManager.IsWaitingDiceSelection)
            cardManager.ConfirmCancelByBackground();
    }
}
