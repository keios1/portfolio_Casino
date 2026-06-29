using System;
using System.Collections;
using UnityEngine;

[CreateAssetMenu(menuName = "Cards/Effects/GameWinner")]
public class GameWinnerEffect : CardEffect
{
    public override IEnumerator Apply(
        BattleContext context,
        ICardTarget target,
        int[] consumedDice,
        Action<bool> onFinished
    )
    {
        Debug.Log("[GameWinner] 발동");

        yield return new WaitForSeconds(0.3f);

        if (context == null || context.Player == null)
        {
            Debug.LogWarning("Player 없음");
            onFinished?.Invoke(false);
            yield break;
        }

        bool isSuccess = false;
        bool waiting = true;

        GameWinnerUI.Instance.Open(result =>
        {
            isSuccess = result;
            waiting = false;
        });

        yield return new WaitUntil(() => !waiting);

        if (isSuccess)
        {
            Debug.Log("[GameWinner] 성공 → 이번 턴 데미지 0");
            context.Player.SetDamageMultiplier(0f);
        }
        else
        {
            Debug.Log("[GameWinner] 실패 → 이번 턴 데미지 2배");
            context.Player.SetDamageMultiplier(2f);
        }

        onFinished?.Invoke(true);
    }
}
