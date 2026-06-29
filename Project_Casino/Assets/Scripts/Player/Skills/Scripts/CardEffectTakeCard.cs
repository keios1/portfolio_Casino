using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(menuName = "Cards/Effects/TakeCard", fileName = "FX_TakeCard_")]
public class CardEffectTakeCard : CardEffect
{
    private bool isCardClickAllows = false;
    private bool isBlocked = false;
    private bool effectSucceeded = false;

    [SerializeField] private Sprite cardBack;
    [SerializeField] private Sprite cardTwo;
    [SerializeField] private Sprite cardMinus;

    private IEnumerator Empty()
    {
        yield return null;
    }

    public override IEnumerator Apply(
        BattleContext context,
        ICardTarget target,
        int[] consumedDice,
        Action<bool> onFinished)
    {
        if (context == null || context.Player == null)
        {
            onFinished?.Invoke(false);
            yield break;
        }

        isCardClickAllows = false;

        int diceValue = 0;
        if (consumedDice != null)
        {
            for (int i = 0; i < consumedDice.Length; i++)
                diceValue += consumedDice[i];
        }

        if (diceValue <= 0)
        {
            onFinished?.Invoke(false);
            yield break;
        }

        if (ButtonBlockerManager.Instance == null || CardShowManager.instance == null)
        {
            Debug.LogWarning("[Effect] TakeCard -> 필요한 매니저가 없음");
            onFinished?.Invoke(false);
            yield break;
        }

        ButtonBlockerManager.Instance.SetBlock(ButtonBlockerManager.Instance.cardPickBlock, true);

        isBlocked = true;
        effectSucceeded = false;

        IEnumerator TakeCard(int clickIndex)
        {
            if (isCardClickAllows)
            {
                isCardClickAllows = false;
                
                PlayTrumpFlipSound();

                RandomSet<int> randomSet = new RandomSet<int>();
                randomSet.Add(2);
                randomSet.Add(-1);

                int result = randomSet.Pop();

                if (CardShowManager.instance.CardButton.Count <= clickIndex)
                {
                    Debug.LogWarning("[Effect] TakeCard -> 클릭한 카드 인덱스가 CardButton 범위를 벗어남");
                    effectSucceeded = false;
                    isBlocked = false;
                    yield break;
                }

                Button clickedButton = CardShowManager.instance.CardButton[clickIndex];
                Image clickedImage = clickedButton.GetComponent<Image>();

                if (clickedImage == null)
                {
                    Debug.LogWarning("[Effect] TakeCard -> 클릭한 카드에 Image 컴포넌트가 없음");
                    effectSucceeded = false;
                    isBlocked = false;
                    yield break;
                }

                switch (result)
                {
                    case 2:
                        clickedImage.sprite = cardTwo;
                        break;

                    case -1:
                        clickedImage.sprite = cardMinus;
                        break;
                }

                yield return new WaitForSeconds(0.8f);

                PlayTrumpAttackSound();

                if (result > 0)
                {
                    int damage = diceValue * result;
                    Debug.Log($">> 타겟 공격받음 {damage}");

                    if (target is IBattleUnit unit)
                    {
                        unit.TakeDamage(damage);
                        effectSucceeded = true;
                    }
                    else
                    {
                        effectSucceeded = false;
                    }
                }
                else
                {
                    int selfDamage = diceValue * -result;
                    Debug.Log($">> 플레이어 공격받음 {selfDamage}");

                    context.Player.TakeDamage(selfDamage);
                    effectSucceeded = true;
                }

                isBlocked = false;
            }

            yield return null;
        }

        CardShowManager.instance.SetTakeCardBack(true);

        isCardClickAllows = true;

        yield return CardShowManager.instance.CardClickShow(2, Empty, TakeCard);

        for (int i = 0; i < CardShowManager.instance.CardButton.Count; i++)
        {
            Button btn = CardShowManager.instance.CardButton[i];

            if (btn == null)
                continue;

            Image img = btn.GetComponent<Image>();
            if (img != null)
                img.sprite = cardBack;

            CardTrumphSingle trump = btn.GetComponent<CardTrumphSingle>();
            if (trump != null)
                trump.HideChild();
        }

        while (isBlocked)
            yield return null;

        CardShowManager.instance.SetTakeCardBack(false);

        ButtonBlockerManager.Instance.SetBlock(ButtonBlockerManager.Instance.cardPickBlock, false);

        onFinished?.Invoke(effectSucceeded);
    }
    private void PlayTrumpFlipSound()
    {
        if (AudioManager.Instance == null) return;
        if (AudioManager.Instance.skillSounds == null) return;

        AudioManager.Instance.PlaySkillSFX(
            AudioManager.Instance.skillSounds.trumpCardFlip
        );
    }

    private void PlayTrumpAttackSound()
    {
        if (AudioManager.Instance == null) return;
        if (AudioManager.Instance.skillSounds == null) return;

        AudioManager.Instance.PlaySkillSFX(
            AudioManager.Instance.skillSounds.trumpAttack
        );
    }
}
