using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public enum SortType
{
    None,
    Attack,
    Utility,
    Parrying,
    LowCost,
    HighCost,
    HighestCost
}

/// <summary>
/// 덱 편성 화면 전체를 관리하는 클래스.
/// 보유 카드 목록과 덱 목록을 UI로 생성/갱신하며,
/// 카드 추가/제거 요청을 받아 실제 덱 데이터(PlayerDeckLoadout)에 반영한다.
/// </summary>
/// 
public class EquipmentManager : MonoBehaviour
{
    [Header("Roots")]
    [SerializeField] private Transform ownedCardRoot;
    [SerializeField] private Transform deckCardRoot;

    [Header("Prefabs")]
    [SerializeField] private EquipmentOwnedCardSlot ownedCardSlotPrefab;
    [SerializeField] private EquipmentDeckCardSlot deckCardSlotPrefab;

    [Header("Card Sort State")]
    [Tooltip("어떤 정렬 버튼을 눌렀는지 저장하는 변수 (기본값 : (1순위)공격-유틸-패링 순)")]
    private SortType currentSortType = SortType.None;

    private void OnEnable()
    {
        RefreshAll();
    }

    public void RefreshAll()
    {
        RefreshOwnedCards();
        RefreshDeckCards();
    }

    #region 버튼들이랑 연결
    // { 공격-유틸-패링 정렬 (기본) } 버튼 클릭시 호출
    public void ClickNoneSortButton()
    {
        currentSortType = SortType.Attack;
        RefreshOwnedCards();
    }

    // { 공격 카드 } 버튼 클릭시 호출
    public void ClickAttackSortButton()
    {
        currentSortType = (currentSortType == SortType.Attack) ? SortType.None : SortType.Attack;
        RefreshOwnedCards();
    }

    // { 유틸 카드 } 버튼 클릭시 호출
    public void ClickUtilitySortButton()
    {
        currentSortType = (currentSortType == SortType.Utility) ? SortType.None : SortType.Utility;
        RefreshOwnedCards();
    }

    // { 패링 카드 } 버튼 클릭시 호출
    public void ClickParryingSortButton()
    {
        currentSortType = (currentSortType == SortType.Parrying) ? SortType.None : SortType.Parrying;
        RefreshOwnedCards();
    }

    // { 0 ~ 3 LowCost } 버튼 클릭시 호출
    public void ClickLowCostSortButton()
    {
        currentSortType = (currentSortType == SortType.LowCost) ? SortType.None : SortType.LowCost;
        RefreshOwnedCards();
    }

    // { 4 ~ 6 HighCost } 버튼 클릭시 호출
    public void ClickHighCostSortButton()
    {
        currentSortType = (currentSortType == SortType.HighCost) ? SortType.None : SortType.HighCost;
        RefreshOwnedCards();
    }

    // { 7 ~ HighestCost } 버튼 클릭시 호출
    public void ClickHighestCostSortButton()
    {
        currentSortType = (currentSortType == SortType.HighestCost) ? SortType.None : SortType.HighestCost;
        RefreshOwnedCards();
    }
    #endregion

    // 현재 정렬 규칙에 따라 카드 ui 갱신
    private void RefreshOwnedCards()
    {
        ClearChildren(ownedCardRoot);

        if (PlayerCardCollection.Instance == null) return;

        List<CardOwnedEntry> ownedCards = PlayerCardCollection.Instance.GetOwnedCards();

        // null인 데이터 제외
        var validEntries = ownedCards.Where(entry => entry != null && entry.cardData != null);        

        switch(currentSortType)
        {
            case SortType.Attack:
                validEntries = validEntries.Where(entry => entry.cardData.cardType == CardType.Attack);
                break;
            case SortType.Utility:
                validEntries = validEntries.Where(entry => entry.cardData.cardType == CardType.Utility);
                break;
            case SortType.Parrying:
                validEntries = validEntries.Where(entry => entry.cardData.cardType == CardType.Parrying);
                break;
            case SortType.LowCost:
                validEntries = validEntries.Where(entry => entry.cardData.diceCost >= 0 && entry.cardData.diceCost <= 3);
                break;
            case SortType.HighCost:
                validEntries = validEntries.Where(entry => entry.cardData.diceCost >= 4 && entry.cardData.diceCost <= 6);
                break;
            case SortType.HighestCost:
                validEntries = validEntries.Where(entry => entry.cardData.diceCost >= 7);
                break;

            case SortType.None:
            default:
                break;
        }

        IOrderedEnumerable<CardOwnedEntry> sortedEntries;

        // 공격, 유틸, 패링 카드들 : 저코스트, 가나다 순으로 정렬
        if (currentSortType == SortType.Attack ||
            currentSortType == SortType.Utility ||
            currentSortType == SortType.Parrying)
        {
            sortedEntries = validEntries
                .OrderBy(entry => entry.cardData.diceCost)         // 1순위: 저코스트순
                .ThenBy(entry => entry.cardData.cardName);         // 2순위: 가나다순
        }
        else
        {
            // 기본, 특정 코스트 카드들 : 공격-유틸-패링, 저코스트, 가나다 순으로 정렬
            sortedEntries = validEntries
                .OrderBy(entry => GetTypeSortWeight(entry.cardData.cardType)) // 1순위: 공격-유틸-패링순
                .ThenBy(entry => entry.cardData.diceCost)                     // 2순위: 저코스트순
                .ThenBy(entry => entry.cardData.cardName);                     // 3순위: 가나다순
        }

        foreach (CardOwnedEntry entry in sortedEntries)
        {
            EquipmentOwnedCardSlot slot = Instantiate(ownedCardSlotPrefab, ownedCardRoot);
            slot.Setup(entry.cardData, entry.count, this);
        }
    }

    private void RefreshDeckCards()
    {
        ClearChildren(deckCardRoot);

        if (PlayerDeckLoadout.Instance == null) return;

        List<SkillCardData> deck = PlayerDeckLoadout.Instance.GetCurrentDeck();
        for (int i = 0; i < deck.Count; i++)
        {
            SkillCardData card = deck[i];
            if (card == null) continue;

            EquipmentDeckCardSlot slot = Instantiate(deckCardSlotPrefab, deckCardRoot);
            slot.Setup(card, this);
        }
    }

    public void TryAddToDeck(SkillCardData card)
    {
        if (TutorialManager.Instance != null && TutorialManager.Instance.IsTutorialActive)
        {
            Debug.Log("튜토리얼 중에는 카드를 장착할 수 없습니다");
            return;
        }

        if (PlayerDeckLoadout.Instance == null) return;

        bool added = PlayerDeckLoadout.Instance.AddToDeck(card);
        if (added)
            RefreshAll();
    }

    public void TryRemoveFromDeck(SkillCardData card)
    {
        if (TutorialManager.Instance != null && TutorialManager.Instance.IsTutorialActive)
        {
            Debug.Log("튜토리얼중에는 카드를 해제할 수 없습니다");
            return;
        }

        if (PlayerDeckLoadout.Instance == null) return;

        bool removed = PlayerDeckLoadout.Instance.RemoveFromDeck(card);
        if (removed)
            RefreshAll();
    }

    private void ClearChildren(Transform root)
    {
        if (root == null) return;

        for (int i = root.childCount - 1; i >= 0; i--)
        {
            Destroy(root.GetChild(i).gameObject);
        }
    }

    private int GetTypeSortWeight(CardType type)
    {
        switch (type)
        {
            case CardType.Attack: return 1;
            case CardType.Utility: return 2;
            case CardType.Parrying: return 3;
            default: return 4;
        }
    }
}
