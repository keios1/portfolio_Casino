using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DiceOwnedSlotUI : MonoBehaviour
{
    [SerializeField] private Image iconImage;
    [SerializeField] private TMP_Text nameText;

    private DiceItemData dice;
    private DiceEquipmentManager manager;

    public void Init(DiceEquipmentManager owner, DiceItemData data)
    {
        manager = owner;
        dice = data;

        if (iconImage != null)
            iconImage.sprite = dice.icon;

        if (nameText != null)
            nameText.text = dice.diceName;
    }

    public void OnClick()
    {
        if (manager == null) return;
        manager.EquipSelectedDice(dice);
    }
}
