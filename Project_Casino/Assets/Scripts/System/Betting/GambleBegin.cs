using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GambleBegin : MonoBehaviour
{
    public static GambleBegin instance;

    public Button betButton;
    public DigitInput digitInput;
    public GameObject digitInputUi;

    private void Awake()
    {
        instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        digitInput = GetComponent<DigitInput>();

        betButton.onClick.AddListener(() =>
        {
            Bet();
        });
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void Bet()
    {
        if (TutorialManager.Instance != null && TutorialManager.Instance.IsTutorialActive)
        {
            Debug.Log("튜토리얼 진행 중에는 도박을 시작할 수 없습니다");
            return; 
        }

        int value = 0;

        for (int index = 0; index < digitInput.digits.Count; index++)
        {
            value *= 10;
            value += digitInput.digits[index];
        }

        if (value <= PlayerRuntimeData.Instance.coin && value > 0)
        {
            GambleManager.instance.betAmount = value;
            PlayerRuntimeData.Instance.coin -= value;


            Debug.Log($"{value}원을 배팅했습니다.");

            GambleManager.instance.GambleAmount = value;
            StartCoroutine(GambleManager.instance.StartGame());

            digitInputUi.SetActive(false);
        }



        for (int index = 0; index < digitInput.digits.Count; index++)
        {
            digitInput.digits[index] = 0;
        }
        digitInput.ShowDigit();
    }
}
