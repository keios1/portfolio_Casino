using System;
using System.Collections;
using UnityEngine;

[CreateAssetMenu(menuName = "Cards/Effects/DicePlusTwo")]
public class CardEffectDicePlusTwo : CardEffect
{
    public override IEnumerator Apply(
        BattleContext context,
        ICardTarget target,
        int[] consumedDice,
        Action<bool> onFinished)
    {
        context.Player.AddBonusDiceNextTurn(2);

        BattleManager battleManager = FindObjectOfType<BattleManager>();
        if (battleManager != null)
            battleManager.RefreshNextDicePreview();

        Debug.Log("[Effect] 다음 턴 주사위 +2");

        yield return null;
        onFinished?.Invoke(true);
    }
}
