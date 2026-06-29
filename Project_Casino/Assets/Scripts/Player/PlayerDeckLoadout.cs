using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 플레이어가 현재 사용할 덱을 구성하고 관리하는 클래스.
/// 보유 카드 수량을 기반으로 덱 편성 가능 여부를 판단하며,
/// 덱 추가/제거 및 초기화 기능을 제공한다.
/// </summary>
public class PlayerDeckLoadout : MonoBehaviour
{
    public static PlayerDeckLoadout Instance { get; private set; }

    [Header("Deck")]
    [SerializeField] private int maxDeckSize = 12;
    [SerializeField] private List<SkillCardData> currentDeck = new List<SkillCardData>();
    [Header("Default Deck")]
    [SerializeField] private List<SkillCardData> defaultEquippedCards = new List<SkillCardData>();
    [Header("Card Catalog")]
    [SerializeField] private List<SkillCardData> cardCatalog = new List<SkillCardData>();

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
        currentDeck.Clear();

        if (SaveManager.Instance == null || SaveManager.Instance.CurrentSaveData == null)
            return;

        List<int> savedDeck = SaveManager.Instance.CurrentSaveData.deck.deckCardIds;

        for (int i = 0; i < savedDeck.Count; i++)
        {
            SkillCardData card = FindCardById(savedDeck[i]);
            if (card != null)
                currentDeck.Add(card);
        }
        //EnsureDefaultDeckCard();
    }

    public void SaveToSaveData()
    {
        if (SaveManager.Instance == null || SaveManager.Instance.CurrentSaveData == null)
            return;

        List<int> savedDeck = SaveManager.Instance.CurrentSaveData.deck.deckCardIds;
        savedDeck.Clear();

        for (int i = 0; i < currentDeck.Count; i++)
        {
            if (currentDeck[i] != null)
                savedDeck.Add(currentDeck[i].id);
        }

        SaveManager.Instance.SaveToFile();
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

    public List<SkillCardData> GetCurrentDeck()
    {
        return new List<SkillCardData>(currentDeck);
    }

    public bool AddToDeck(SkillCardData card)
    {
        if (card == null) return false;
        if (currentDeck.Count >= maxDeckSize) return false;

        int owned = PlayerCardCollection.Instance != null ? PlayerCardCollection.Instance.GetOwnedCount(card) : 0;
        int alreadyInDeck = 0;

        for (int i = 0; i < currentDeck.Count; i++)
        {
            if (currentDeck[i] == card)
                alreadyInDeck++;
        }

        if (alreadyInDeck >= owned) return false;

        currentDeck.Add(card);
        SaveToSaveData();
        return true;
    }

    public bool AddToDeckRuntimeOnly(SkillCardData card)
    {
        if (card == null) return false;
        if (currentDeck.Count >= maxDeckSize) return false;

        int owned = PlayerCardCollection.Instance != null ? PlayerCardCollection.Instance.GetOwnedCount(card) : 0;
        int alreadyInDeck = 0;

        for (int i = 0; i < currentDeck.Count; i++)
        {
            if (currentDeck[i] == card)
                alreadyInDeck++;
        }

        if (alreadyInDeck >= owned) return false;

        currentDeck.Add(card);
        return true;
    }
    private void EnsureDefaultDeckCard()
    {
        bool changed = false;

        for (int i = 0; i < defaultEquippedCards.Count; i++)
        {
            SkillCardData card = defaultEquippedCards[i];

            if (card == null)
                continue;

            if (currentDeck.Contains(card))
                continue;

            if (currentDeck.Count >= maxDeckSize)
                break;

            currentDeck.Add(card);
            changed = true;
        }

        if (changed)
            SaveToSaveData();
    }
    public void ResetToDefaultDeck()
    {
        currentDeck.Clear();

        for (int i = 0; i < defaultEquippedCards.Count; i++)
        {
            SkillCardData card = defaultEquippedCards[i];

            if (card == null)
                continue;

            if (currentDeck.Count >= maxDeckSize)
                break;

            currentDeck.Add(card);
        }

        SaveToSaveData();
    }

    public bool RemoveFromDeck(SkillCardData card)
    {
        if (card == null) return false;

        bool removed = currentDeck.Remove(card);
        if (removed)
            SaveToSaveData();

        return removed;
    }

    public void ClearDeck()
    {
        currentDeck.Clear();
        SaveToSaveData();
    }

    public void ClearDeckRuntimeOnly()
    {
        currentDeck.Clear();
    }
}
