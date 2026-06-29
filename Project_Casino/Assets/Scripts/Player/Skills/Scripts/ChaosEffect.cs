using System;
using System.Collections;
using UnityEngine;

[CreateAssetMenu(menuName = "Cards/Effects/Chaos")]
public class ChaosEffect : CardEffect
{
    public override IEnumerator Apply(
        BattleContext context,
        ICardTarget target,
        int[] consumedDice,
        Action<bool> onFinished
    )
    {
        Debug.Log("카오스 카드 발동");
        int randomIndex = UnityEngine.Random.Range(0, 5); // 6 → 5 (case 5개)
        yield return new WaitForSeconds(0.5f);

        switch (randomIndex)
        {
            // 1️ 모든 주사위 6으로
            case 0:
                Debug.Log("[카오스] 모든 주사위 6으로 변경");
                if (context.Player != null)
                {
                    int[] dice = context.Player.GetDiceSnapshot();
                    for (int i = 0; i < dice.Length; i++)
                        context.Player.TrySetDice(i, 6);
                }
                break;

            // 2️ 도망 + 500G
            case 1:
                Debug.Log("[카오스] 전투 종료 + 500G 지급");
                if (context.Player != null)
                    context.Player.AddCoin(500);
                if (context.BattleManager != null)
                    context.BattleManager.RequestEsacape();
                break;

            // 3️ 체력 +100
            case 2:
                Debug.Log("[카오스] 체력 +100");
                if (context.Player != null)
                    context.Player.Heal(100, false);
                break;

            // 4️ 코인토스
            case 3:
                Debug.Log("[카오스] 코인토스 2회 시작");
                bool hasHead = false;
                int tossCount = 0;
                while (tossCount < 2)
                {
                    bool tossDone = false;
                    CoinTossManager.Instance.Open(result =>
                    {
                        if (result == CoinTossManager.CoinFace.Head)
                            hasHead = true;
                        tossDone = true;
                    });
                    yield return new WaitUntil(() => tossDone);
                    tossCount++;
                    if (hasHead) break;
                }
                if (hasHead)
                {
                    Debug.Log("[카오스] 성공 → 적 HP를 1로");
                    if (context.SelectedEnemy != null)
                    {
                        int damage = context.SelectedEnemy.CurrentHP - 1;
                        if (damage > 0)
                            context.SelectedEnemy.TakeDamage(damage);
                    }
                }
                else
                {
                    Debug.Log("[카오스] 실패 → 플레이어 HP를 1로");
                    if (context.Player != null)
                    {
                        int damage = context.Player.CurrentHp - 1;
                        if (damage > 0)
                            context.Player.TakeDamage(damage);
                    }
                }
                break;

            // 5️ 아무 일도 없음
            case 4:
                Debug.Log("[카오스] 아무 일도 일어나지 않음");
                break;
        }

        onFinished?.Invoke(true);
    }
}
