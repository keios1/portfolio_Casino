using System.Collections.Generic;
using UnityEngine;

public class BlackMarketManager : MonoBehaviour
{
    [SerializeField] private Player player;
    [SerializeField] private int currentStage = 0;

    [Header("Shop Items")]
    [SerializeField] private List<BlackMarketItemData> allItems = new List<BlackMarketItemData>();

    public List<BlackMarketItemData> GetUnlockedItems()
    {
        List<BlackMarketItemData> result = new List<BlackMarketItemData>();

        foreach (BlackMarketItemData item in allItems)
        {
            if (item == null) continue;

            if (item.unlockStage <= currentStage)
                result.Add(item);
        }

        return result;
    }

    public bool BuyItem(BlackMarketItemData item)
    {
        if (item == null) return false;
        if (player == null) return false;
        if (PlayerDiceInventory.Instance == null) return false;

        if (!player.SpendCoin(item.price))
        {
            Debug.Log("[BlackMarket] 코인이 부족합니다.");
            return false;
        }

        switch (item.itemType)
        {
            case BlackMarketItemType.BattleDiceSlotUpgrade:
                if (!PlayerDiceInventory.Instance.UpgradeBattleDiceSlot())
                {
                    player.AddCoin(item.price);
                    Debug.Log("[BlackMarket] 주사위 칸이 이미 최대입니다.");
                    return false;
                }
                break;

            case BlackMarketItemType.DiceEquipSlotUpgrade:
                if (!PlayerDiceInventory.Instance.UpgradeDiceEquipSlot())
                {
                    player.AddCoin(item.price);
                    Debug.Log("[BlackMarket] 주사위 지급 공간이 이미 최대입니다.");
                    return false;
                }
                break;

            case BlackMarketItemType.DiceItem:
                PlayerDiceInventory.Instance.AddDice(item.diceItem);
                break;
        }

        Debug.Log($"[BlackMarket] 구매 완료: {item.itemName}");
        return true;
    }
}
