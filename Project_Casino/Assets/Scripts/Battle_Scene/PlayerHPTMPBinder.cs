using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Player의 HP 데이터를 UI에 바인딩하는 클래스.
/// HP 변화 이벤트를 구독하여 텍스트와 이미지 게이지를 갱신한다.
/// </summary>
public class PlayerHpUIBinder : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private Player player;

    [Header("Text (Optional)")]
    [SerializeField] private TMP_Text hpText;
    [SerializeField] private string prefix = "HP: ";

    [Header("Image Fill (Optional)")]
    [SerializeField] private Image hpFillImage;

    private void OnEnable()
    {
        if (player != null)
            player.OnHpChanged += HandleHpChanged;

        RefreshNow();
    }

    private void OnDisable()
    {
        if (player != null)
            player.OnHpChanged -= HandleHpChanged;
    }

    private void HandleHpChanged(int current, int max)
    {
        if (hpText != null)
            hpText.text = $"{prefix}{current}/{max}";

        if (hpFillImage != null)
        {
            float ratio = (max > 0) ? (float)current / max : 0f;
            hpFillImage.fillAmount = Mathf.Clamp01(ratio);
        }
    }

    private void RefreshNow()
    {
        if (player == null) return;
        HandleHpChanged(player.CurrentHp, player.MaxHp);
    }
}
