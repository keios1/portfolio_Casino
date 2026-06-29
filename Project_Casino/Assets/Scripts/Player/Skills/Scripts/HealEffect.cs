using System;
using System.Collections;
using UnityEngine;

[CreateAssetMenu(menuName = "Cards/Effects/Heal")]
public class HealEffect : CardEffect
{
    private int multiplier = 5;

    public override IEnumerator Apply(
        BattleContext context,
        ICardTarget target,
        int[] consumedDice,
        Action<bool> onFinished
    )
    {
        // 1.주사위 합
        int diceValue = 0;
        foreach (var dice in consumedDice)
            diceValue += dice;

        Debug.Log($"주사위 값: {diceValue}");

        // 2.회복량 계산
        int healAmount = diceValue * multiplier;

        Debug.Log($"회복량: {healAmount}");

        yield return new WaitForSeconds(0.3f);

        // 핵심: Player 직접 접근
        if (context != null && context.Player != null)
        {
            context.Player.Heal(healAmount, false);
            PlayHealSound();
            Debug.Log($"플레이어 회복: {healAmount}");
            onFinished?.Invoke(true);
        }
        else
        {
            Debug.LogWarning("Player 없음");
            onFinished?.Invoke(false);
        }
    }
    private void PlayHealSound()
    {
        if (AudioManager.Instance == null) return;
        if (AudioManager.Instance.skillSounds == null) return;
        if (AudioManager.Instance.skillSounds.healSuccess == null) return;
        AudioManager.Instance.PlaySkillSFX(
            AudioManager.Instance.skillSounds.healSuccess
        );
    }
}
