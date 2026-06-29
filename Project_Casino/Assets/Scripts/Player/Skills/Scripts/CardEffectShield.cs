using System;
using System.Collections;
using UnityEngine;

[CreateAssetMenu(menuName = "Cards/Effects/Shield", fileName = "FX_Shield_")]
public class CardEffectShield : CardEffect
{
    public override IEnumerator Apply(
        BattleContext context,
        ICardTarget target,
        int[] consumedDice,
        Action<bool> onFinished)
    {
        if (consumedDice == null || consumedDice.Length == 0)
        {
            onFinished?.Invoke(false);
            yield break;
        }

        int shieldValue = 0;

        for (int i = 0; i < consumedDice.Length; i++)
        {
            shieldValue += consumedDice[i];
        }

        if (target is IBattleUnit unit)
        {
            unit.AddShield(shieldValue);
            PlayShieldSound();

            Debug.Log($"[CardEffectShield] Shield +{shieldValue}");

            onFinished?.Invoke(true);
        }
        else
        {
            Debug.LogWarning("[CardEffectShield] target이 IBattleUnit이 아니라 쉴드를 줄 수 없음");
            onFinished?.Invoke(false);
        }

        yield return null;
    }
    private void PlayShieldSound()
    {
        if (AudioManager.Instance == null) return;
        if (AudioManager.Instance.skillSounds == null) return;
        if (AudioManager.Instance.skillSounds.shield == null) return;

        AudioManager.Instance.PlaySkillSFX(
            AudioManager.Instance.skillSounds.shield
        );
    }
}
