using System;
using System.Collections;
using UnityEngine;

[CreateAssetMenu(menuName = "Cards/Effects/Bomb", fileName = "FX_Bomb_")]
public class CardEffectBomb : CardEffect
{
    public override IEnumerator Apply(
        BattleContext context,
        ICardTarget target,
        int[] consumedDice,
        Action<bool> onFinished)
    {
        if (consumedDice == null || consumedDice.Length == 0)
        {
            onFinished?.Invoke(false);
            yield break;
        }

        int sum = 0;
        for (int i = 0; i < consumedDice.Length; i++)
        {
            sum += consumedDice[i] * 2;
        }

        if (target is IBattleUnit unit)
        {
            unit.TakeDamage(sum);
            Debug.Log($"[Effect] Bomb -> {sum}");
            onFinished?.Invoke(true);
        }
        else
        {
            Debug.LogWarning("[Effect] Bomb -> target이 데미지를 받을 수 없음");
            onFinished?.Invoke(false);
        }

        yield return null;
    }
}
