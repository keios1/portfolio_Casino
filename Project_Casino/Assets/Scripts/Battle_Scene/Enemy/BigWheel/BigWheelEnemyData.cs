using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "BigWheelEnemyData", menuName = "Game/Enemy/BigWheel Data")]
public class BigWheelEnemyData : EnemyData
{
    [Header("설정")]
    public int wheelCount = 6;
    public int damage = 15;
}
