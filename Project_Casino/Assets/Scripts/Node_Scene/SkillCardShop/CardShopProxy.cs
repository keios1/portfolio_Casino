using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// 상점 카드 클릭 감지 프록시 스크립트
/// </summary>
public class CardShopProxy : MonoBehaviour, IPointerClickHandler
{
    private SkillCardSlot ownerSlot;

    public void Setup(SkillCardSlot slot)
    {
        ownerSlot = slot;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        // 슬롯에게 구매 프로세스 시작을 요청
        if (ownerSlot != null)
        {
            ownerSlot.OnClickPurchase();
        }
    }
}
