using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 전투 보상 하나의 정보를 담는 데이터 클래스.
/// 보상 종류, 이름, 수량, 아이콘 등을 포함한다.
/// </summary>
public enum RewardType
{
    Coin,
    PassiveItem // Chaos item
}

[System.Serializable]
public class RewardData
{
    public RewardType rewardType;
    public string rewardName;
    public int amount;
    public Sprite rewardIcon;
}
