using System;
using System.Collections;
using UnityEngine;

[CreateAssetMenu(menuName = "Cards/Effects/Multiply Roulette Plus")]
public class MultiplyRoulettePlusEffect : CardEffect
{
    public override IEnumerator Apply(
        BattleContext context,
        ICardTarget target,
        int[] consumedDice,
        Action<bool> onFinished)
    {
        // 주사위 합산
        int diceValue = 0;
        foreach (var dice in consumedDice)
            diceValue += dice;
        Debug.Log($"기존 주사위 값: {diceValue}");

        // 안전 체크
        if (RoulettePlusUI.Instance == null)
        {
            Debug.LogError("RoulettePlusUI Instance 없음!");
            onFinished?.Invoke(false);
            yield break;
        }

        int multiplier = 1;

        // 룰렛 실행 - RouletteUI.Spin과 동일하게 yield return으로 대기
        yield return RoulettePlusUI.Instance.Spin((result) =>
        {
            multiplier = result;
        });

        Debug.Log($"룰렛 결과: x{multiplier}");

        // 최종 계산
        int finalValue = diceValue * multiplier;
        Debug.Log($"최종 값: {finalValue}");

        // 타겟 적용
        if (target is IBattleUnit unit)
        {
            int damage = Mathf.Abs(finalValue);

            if (finalValue > 0)
            {
                unit.TakeDamage(damage);
                Debug.Log($"타겟에게 {damage} 데미지");
            }
            else
            {
                context.Player.TakeDamage(damage);
                Debug.Log($"역효과! 플레이어가 {damage} 데미지");
            }

            onFinished?.Invoke(true);
        }
        else
        {
            Debug.LogWarning("타겟이 데미지를 받을 수 없음");
            onFinished?.Invoke(false);
        }
    }
}
