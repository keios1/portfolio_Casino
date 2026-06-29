using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CardTrumphSingle : MonoBehaviour
{
    public Image cardImage;
    public List<Sprite> cardSprite = new List<Sprite>();
    public Dictionary<char, int> wordHead2index = new Dictionary<char, int>()
    {
        {'D', 0}, {'S', 13}, {'C', 26}, {'H', 39}
    };
    public Dictionary<string, int> wordTail2Index = new System.Collections.Generic.Dictionary<string, int>()
    {
        { "A", 0 }, { "2", 1 }, { "3", 2 }, { "4", 3 }, { "5", 4 }, { "6", 5 }, { "7", 6 }, { "8", 7 }, { "9", 8 }, { "10", 9 },
        { "J", 10 }, { "Q", 11 }, { "K", 12 },
        { "1", 0 }, { "11", 10 }, { "12", 11 }, { "13", 12 }
    };
    [SerializeField] GameObject[] wantToHideWhenShow;

    public void HideChild()
    {
        if (wantToHideWhenShow != null && wantToHideWhenShow.Length > 0)
        {
            foreach (GameObject go in wantToHideWhenShow)
            {
                go.SetActive(false);
            }
        }
    }
    public void Show(string word)
    {
        cardImage = gameObject.GetComponent<Image>();
        HideChild();

        Debug.Assert(cardImage != null, "CardTrumphSingle: cardImage is not assigned.");
        Debug.Assert(cardSprite != null, "CardTrumphSingle: cardSprite is not assigned.");
        Debug.Assert(wordTail2Index != null, "CardTrumphSingle: wordTail2Index is not assigned.");
        Debug.Assert(word != null, "CardTrumphSingle: word is not assigned.");
        cardImage.sprite = cardSprite[wordHead2index[word[0]] + wordTail2Index[word.Substring(1)]];
    }

    private void Awake()
    {
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
