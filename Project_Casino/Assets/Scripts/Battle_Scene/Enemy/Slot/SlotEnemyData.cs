using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SlotEnemyData", menuName = "Game/Enemy/Slot Data")]
public class SlotEnemyData : EnemyData
{
    [Header("슬롯 설정")]
    [Range(0, 100)] public int oooChance = 40;
    [Range(0, 100)] public int xxxChance = 30;
    [Range(0, 100)] public int starChance = 30;

    public int oooDamage = 10;
    public int xxxDamage = 0;
    public int starDamage = 30;
}
