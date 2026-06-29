using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(menuName = "Cards/Effects/Trumph", fileName = "FX_Trumph_")]
public class CardEffectTrumph : CardEffect
{
    [Header("52장 스프라이트 순서")]
    public List<Sprite> CardImage = new List<Sprite>();

    [Header("표시용 카드 프리팹")]
    public GameObject defaultCardGo;

    [Header("연출용")]
    [SerializeField] private Sprite cardBackSprite;
    [SerializeField] private float moveDuration = 0.25f;
    [SerializeField] private float spawnDelay = 0.08f;
    [SerializeField] private float flipDelay = 0.15f;
    [SerializeField] private float showTimeAfterFlip = 1.0f;
    [SerializeField] private Vector2 startPosition = new Vector2(0f, -300f);

    private struct TrumpCardInfo
    {
        public string suit;
        public string rankText;
        public int attackValue;
        public int spriteIndex;

        public TrumpCardInfo(string suit, string rankText, int attackValue, int spriteIndex)
        {
            this.suit = suit;
            this.rankText = rankText;
            this.attackValue = attackValue;
            this.spriteIndex = spriteIndex;
        }
    }

    public override IEnumerator Apply(
        BattleContext context,
        ICardTarget target,
        int[] consumedDice,
        Action<bool> onFinished)
    {
        if (!(target is IBattleUnit unit))
        {
            Debug.LogWarning("[Effect] Trumph -> target이 데미지를 받을 수 없음");
            onFinished?.Invoke(false);
            yield break;
        }

        if (CardShowManager.instance == null)
        {
            Debug.LogWarning("[Effect] Trumph -> CardShowManager가 없음");
            onFinished?.Invoke(false);
            yield break;
        }

        if (defaultCardGo == null)
        {
            Debug.LogWarning("[Effect] Trumph -> defaultCardGo가 비어있음");
            onFinished?.Invoke(false);
            yield break;
        }

        if (CardImage == null || CardImage.Count < 52)
        {
            Debug.LogWarning("[Effect] Trumph -> CardImage 52장이 전부 연결되지 않음");
            onFinished?.Invoke(false);
            yield break;
        }

        bool success = false;

        IEnumerator Empty()
        {
            yield return null;
        }

        IEnumerator ShowCard(List<GameObject> cardGos)
        {
            List<TrumpCardInfo> deck = BuildDeck();

            int sum = 0;
            int drawCount = Mathf.Min(5, cardGos.Count, deck.Count);

            List<Sprite> pickedSprites = new List<Sprite>();
            List<Vector2> targetPositions = new List<Vector2>();

            for (int pick = 0; pick < drawCount; pick++)
            {
                int randomIndex = UnityEngine.Random.Range(0, deck.Count);
                TrumpCardInfo picked = deck[randomIndex];
                deck.RemoveAt(randomIndex);

                sum += picked.attackValue;
                pickedSprites.Add(CardImage[picked.spriteIndex]);

                Debug.Log($">> 트럼프 카드: {picked.suit}{picked.rankText} / 공격값: {picked.attackValue}");
            }

            // 1. 생성된 모든 Clone 카드를 먼저 숨기고 시작 위치로 내림
            for (int i = 0; i < drawCount; i++)
            {
                GameObject cardGo = cardGos[i];

                if (cardGo == null)
                {
                    targetPositions.Add(Vector2.zero);
                    continue;
                }

                Image img = cardGo.GetComponent<Image>();
                RectTransform rect = cardGo.GetComponent<RectTransform>();

                if (rect != null)
                {
                    targetPositions.Add(rect.anchoredPosition);
                    rect.anchoredPosition = startPosition;
                }
                else
                {
                    targetPositions.Add(Vector2.zero);
                }

                if (img != null)
                {
                    img.sprite = cardBackSprite;

                    Color color = img.color;
                    color.a = 0f;
                    img.color = color;
                }
            }

            yield return null;

            // 2. 자기 차례인 카드만 보이게 만든 뒤 위로 이동
            for (int i = 0; i < drawCount; i++)
            {
                GameObject cardGo = cardGos[i];

                if (cardGo == null)
                    continue;

                Image img = cardGo.GetComponent<Image>();
                RectTransform rect = cardGo.GetComponent<RectTransform>();

                if (img != null)
                {
                    Color color = img.color;
                    color.a = 1f;
                    img.color = color;
                }

                if (rect != null)
                {
                    PlayTrumpMoveSound();

                    yield return MoveCard(
                        rect,
                        startPosition,
                        targetPositions[i],
                        moveDuration
                    );
                }

                yield return new WaitForSeconds(spawnDelay);
            }

            yield return new WaitForSeconds(moveDuration);

            for (int i = 0; i < drawCount; i++)
            {
                if (cardGos[i] == null)
                    continue;

                Image img = cardGos[i].GetComponent<Image>();

                if (img != null)
                {
                    PlayTrumpFlipSound();
                    img.sprite = pickedSprites[i];
                }

                yield return new WaitForSeconds(flipDelay);
            }

            yield return new WaitForSeconds(showTimeAfterFlip);

            PlayTrumpAttackSound();
            yield return new WaitForSeconds(0.15f);

            unit.TakeDamage(sum);
            Debug.Log($">> 트럼프 총합 데미지: {sum}");

            success = true;
        }

        List<GameObject> displayCards = new List<GameObject>();

        for (int i = 0; i < 5; i++)
            displayCards.Add(defaultCardGo);

        yield return CardShowManager.instance.CardWaitShow(
            Empty,
            ShowCard,
            displayCards,
            30f
        );

        onFinished?.Invoke(success);
    }

    private IEnumerator MoveCard(RectTransform rect, Vector2 start, Vector2 end, float duration)
    {
        float time = 0f;
        rect.anchoredPosition = start;

        while (time < duration)
        {
            time += Time.deltaTime;
            float t = Mathf.Clamp01(time / duration);

            rect.anchoredPosition = Vector2.Lerp(start, end, t);

            yield return null;
        }

        rect.anchoredPosition = end;
    }

    private void PlayTrumpMoveSound()
    {
        if (AudioManager.Instance == null) return;
        if (AudioManager.Instance.skillSounds == null) return;

        AudioManager.Instance.PlaySkillSFX(
            AudioManager.Instance.skillSounds.trumpCardMove
        );
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

    private List<TrumpCardInfo> BuildDeck()
    {
        List<TrumpCardInfo> deck = new List<TrumpCardInfo>(52);

        string[] suits = { "H", "D", "C", "S" };

        for (int suitIndex = 0; suitIndex < suits.Length; suitIndex++)
        {
            int baseIndex = suitIndex * 13;

            for (int n = 2; n <= 10; n++)
            {
                int localIndex = n - 2;

                deck.Add(new TrumpCardInfo(
                    suits[suitIndex],
                    n.ToString(),
                    n,
                    baseIndex + localIndex
                ));
            }

            deck.Add(new TrumpCardInfo(suits[suitIndex], "A", 1, baseIndex + 9));
            deck.Add(new TrumpCardInfo(suits[suitIndex], "J", 11, baseIndex + 10));
            deck.Add(new TrumpCardInfo(suits[suitIndex], "Q", 12, baseIndex + 11));
            deck.Add(new TrumpCardInfo(suits[suitIndex], "K", 13, baseIndex + 12));
        }

        return deck;
    }
}
