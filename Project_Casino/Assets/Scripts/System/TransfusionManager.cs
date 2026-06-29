using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TransfusionManager : MonoBehaviour
{
    public static TransfusionManager instance;

    [Header("UI")]
    public Button bloodToCoinButton;
    public Button coinToBloodButton;
    public TextMeshProUGUI bloodText;
    public TextMeshProUGUI coinText;

    [Header("DEBUG")]
    public bool isDebugging;

    [Header("Exchange Setting")]
    [SerializeField] private int exchangeCost = 10;
    [SerializeField] private int exchangeRatePercent = 80;

    public int CurrentCoin
    {
        get { return PlayerRuntimeData.Instance.coin; }
    }

    public int CurrentHp
    {
        get { return PlayerRuntimeData.Instance.currentHp; }
    }

    public int CoinToBloodCost
    {
        get { return exchangeCost; }
    }

    public int BloodToCoinCost
    {
        get { return exchangeCost; }
    }

    public int ExchangeGain
    {
        get { return (exchangeCost * exchangeRatePercent) / 100; }
    }

    public int PreviewCoinAfterTransfusion
    {
        get { return Mathf.Max(0, CurrentCoin - CoinToBloodCost); }
    }

    public int PreviewHpAfterTransfusion
    {
        get
        {
            int maxHp = PlayerRuntimeData.Instance.maxHp;
            return Mathf.Min(maxHp, CurrentHp + ExchangeGain);
        }
    }

    public int PreviewHpAfterDonation
    {
        get { return Mathf.Max(0, CurrentHp - BloodToCoinCost); }
    }

    public int PreviewCoinAfterDonation
    {
        get { return CurrentCoin + ExchangeGain; }
    }
    // Start is called before the first frame update
    void Start()
    {
        if (PlayerRuntimeData.Instance == null) return;

        UpdateText(PlayerRuntimeData.Instance.coin, PlayerRuntimeData.Instance.currentHp);
        bloodToCoinButton.onClick.AddListener(() =>
        {
            //tutorial code
            if (TutorialManager.Instance != null && TutorialManager.Instance.IsTutorialActive)
            {
                if (!TutorialManager.Instance.TryAction("ClickToCoin")) return;
            }

            int amount = exchangeCost;
            int coinGain = ExchangeGain;

            if (PlayerRuntimeData.Instance.SpendHp(amount))
            {
                PlayerRuntimeData.Instance.AddCoin(coinGain);
            }

            UpdateText(PlayerRuntimeData.Instance.coin, PlayerRuntimeData.Instance.currentHp);


        });
        coinToBloodButton.onClick.AddListener(() =>
        {
            //tutorial code
            if (TutorialManager.Instance != null && TutorialManager.Instance.IsTutorialActive)
            {
                if (!TutorialManager.Instance.TryAction("ClickToBlood")) return;
            }

            int amount = exchangeCost;
            int hpGain = ExchangeGain;

            if (PlayerRuntimeData.Instance.SpendCoin(amount))
            {
                PlayerRuntimeData.Instance.AddHp(hpGain);
            }

            UpdateText(PlayerRuntimeData.Instance.coin, PlayerRuntimeData.Instance.currentHp);
        });
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void BloodToCoin(ref int blood, ref int coin, int amount)
    {
        if (blood - amount <= 0) return;
        blood -= amount;
        coin += (amount * 8) / 10;
    }

    public void CoinToBlood(ref int blood, ref int coin, int amount)
    {
        if (coin - amount < 0) return;
        blood += (amount * 8) / 10;
        coin -= amount;
    }

    public void UpdateText(int coin, int blood)
    {
        bloodText.text = $"blood : {blood}";
        coinText.text = $"coins : {coin}";
    }
}
