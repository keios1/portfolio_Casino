using System;
using System.Collections;
using UnityEngine;

[CreateAssetMenu(menuName = "Cards/Effects/Coin Toss Damage By One Die", fileName = "FX_CoinTossDamageByOneDie_")]
public class CoinTossDamageByOneDice : CardEffect
{
    public override IEnumerator Apply(
        BattleContext context,
        ICardTarget target,
        int[] consumedDice,
        Action<bool> onFinished)
    {
        if (!(target is IBattleUnit unit))
        {
            onFinished?.Invoke(false);
            yield break;
        }

        int diceValue = (consumedDice != null && consumedDice.Length > 0) ? consumedDice[0] : 0;
        if (diceValue <= 0)
        {
            onFinished?.Invoke(false);
            yield break;
        }

        IBattleUnit fixedTarget = unit;

        CoinTossManager toss = CoinTossManager.Instance;
        if (toss == null)
        {
            Debug.LogWarning("[Effect] CoinTossDamageByOneDie -> CoinTossManager not found. Apply fallback damage.");
            fixedTarget.TakeDamage(diceValue);
            onFinished?.Invoke(true);
            yield break;
        }

        bool finished = false;
        bool success = false;

        toss.Open((CoinTossManager.CoinFace face) =>
        {
            int mult = (face == CoinTossManager.CoinFace.Head) ? 2 : 0;
            int dmg = diceValue * mult;
            fixedTarget.TakeDamage(dmg);
            //if (dmg > 0)
            //{
            //    fixedTarget.TakeDamage(dmg);
            //}

            Debug.Log($"[Effect] CoinTossDamageByOneDie -> Dice:{diceValue}, Face:{face}, Damage:{dmg}");
            success = true;
            finished = true;
        });

        while (!finished)
            yield return null;

        onFinished?.Invoke(success);
    }
}
