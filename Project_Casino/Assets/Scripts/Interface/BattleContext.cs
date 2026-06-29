using System.Collections.Generic;
/// <summary>
/// 전투 상황에서 필요한 데이터를 묶어서 전달하는 컨텍스트 클래스.
/// 플레이어, 적 목록, 선택된 타겟 정보를 포함하여
/// 카드 효과 실행 시 공통 데이터로 사용된다.
/// </summary>
public class BattleContext
{
    public Player Player;
    public List<EnemyBase> Enemies;
    public EnemyBase SelectedEnemy;

    public BattleManager BattleManager;
}
