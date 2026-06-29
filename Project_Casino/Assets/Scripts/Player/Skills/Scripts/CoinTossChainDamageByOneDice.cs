using System;
using System.Collections;
using UnityEngine;

[CreateAssetMenu(menuName = "Cards/Effects/Coin Toss Chain Damage By One Die", fileName = "FX_CoinTossChainDamageByOneDie_")]
public class CoinTossChainDamageByOneDice : CardEffect
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
            Debug.LogWarning("[Effect] CoinTossChainDamageByOneDice -> CoinTossManager not found.");
            fixedTarget.TakeDamage(diceValue);
            onFinished?.Invoke(true);
            yield break;
        }

        bool finished = false;
        bool success = false;

        toss.OpenSequence(diceValue, (CoinTossSequenceResult result) =>
        {
            fixedTarget.TakeDamage(result.totalDamage); //0으로 데미지가 들어감
            //if (result.totalDamage > 0)
            //{
            //    fixedTarget.TakeDamage(result.totalDamage);
            //}//데미지가 아예 안들어감

            Debug.Log(
                $"[Effect] Coin Toss Chain -> Base:{result.baseDiceValue}, " +
                $"Success:{result.successCount}, FinalDamage:{result.totalDamage}"
            );

            success = true;
            finished = true;
        });

        while (!finished)
            yield return null;

        onFinished?.Invoke(success);
    }
}
