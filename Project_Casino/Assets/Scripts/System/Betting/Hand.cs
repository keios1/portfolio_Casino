using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;

public class Hand : MonoBehaviour
{
    public Transform cardPivot;
    public GameObject cardPrefab;

    private List<BetingCard> cards = new List<BetingCard>();


    public void PrepareHand()
    {
        if (cards != null && cards.Count > 0)
        {
            for (int index = cards.Count - 1; index >= 0; index--)
            {
                cards.RemoveAt(index);
                Destroy(transform.GetChild(index).gameObject);
            }
        }

        cards = new List<BetingCard>();
    }

    public IEnumerator AddCard(BetingCard card)
    {
        Debug.Log($">> {gameObject.name} -> Hand.AddCard(BetingCard card) 실행");

        yield return null;

        Debug.Log($">> {gameObject.name} -> Hand.AddCard(BetingCard card) 진입");


        cards.Add(card);

        Vector3 beginPosition = cardPivot.transform.InverseTransformPoint(GambleManager.instance.cardStack.position);
        Vector2 endPosition = new Vector3(cards.Count * 0.5f * 100, 0, 0);
        float timeMax = 0.4f;
        GameObject cardGO = Instantiate(cardPrefab, cardPivot);
        PlayTrumpCardMoveSound();
        cardGO.GetComponent<CardTrumphSingle>().Show(card.GetValue());

        for (float _time = 0f; _time < timeMax; _time += Time.deltaTime)
        {
            cardGO.transform.localPosition = Vector3.Lerp(beginPosition, endPosition, _time / timeMax);

            Debug.Log($">> {gameObject.name} -> Hand.AddCard(BetingCard card) : _time = {_time} / timeMax = {timeMax}");

            yield return null;
        }

        cardGO.transform.localPosition = new Vector3(cards.Count * 0.5f * 100, 0, 0);
        //cardGO.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = card.value.ToString();
    }

    public int GetScore()
    {
        int score = 0;
        int aceCount = 0;

        foreach (BetingCard card in cards)
        {
            score += card.GetBlackjackValue();
            if (card.value == 1) aceCount++;
        }

        // Ace 보정 (1 → 11)
        while (aceCount > 0 && score + 10 <= 21)
        {
            score += 10;
            aceCount--;
        }

        return score;
    }

    public bool IsBust()
    {
        return GetScore() > 21;
    }
    private void PlayTrumpCardMoveSound()
    {
        if (AudioManager.Instance == null) return;
        if (AudioManager.Instance.skillSounds == null) return;
        if (AudioManager.Instance.skillSounds.trumpCardMove == null) return;

        AudioManager.Instance.PlaySkillSFX(
            AudioManager.Instance.skillSounds.trumpCardMove
        );
    }
}
