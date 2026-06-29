using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Cheating Dice", menuName = "Items/Passive/Cheating Dice")]
public class PassiveItemCheatingDice : PassiveItemData
{
    public int GetCheatingValue(int step)
    {
        Debug.Log($"사기 주사위 {step}번 눈금 설정");
        int[] faces = currentStock switch
        {
            1 => new int[] { 2, 2, 4, 4, 6, 6 },
            2 => new int[] { 4, 4, 5, 5, 6, 6 },
            3 => new int[] { 10, 10, 10, 10, 10, 10 },
            _ => new int[] { 1, 2, 3, 4, 5, 6 }
        };

        return faces[Random.Range(0, faces.Length)];
    }

    public override void OnItemEffects()
    {
        
    }
}
