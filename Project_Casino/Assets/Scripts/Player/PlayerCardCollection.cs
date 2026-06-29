using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 플레이어가 보유한 카드 목록과 개수를 관리하는 싱글톤 클래스.
/// 카드 추가/제거 및 보유 개수 조회 기능을 제공하며,
/// 게임 전반에서 카드 소유 상태를 유지한다.
/// </summary>
[System.Serializable]
public class CardOwnedEntry
{
    public SkillCardData cardData;
    public int count;

    public int remainingDurability;
}

public class PlayerCardCollection : MonoBehaviour
{
    public static PlayerCardCollection Instance { get; private set; }

    [Header("Card Catalog")]
    [SerializeField] private List<SkillCardData> cardCatalog = new List<SkillCardData>();

    [Header("Default Owned Cards")]
    [SerializeField] private List<CardOwnedEntry> defaultOwnedCards = new List<CardOwnedEntry>();

    [SerializeField] private List<CardOwnedEntry> ownedCards = new List<CardOwnedEntry>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadFromSaveData();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void LoadFromSaveData()
    {
        ownedCards.Clear();

        if (SaveManager.Instance != null && SaveManager.Instance.CurrentSaveData != null)
        {
            List<CardOwnedSaveEntry> savedList = SaveManager.Instance.CurrentSaveData.cardCollection.ownedCards;

            for (int i = 0; i < savedList.Count; i++)
            {
                SkillCardData card = FindCardById(savedList[i].cardId);
                if (card == null) continue;

                ownedCards.Add(new CardOwnedEntry
                {
                    cardData = card,
                    count = savedList[i].count,
                    remainingDurability = savedList[i].remainingDurability <= 0
                        ? GetDefaultDurability(card)
                        : savedList[i].remainingDurability
                });
            }
        }

        EnsureDefaultOwnedCards();
    }

    public void SaveToSaveData()
    {
        if (SaveManager.Instance == null || SaveManager.Instance.CurrentSaveData == null)
            return;

        List<CardOwnedSaveEntry> savedList = SaveManager.Instance.CurrentSaveData.cardCollection.ownedCards;
        savedList.Clear();

        for (int i = 0; i < ownedCards.Count; i++)
        {
            if (ownedCards[i].cardData == null) continue;

            savedList.Add(new CardOwnedSaveEntry
            {
                cardId = ownedCards[i].cardData.id,
                count = ownedCards[i].count,
                remainingDurability = ownedCards[i].remainingDurability
            });
        }

        SaveManager.Instance.SaveToFile();
    }

    private void EnsureDefaultOwnedCards()
    {
        bool changed = false;

        for (int i = 0; i < defaultOwnedCards.Count; i++)
        {
            SkillCardData defaultCard = defaultOwnedCards[i].cardData;
            int defaultCount = Mathf.Max(1, defaultOwnedCards[i].count);

            if (defaultCard == null)
                continue;

            CardOwnedEntry entry = ownedCards.Find(x => x.cardData == defaultCard);

            if (entry == null)
            {
                ownedCards.Add(new CardOwnedEntry
                {
                    cardData = defaultCard,
                    count = defaultCount,
                    remainingDurability = GetDefaultDurability(defaultCard)
                });
                changed = true;
            }
            else if (entry.count < defaultCount)
            {
                entry.count = defaultCount;
                changed = true;
            }
        }

        if (changed)
            SaveToSaveData();
    }

    private SkillCardData FindCardById(int id)
    {
        for (int i = 0; i < cardCatalog.Count; i++)
        {
            if (cardCatalog[i] != null && cardCatalog[i].id == id)
                return cardCatalog[i];
        }

        return null;
    }

    public List<CardOwnedEntry> GetOwnedCards()
    {
        return ownedCards;
    }
    private int GetDefaultDurability(SkillCardData card)
    {
        if (card == null)
            return 1;

        return Mathf.Max(1, card.durability);
    }

    public bool ConsumeCardDurability(SkillCardData card)
    {
        if (card == null)
            return false;

        CardOwnedEntry entry = ownedCards.Find(x => x.cardData == card);
        if (entry == null || entry.count <= 0)
            return false;

        if (entry.remainingDurability <= 0)
            entry.remainingDurability = GetDefaultDurability(card);

        entry.remainingDurability--;

        SaveToSaveData();

        return entry.remainingDurability <= 0;
    }
    public int GetRemainingDurability(SkillCardData card)
    {
        if (card == null)
            return 0;

        if (card.useLimitType == CardUseLimitType.Unlimited)
            return 0;

        CardOwnedEntry entry = ownedCards.Find(x => x.cardData == card);

        if (entry == null)
            return Mathf.Max(1, card.durability);

        if (entry.remainingDurability <= 0)
            return Mathf.Max(1, card.durability);

        return entry.remainingDurability;
    }
    public int GetOwnedCount(SkillCardData card)
    {
        if (card == null) return 0;

        CardOwnedEntry entry = ownedCards.Find(x => x.cardData == card);
        return entry != null ? entry.count : 0;
    }

    public void AddCard(SkillCardData card, int amount = 1)
    {
        if (card == null || amount <= 0) return;

        CardOwnedEntry entry = ownedCards.Find(x => x.cardData == card);
        if (entry == null)
        {
            entry = new CardOwnedEntry
            {
                cardData = card,
                count = amount,
                remainingDurability = GetDefaultDurability(card)
            };
            ownedCards.Add(entry);
        }
        else
        {
            entry.count += amount;

            if (entry.remainingDurability <= 0)
                entry.remainingDurability = GetDefaultDurability(card);
        }

        SaveToSaveData();
    }

    public void AddCardRuntimeOnly(SkillCardData card, int amount = 1)
    {
        if (card == null || amount <= 0) return;

        CardOwnedEntry entry = ownedCards.Find(x => x.cardData == card);
        if (entry == null)
        {
            entry = new CardOwnedEntry
            {
                cardData = card,
                count = amount,
                remainingDurability = GetDefaultDurability(card)
            };
            ownedCards.Add(entry);
        }
        else
        {
            entry.count += amount;

            if (entry.remainingDurability <= 0)
                entry.remainingDurability = GetDefaultDurability(card);
        }
    }

    public bool RemoveCard(SkillCardData card, int amount = 1)
    {
        if (card == null || amount <= 0) return false;

        CardOwnedEntry entry = ownedCards.Find(x => x.cardData == card);
        if (entry == null || entry.count < amount) return false;

        entry.count -= amount;

        if (entry.count <= 0)
        {
            ownedCards.Remove(entry);
        }
        else
        {
            entry.remainingDurability = GetDefaultDurability(card);
        }

        SaveToSaveData();
        return true;
    }
}
