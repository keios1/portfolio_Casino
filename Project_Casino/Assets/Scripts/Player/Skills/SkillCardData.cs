using UnityEngine;

public enum CardType
{
    Attack,
    Utility,
    Parrying
}

public enum CardTarget
{
    None,
    Self,
    EnemySingle,
    EnemyAll
}

public enum CardPlayTiming
{
    PlayerTurnOnly,
    AnyTurn
}

public enum CardRarity
{
    Basic,
    Common,
    Rare,
    Epic,
    Legendary,
    Hidden
}

public enum CardUseLimitType
{
    Unlimited, // 전역 무제한
    Limited    // 전역 제한 (maxUsesGlobal 사용)
}

[CreateAssetMenu(menuName = "Cards/Skill Card Data", fileName = "Card_")]
public class SkillCardData : ScriptableObject
{
    [Header("ID")]
    [Min(1)]
    public int id;

    [Header("Sprite")]
    public Sprite cardSprite;

    [Header("Info")]
    public string cardName;
    [TextArea] public string description;

    [Header("Timing")]
    public CardPlayTiming timing = CardPlayTiming.PlayerTurnOnly;

    [Header("Target")]
    public CardTarget target = CardTarget.EnemySingle;

    [Header("Rarity")]
    public CardRarity rarity = CardRarity.Basic;

    [Header("Use Limit")]
    public CardUseLimitType useLimitType = CardUseLimitType.Unlimited;

    [Tooltip("전투 내 사용 제한. 0 이하이면 전투 내 무제한.")]
    [Min(0)]
    public int maxUsesPerBattle = 1;

    [Tooltip("전역 사용 제한. Limited일 때만 의미 있음. 0이면 무제한 취급(현재 로직 기준).")]
    [Min(0)]
    public int durability = 0;

    [Header("Cost")]
    [Min(0)]
    public int diceCost = 1;

    [Tooltip("선택한 주사위 합의 최소값, 0이면 조건 없음.")]
    [Min(0)]
    public int minDiceSum = 0;

    [Header("Effect")]
    public CardEffect effect;

    [Header("Price")]
    public int price = 0;

    [Header("Card Type")]
    public CardType cardType;
}
