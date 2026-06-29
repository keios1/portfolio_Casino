using UnityEngine;

[CreateAssetMenu(menuName = "BlackMarket/Shop Item")]
public class BlackMarketItemData : ScriptableObject
{
    public BlackMarketItemType itemType;

    public string itemName;
    public Sprite icon;
    public int price = 100;
    public int unlockStage = 0;

    public DiceItemData diceItem;
}
