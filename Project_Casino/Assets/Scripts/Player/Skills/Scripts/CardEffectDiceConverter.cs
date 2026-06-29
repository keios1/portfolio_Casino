using System;
using System.Collections;
using UnityEngine;

[CreateAssetMenu(menuName = "Cards/Effects/DiceConverter", fileName = "FX_DiceConverter_")]
public class CardEffectDiceConverter : CardEffect
{
    public override IEnumerator Apply(
        BattleContext context,
        ICardTarget target,
        int[] consumedDice,
        Action<bool> onFinished)
    {
        if (context == null || context.Player == null)
        {
            Debug.LogWarning("[Effect] DiceConverter -> context.Player가 없음");
            onFinished?.Invoke(false);
            yield break;
        }

        bool added = context.Player.TryAddRolledDice(6, out int addedIndex);
        if (!added)
        {
            Debug.LogWarning("[Effect] DiceConverter -> 주사위 슬롯이 가득 참");
            onFinished?.Invoke(false);
            yield break;
        }

        Debug.Log($">> 다이스 변화 사용, index={addedIndex}");
        onFinished?.Invoke(true);
        yield return null;
    }
}
