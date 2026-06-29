using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Underhand", menuName = "Items/Passive/Underhand")]
public class PassiveItemUnderhand : PassiveItemData
{
    [Header("공격 무시 확률(0, 1, 2, 3)")]
    public float[] chances = { 0f, 0.2f, 0.5f };

    public bool IgnoreAttack()
    {     
        return Random.value <= chances[currentStock];
    }

    public override void OnItemEffects()
    {

    }
}
