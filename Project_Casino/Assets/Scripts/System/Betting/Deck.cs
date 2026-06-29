using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Deck : MonoBehaviour
{
    private List<BetingCard> cards = new List<BetingCard>();

    void Awake()
    {
        InitDeck();
        Shuffle();
    }

    public void Prepare()
    {
        InitDeck();
        Shuffle();
    }

    void InitDeck()
    {
        cards.Clear();
        foreach (Suit suit in System.Enum.GetValues(typeof(Suit)))
        {
            for (int i = 1; i <= 13; i++)
            {
                cards.Add(new BetingCard { suit = suit, value = i });
            }
        }
    }

    void Shuffle()
    {
        for (int i = 0; i < cards.Count; i++)
        {
            int rand = Random.Range(0, cards.Count);
            (cards[i], cards[rand]) = (cards[rand], cards[i]);
        }
    }

    public BetingCard Draw()
    {
        BetingCard card = cards[0];
        cards.RemoveAt(0);
        return card;
    }
}
