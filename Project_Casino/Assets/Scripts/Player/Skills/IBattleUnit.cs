/// <summary>
/// 전투에 참여하는 유닛의 공통 인터페이스.
/// 피해를 받는 기능을 정의하며, 카드 타겟으로 사용될 수 있다.
/// </summary>
public interface IBattleUnit : ICardTarget
{
    int CurrentShield { get; }

    void TakeDamage(int damage);
    void AddShield(int shield);
    void ClearShield();
}
