using TMPro;
using UnityEngine;
using UnityEngine.UI;
/// <summary>
/// 덱에 들어있는 카드 1개의 UI 슬롯.
/// 카드 정보(이름, 설명, 이미지)를 표시하고,
/// 클릭 시 해당 카드를 덱에서 제거하도록 EquipmentManager에 요청한다.
/// </summary>
public class EquipmentDeckCardSlot : MonoBehaviour
{
    [SerializeField] private TMP_Text cardNameText;
    [SerializeField] private TMP_Text descText;
    [SerializeField] private TMP_Text remainText;
    [SerializeField] private TMP_Text maxText;
    [SerializeField] private Image iconImage;
    [SerializeField] private Button button;

    private SkillCardData cardData;
    private EquipmentManager equipmentManager;

    public void Setup(SkillCardData card, EquipmentManager manager)
    {
        cardData = card;
        equipmentManager = manager;

        if (cardNameText != null)
            cardNameText.text = card.cardName;

        if (descText != null)
            descText.text = cardData.description;

        if (iconImage != null)
            iconImage.sprite = card.cardSprite;

        if (button != null)
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(OnClickRemoveFromDeck);
        }
        RefreshDurabilityUI();
    }

    private void OnClickRemoveFromDeck()
    {
        if (equipmentManager == null || cardData == null) return;
        equipmentManager.TryRemoveFromDeck(cardData);
    }
    private void RefreshDurabilityUI()
    {
        if (cardData == null) return;

        if (cardData.useLimitType == CardUseLimitType.Unlimited)
        {
            if (remainText != null) remainText.text = "∞";
            if (maxText != null) maxText.text = "∞";
            return;
        }

        int maxDurability = Mathf.Max(1, cardData.durability);
        int remainDurability = maxDurability;

        PlayerCardCollection collection = PlayerCardCollection.Instance;
        if (collection != null)
            remainDurability = collection.GetRemainingDurability(cardData);

        if (remainText != null) remainText.text = remainDurability.ToString();
        if (maxText != null) maxText.text = maxDurability.ToString();
    }
}
