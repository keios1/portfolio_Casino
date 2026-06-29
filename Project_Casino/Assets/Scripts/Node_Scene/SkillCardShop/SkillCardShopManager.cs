using System;
using System.Collections;
using System.Collections.Generic;
//using UnityEditor.Sprites;
using UnityEngine;
using UnityEngine.UIElements;

[Serializable]
public class SkillCardStock
{
    public SkillCardData cardData;  // 카드 데이터
    public int stockCount;          // 현재 재고
    public int maxStockCount;       // 최대 재고
}

/// <summary>
/// 상점 재고 관리 스크립트
/// </summary>
public class SkillCardShopManager : MonoBehaviour
{
    public static SkillCardShopManager Instance { get; private set; }

    [Header("리롤 설정")]
    public int RerollCount = 0; // 리롤 횟수
    private const int BaseRerollCost = 20;
    private const int RerollCostIncrease = 5;

    [Header("재고 데이터")]
    public List<SkillCardStock> cardStocks = new List<SkillCardStock>();

    public List<SkillCardData> currentDisplayedCards = new List<SkillCardData>();

    /// <summary>
    /// 카드 등급별 확률 설정
    /// </summary>
    private Dictionary<CardRarity, float> rarityChances = new Dictionary<CardRarity, float>()
    { {CardRarity.Common, 48f},
        {CardRarity.Rare, 30f },
        {CardRarity.Epic, 15f },
        {CardRarity.Legendary, 5f },
        {CardRarity.Hidden, 2f },
    };    

    private void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

        SetCardStocks();
    }

    /// <summary>
    /// 카드 재고 최대치 초기화
    /// </summary>
    private void SetCardStocks()
    {
        for(int i = 0; i < cardStocks.Count; i++)
        {
            cardStocks[i].stockCount = cardStocks[i].maxStockCount;
        }
    }

    /// <summary>
    /// 리롤 횟수에 따른 비용 계산
    /// </summary>
    /// <returns></returns>
    public int GetCurrentRerollCost() => BaseRerollCost + (RerollCount * RerollCostIncrease);

    public void IncreaseRerollCount() => RerollCount++;

    /// <summary>
    /// 리롤 횟수 초기화 (전투 종료시 호출)
    /// </summary>
    public void ResetRerollCount()
    {
        RerollCount = 0;
        Debug.Log("리롤 횟수 0으로 초기화");
    }

    /// <summary>
    /// 상점에 표시할 카드 4장 리스트로 반환
    /// </summary>
    /// <param name="count">기본 4장</param>
    /// <returns></returns>
    public List<SkillCardData> GetRandomCards(int count = 4)
    {
        currentDisplayedCards.Clear();

        for (int i = 0; i < count; i++)
        {
            SkillCardData picked = GetSingleCardByRarity();
            currentDisplayedCards.Add(picked);
        }

        return currentDisplayedCards;
    }

    /// <summary>
    /// 특정 슬롯의 카드 데이터 업데이트
    /// </summary>
    /// <param name="index"></param>
    /// <param name="newData"></param>
    public void UpdateDisplayedCard(int index, SkillCardData newData)
    {
        if (index >= 0 && index < currentDisplayedCards.Count)
        {
            currentDisplayedCards[index] = newData;
        }
    }

    /// <summary>
    /// 등급 확률에 따라 카드 1장 뽑기
    /// </summary>
    /// <returns></returns>
    public SkillCardData GetSingleCardByRarity()
    {
        return GetSingleCardByRarity(new List<SkillCardData>());
    }

    /// <summary>
    /// 등급 확률에 따라 카드 1장을 뽑습니다. 특정 카드들을 제외할 수 있습니다.
    /// </summary>
    /// <param name="excludeList">제외할 카드 리스트</param>
    /// <returns>뽑힌 카드 데이터 (재고가 없으면 null)</returns>
    public SkillCardData GetSingleCardByRarity(List<SkillCardData> excludeList)
    {
        int maxAttempts = 40;

        while (maxAttempts > 0)
        {
            maxAttempts--;

            // 등급 뽑기
            CardRarity selectedRarity = RollRarity();

            // 해당 등급 중 재고 남은 카드 있는지
            List<SkillCardStock> availableStocks = cardStocks.FindAll(s => s.cardData.rarity == selectedRarity &&
            s.stockCount > 0 &&
            !excludeList.Contains(s.cardData));

            // 재고 없으면 등급 뽑기 다시
            if (availableStocks.Count == 0)
                continue;

            // 재고 있으면 확정
            return SelectCardByWeight(availableStocks);
        }
        return null;
    }

    /// <summary>
    /// 등급별 확률 계산
    /// </summary>
    /// <returns></returns>
    private CardRarity RollRarity()
    {
        float roll = UnityEngine.Random.Range(0f, 100f);
        float cumulative = 0f;

        foreach (var pair in rarityChances)
        {
            cumulative += pair.Value;
            if (roll <= cumulative)
                return pair.Key;
        }

        return CardRarity.Common;
    }

    /// <summary>
    /// 동일 등급 내에서 현재 남은 재고 수량을 가중치로 삼아 최종 카드 1장을 선택합니다.
    /// </summary>
    /// <param name="stocks">후보 카드 재고 리스트</param>
    /// <returns>최종 선택된 카드 데이터</returns>
    private SkillCardData SelectCardByWeight(List<SkillCardStock> stocks)
    {
        int totalStock = 0;
        foreach (var s in stocks) totalStock += s.stockCount;

        int roll = UnityEngine.Random.Range(0, totalStock);
        int cumulative = 0;

        foreach (var s in stocks)
        {
            cumulative += s.stockCount;
            if (roll < cumulative)
            {
                return s.cardData;
            }
        }
        return null;
    }    

    /// <summary>
    /// 카드 재고 차감
    /// </summary>
    public void PurchaseCard(SkillCardData cardData)
    {
        if (cardData == null) return;

        var target = cardStocks.Find(s => s.cardData == cardData);
        if (target != null && target.stockCount > 0)
        {
            target.stockCount--; // 여기서 실제로 차감
            Debug.Log($"{cardData.cardName} 재고 차감됨. 남은 재고: {target.stockCount}");
        }
    }

    /// <summary>
    /// 카드 사용 시 재고 복구
    /// </summary>
    public void ReturnStock(SkillCardData cardData)
    {
        if (cardData == null) return;

        var target = cardStocks.Find(s => s.cardData == cardData);

        if (target == null)
        {
            target = new SkillCardStock
            {
                cardData = cardData,
                stockCount = 0,
                maxStockCount = 9999
            };

            cardStocks.Add(target);
        }

        if (target.stockCount < target.maxStockCount)
        {
            target.stockCount++;

            SkillCardShopUI shopUI = FindObjectOfType<SkillCardShopUI>();
            if (shopUI != null && shopUI.gameObject.activeInHierarchy)
            {
                shopUI.RefillEmptySlotWith(cardData);
            }
        }
    }

    /// <summary>
    ///  특정 카드 현재 재고 반환
    /// </summary>
    /// <param name="cardData"></param>
    /// <returns></returns>
    public int GetStockCount(SkillCardData cardData)
    {
        if (cardData == null) return 0;
        var target = cardStocks.Find(s => s.cardData == cardData);
        return target != null ? target.stockCount : 0;
    }
    /// <summary>
    /// 상점 초기화  - 리롤 횟수 초기화, 현재 진열 카드 리스트 초기화, 카드 재고 초기화
    /// </summary>
    public void ResetShopForNewRun()
    {
        RerollCount = 0;
        currentDisplayedCards.Clear();
        SetCardStocks();
    }
}
