using UnityEngine;

[CreateAssetMenu(menuName = "BlackMarket/Dice Item")]
public class DiceItemData : ScriptableObject
{
    public int diceId;
    public string diceName;
    public Sprite icon;
    public int minValue;
    public int maxValue;
    public int price;
    public int unlockStage;

    public int Roll()
    {
        return Random.Range(minValue, maxValue + 1);
    }
}
