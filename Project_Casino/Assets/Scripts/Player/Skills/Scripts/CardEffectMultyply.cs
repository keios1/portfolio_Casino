using System;
using System.Collections;
using UnityEngine;

[CreateAssetMenu(menuName = "Cards/Effects/Multyply", fileName = "FX_Multyplyr_")]
public class CardEffectMultyply : CardEffect
{
    public override IEnumerator Apply(
        BattleContext context,
        ICardTarget target,
        int[] consumedDice,
        Action<bool> onFinished)
    {
        Debug.LogWarning("[Effect] Multyply -> 아직 구현되지 않음");
        onFinished?.Invoke(false);
        yield break;
    }
}
