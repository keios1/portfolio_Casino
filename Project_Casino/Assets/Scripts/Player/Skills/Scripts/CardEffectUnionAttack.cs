using System;
using System.Collections;
using UnityEngine;

[CreateAssetMenu(menuName = "Cards/Effects/UnionAttack", fileName = "FX_UnionAttack_")]
public class CardEffectUnionAttack : CardEffect
{
    public override IEnumerator Apply(
        BattleContext context,
        ICardTarget target,
        int[] consumedDice,
        Action<bool> onFinished)
    {
        if (consumedDice == null || consumedDice.Length < 3)
        {
            Debug.LogWarning("[Effect] UnionAttack -> 주사위 3개 필요");
            onFinished?.Invoke(false);
            yield break;
        }

        int damage = (consumedDice[0] + consumedDice[1] + consumedDice[2]) * 3;

        if (target is IBattleUnit unit)
        {
            unit.TakeDamage(damage);
            Debug.Log($">> 유니온 공격 : {damage}");
            onFinished?.Invoke(true);
        }
        else
        {
            Debug.LogWarning("[Effect] UnionAttack -> target이 데미지를 받을 수 없음");
            onFinished?.Invoke(false);
        }

        yield return null;
    }
}
