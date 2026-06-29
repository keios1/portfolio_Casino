using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New CloneDice", menuName = "Items/Passive/Clone Dice")]
public class PassiveItemCloneDice : PassiveItemData
{
    [Header("복사 주사위 확률 (0, 1, 2, 3)")]
    public float[] chances = { 0f, 0.2f, 0.5f, 1.0f };

    public bool TryDuplicate()
    {     
        int index = Mathf.Clamp(currentStock, 0, chances.Length - 1);
        float chance = chances[index];

        return Random.value <= chance;
    }

    public override void OnItemEffects()
    {
        
    }
}
