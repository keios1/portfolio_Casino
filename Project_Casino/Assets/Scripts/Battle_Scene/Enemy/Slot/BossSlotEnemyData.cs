using UnityEngine;

[CreateAssetMenu(fileName = "BossSlotEnemyData", menuName = "Game/Enemy/Boss Slot Data")]
public class BossSlotEnemyData : EnemyData
{
    [Header("보스 슬롯 설정")]
    public int bingoDamagePerLine = 10; //1줄 빙고당 10 데미지

    [Header("발악 패턴 설정")]
    public int penaltyDamage = 5;

    [Header("보스 BGM")]
    public AudioClip phase2BGM;
    public AudioClip desperationBGM;
    public float bgmFadeTime = 1.5f;
}
