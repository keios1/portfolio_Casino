using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DiceEquipSlotUI : MonoBehaviour
{
    [SerializeField] private Image iconImage;
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private GameObject lockObject;

    private int slotIndex;
    private DiceEquipmentManager manager;

    public void Init(DiceEquipmentManager owner, int index)
    {
        manager = owner;
        slotIndex = index;
        Refresh();
    }

    public void Refresh()
    {
        if (PlayerDiceInventory.Instance == null) return;

        bool unlocked = slotIndex < PlayerDiceInventory.Instance.UnlockedDiceEquipSlotCount;
        lockObject.SetActive(!unlocked);

        DiceItemData dice = null;

        if (slotIndex < PlayerDiceInventory.Instance.EquippedDice.Count)
            dice = PlayerDiceInventory.Instance.EquippedDice[slotIndex];

        if (dice != null)
        {
            iconImage.sprite = dice.icon;
            iconImage.enabled = true;
            nameText.text = dice.diceName;
        }
        else
        {
            iconImage.enabled = false;
            nameText.text = unlocked ? "비어 있음" : "잠김";
        }
    }

    public void OnClickSlot()
    {
        if (manager == null) return;
        manager.SelectEquipSlot(slotIndex);
    }
}
