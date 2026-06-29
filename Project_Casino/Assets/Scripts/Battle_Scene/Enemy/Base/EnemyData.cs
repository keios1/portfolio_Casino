using UnityEngine;

public class EnemyData : ScriptableObject
{
    [Header("공통 기본 정보")]
    public string enemyName;
    public EnemyType enemyType;
    public int maxHP;
    [Header("공통 보상 정보")]
    public int rewardCoin;
}

public enum EnemyType { FistFloor, SecondFloor, SectionBoss }
