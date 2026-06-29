using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DigitInput : MonoBehaviour
{
    public List<int> digits = new List<int>() { 0, 0, 0, 0, 0, 0 };

    public List<Button> digitNumberButton;
    public List<GameObject> digitNumberDisplay;

    public List<Sprite> digitImage;


    // Start is called before the first frame update
    void Start()
    {
        for (int index = 0; index < 10; index++)
        {
            int local = index;

            digitNumberButton[local].onClick.AddListener(
                () =>
                {
                    if (TutorialManager.Instance != null && TutorialManager.Instance.IsTutorialActive) return;

                    digits.Add(local);
                    digits.RemoveAt(0);
                    ShowDigit();
                }
                );
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public int GetNum()
    {
        int sum = 0;
        for (int index = 0; index < digits.Count; index++)
        {
            sum += digits[index] * (int)Mathf.Pow(10, 5 - index);
        }
        return sum;
    }

    public void ShowDigit()
    {
        for (int index = 0; index < digitNumberDisplay.Count; index++)
        {
            //digitNumberDisplay[index]
            //    .transform.GetChild(0)
            //    .GetComponent<TextMeshProUGUI>().text = digits[index].ToString();
            digitNumberDisplay[index].GetComponent<Image>().sprite = digitImage[digits[index]];
        }
    }

    public void RemoveDigit()
    {
        if (TutorialManager.Instance != null && TutorialManager.Instance.IsTutorialActive) return;

        digits.Insert(0, 0);
        digits.RemoveAt(6);
        ShowDigit();
    }

    public void ClearDigit()
    {
        if (TutorialManager.Instance != null && TutorialManager.Instance.IsTutorialActive) return;

        digits = new List<int>() { 0, 0, 0, 0, 0, 0 };
        ShowDigit();
    }
}
