using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Suit { Heart, Diamond, Club, Spade }

public class BetingCard // : MonoBehaviour
{
    public Suit suit;
    public int value; // 1~13 (A~K)

    public string GetValue()
    {
        string valueStr = value switch
        {
            1 => "A",
            11 => "J",
            12 => "Q",
            13 => "K",
            _ => value.ToString()
        };
        string suitStr = suit switch
        {
            Suit.Heart => "H",
            Suit.Diamond => "D",
            Suit.Club => "C",
            Suit.Spade => "S",
            _ => ""
        };
        return $"{suitStr}{valueStr}";
    }
    public int GetBlackjackValue()
    {
        if (value >= 10) return 10;
        return value;
    }
}
