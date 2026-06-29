using System;
using System.Collections.Generic;
/// <summary>
/// 게임의 전체 세이브 데이터를 구조화하여 저장하기 위한 클래스.
/// 플레이어 상태, 카드 컬렉션, 덱 구성, 맵 진행도, 아이템 정보를 포함하며,
/// JSON 직렬화를 통해 파일로 저장/로드된다.
/// </summary>
[Serializable]
public class SaveData
{
    public PlayerSaveData player = new PlayerSaveData();
    public CardCollectionSaveData cardCollection = new CardCollectionSaveData();
    public DeckSaveData deck = new DeckSaveData();

    // 앞으로 추가될 것들
    public MapProgressSaveData mapProgress = new MapProgressSaveData();
    public ItemInventorySaveData itemInventory = new ItemInventorySaveData();
    public TutorialSaveData tutorial = new TutorialSaveData();
    public DiceSystemSaveData diceSystem = new DiceSystemSaveData();
}

[Serializable]
public class PlayerSaveData
{
    public int maxHp = 100;
    public int currentHp = 100;
    public int coin = 100;
    public int coinInSafe = 0;
}

[Serializable]
public class CardCollectionSaveData
{
    public List<CardOwnedSaveEntry> ownedCards = new List<CardOwnedSaveEntry>();
}

[Serializable]
public class CardOwnedSaveEntry
{
    public int cardId;
    public int count;
    public int remainingDurability;
}

[Serializable]
public class DeckSaveData
{
    public List<int> deckCardIds = new List<int>();
}

[Serializable]
public class MapProgressSaveData
{
    public int currentFloor = 0;
    public int currentNodeIndex = -1;
    public int currentUnlockedIndex = 0;
    public List<bool> clearStates = new List<bool>();

    public string lastSceneName = "NodeScene";
    public bool isInBattle = false;

    public int currentBattleSeed = 0;
}

[Serializable]
public class ItemInventorySaveData
{
    public List<int> itemIds = new List<int>();
}

[Serializable]
public class TutorialSaveData
{
    public bool hasCompletedBattleTutorial = false; // 전투 튜토리얼 완료 여부
    public bool hasCompletedNodeTutorial = false;

    public bool hasShownShopTutorial = false;
    public bool hasShownSafeTutorial = false;
    public bool hasShownBloodCenterTutorial = false;
    public bool hasShownBettingTutorial = false;
}
[Serializable]
public class DiceSystemSaveData
{
    public int battleDiceSlotCount = 6;      // 전투 주사위 칸, 최대 12
    public int unlockedDiceEquipSlotCount = 1; // 지급 공간, 최대 6

    public List<int> ownedDiceIds = new List<int>();
    public List<int> equippedDiceIds = new List<int>();
}
