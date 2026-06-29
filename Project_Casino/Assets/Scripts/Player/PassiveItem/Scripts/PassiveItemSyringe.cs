using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Syringe", menuName = "Items/Passive/Syringe")]
public class PassiveItemSyringe : PassiveItemData
{
    // public int healAmount = 0;

    public int GetSyringeHeal()
    {
        return currentStock == maxStock ? 20 : 10;
    }

    public override void OnItemEffects()
    {
        
    }
}
