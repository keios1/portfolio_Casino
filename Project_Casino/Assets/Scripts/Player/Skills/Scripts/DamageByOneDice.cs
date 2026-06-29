using System;
using System.Collections;
using UnityEngine;

[CreateAssetMenu(menuName = "Cards/Effects/Damage By One Die", fileName = "FX_DamageByOneDie_")]
public class DamageByOneDice : CardEffect
{
    public override IEnumerator Apply(
        BattleContext context,
        ICardTarget target,
        int[] consumedDice,
        Action<bool> onFinished)
    {
        int diceValue = (consumedDice != null && consumedDice.Length > 0) ? consumedDice[0] : 0;

        if (diceValue <= 0)
        {
            onFinished?.Invoke(false);
            yield break;
        }

        if (target is IBattleUnit unit)
        {
            unit.TakeDamage(diceValue);
            Debug.Log($"[Effect] DamageByOneDie -> {diceValue}");
            onFinished?.Invoke(true);
        }
        else
        {
            Debug.LogWarning("[Effect] DamageByOneDie -> target이 데미지를 받을 수 없음");
            onFinished?.Invoke(false);
        }

        yield return null;
    }
}
