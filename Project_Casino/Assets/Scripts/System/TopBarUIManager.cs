using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TopBarUIManager : MonoBehaviour
{
    public static TopBarUIManager Instance;

    [Header("UI 연결")]
    [SerializeField] private TextMeshProUGUI hpText;
    [SerializeField] private TextMeshProUGUI coinText;

    [Header("HP Image Fill")]
    [SerializeField] private Image hpFillImage;

    [Header("Fill Sprite")]
    [SerializeField] private Sprite normalHpSprite;
    [SerializeField] private Sprite shieldHpSprite;


    private PlayerRuntimeData runtimeData;

    private int cachedHp;
    private int cachedMaxHp;
    private int cachedShield;

    private void Awake()
    {
        Instance = this;

        // 기존 HP 이미지를 자동 저장
        if (hpFillImage != null)
        {
            normalHpSprite = hpFillImage.sprite;
        }
    }


    private void OnEnable()
    {
        runtimeData = PlayerRuntimeData.Instance;

        if (runtimeData == null)
        {
            Debug.LogWarning("[TopBarUIManager] PlayerRuntimeData 없음");
            return;
        }

        runtimeData.OnHpChanged += UpdateHpUI;
        runtimeData.OnCoinChanged += UpdateCoinUI;

        RefreshNow();
    }


    private void OnDisable()
    {
        if (runtimeData != null)
        {
            runtimeData.OnHpChanged -= UpdateHpUI;
            runtimeData.OnCoinChanged -= UpdateCoinUI;
        }
    }


    public void RefreshNow()
    {
        if (runtimeData == null) return;

        UpdateHpUI(
            runtimeData.currentHp,
            runtimeData.maxHp
        );

        UpdateCoinUI(runtimeData.coin);
    }


    private void UpdateHpUI(int current, int max)
    {
        cachedHp = current;
        cachedMaxHp = max;

        RefreshHpText();

        if (hpFillImage != null)
        {
            hpFillImage.fillAmount =
                max > 0 ? (float)current / max : 0;
        }
    }

    public void UpdateShieldText(int shield)
    {
        cachedShield = Mathf.Max(0, shield);
        RefreshHpText();
    }

    private void RefreshHpText()
    {
        if (hpText == null) return;

        if (cachedShield > 0)
        {
            hpText.text = $"HP: {cachedHp} + {cachedShield} / {cachedMaxHp}";
        }
        else
        {
            hpText.text = $"HP: {cachedHp} / {cachedMaxHp}";
        }
    }

    private void UpdateCoinUI(int coin)
    {
        if (coinText != null)
        {
            coinText.text = coin.ToString();
        }
    }


    public void SetShieldFillMode(bool active)
    {
        if (hpFillImage == null)
            return;


        if (active)
        {
            if (shieldHpSprite != null)
            {
                hpFillImage.sprite = shieldHpSprite;
            }
        }
        else
        {
            if (normalHpSprite != null)
            {
                hpFillImage.sprite = normalHpSprite;
            }
        }
    }
}
