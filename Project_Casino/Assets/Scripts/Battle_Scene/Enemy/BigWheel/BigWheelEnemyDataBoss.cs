using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "BigWheelEnemyDataBoss_", menuName = "Game/Enemy/BigWheel Data/Boss")]
public class BigWheelEnemyDataBoss : BigWheelEnemyData
{
    [Header("보스 BGM")]
    public AudioClip phase2BGM;
    public AudioClip desperationBGM;
    public float bgmFadeTime = 1.5f;

}
