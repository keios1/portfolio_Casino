using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SafeManager : MonoBehaviour
{
    public static SafeManager instance;

    public Button deposit;
    public DigitInput digitInput;


    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        digitInput = GetComponent<DigitInput>();

        deposit.onClick.AddListener(() =>
        {
            Deposit();
        });
    }

    private void Deposit()
    {
        if (TutorialManager.Instance != null && TutorialManager.Instance.IsTutorialActive)
        {
            Debug.Log("튜토리얼 진행 중에는 금고에 입금할 수 없습니다");
            return;

        }
        int value = 0;

        for (int index = 0; index < digitInput.digits.Count; index++)
        {
            value *= 10;
            value += digitInput.digits[index];
        }

        if (value <= PlayerRuntimeData.Instance.coin)
        {
            if (PlayerRuntimeData.Instance.DepositCoinToSafe(value))
            {
                Debug.Log($"금고에 {value}원을 입금했습니다.");
            }
        }

        for (int index = 0; index < digitInput.digits.Count; index++)
        {
            digitInput.digits[index] = 0;
        }
        digitInput.ShowDigit();

    }
}
