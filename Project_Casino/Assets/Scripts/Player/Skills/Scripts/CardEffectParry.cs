using System;
using System.Collections;
using UnityEngine;

[CreateAssetMenu(menuName = "Cards/Effects/Parry", fileName = "FX_Parry_")]
public class CardEffectParry : CardEffect
{
    public override IEnumerator Apply(
        BattleContext context,
        ICardTarget target,
        int[] consumedDice,
        Action<bool> onFinished)
    {
        // 이 카드는 내 턴에 주사위를 내고 직접 사용하는 카드가 아님
        // 패링 매니저(ParryManager)가 적 턴에 가로채서 사용하는 카드이므로, 내 턴에 쓰면 막아버립니다.
        Debug.LogWarning("이 카드는 적의 공격 타이밍에만 패링 할 수 있습니다");

        // false를 반환하면 카드가 사용되지 않고(취소됨), 주사위도 소모되지 않습니다.
        onFinished?.Invoke(false);
        yield return null;
    }
}
