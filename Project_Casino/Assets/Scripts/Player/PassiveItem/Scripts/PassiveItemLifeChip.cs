using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New LifeChip", menuName = "Items/Passive/Life Chip")]
public class PassiveItemLifeChip : PassiveItemData
{
    [Header("부활시 HP 회복 양")]
    public float reviveAmount = 0.5f;
    public void Revive(Player player)
    {
        if(currentStock > 0)
        {
            int reviveHP = (int)(player.MaxHp * reviveAmount);
            player.Heal(reviveHP);
            currentStock = 0;
            // maxStock = 0;
            PlayerPassiveItemCollection.Instance.RemovePassiveItem(this);
        }
    }

    public override void OnItemEffects()
    {
        
    }
}
