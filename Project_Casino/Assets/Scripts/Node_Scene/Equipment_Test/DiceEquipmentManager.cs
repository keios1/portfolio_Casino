using UnityEngine;

public class DiceEquipmentManager : MonoBehaviour
{
    [Header("Equip Slots")]
    [SerializeField] private Transform equipSlotRoot;
    [SerializeField] private DiceEquipSlotUI equipSlotPrefab;

    [Header("Owned Dice")]
    [SerializeField] private Transform ownedSlotRoot;
    [SerializeField] private DiceOwnedSlotUI ownedSlotPrefab;

    private int selectedEquipSlotIndex = -1;

    private void OnEnable()
    {
        Refresh();
    }

    public void Refresh()
    {
        ClearChildren(equipSlotRoot);
        ClearChildren(ownedSlotRoot);

        if (PlayerDiceInventory.Instance == null)
            return;

        for (int i = 0; i < PlayerDiceInventory.MaxDiceEquipSlotCount; i++)
        {
            DiceEquipSlotUI slot = Instantiate(equipSlotPrefab, equipSlotRoot);
            slot.Init(this, i);
        }

        foreach (DiceItemData dice in PlayerDiceInventory.Instance.OwnedDice)
        {
            DiceOwnedSlotUI slot = Instantiate(ownedSlotPrefab, ownedSlotRoot);
            slot.Init(this, dice);
        }
    }

    public void SelectEquipSlot(int index)
    {
        if (PlayerDiceInventory.Instance == null) return;

        if (index >= PlayerDiceInventory.Instance.UnlockedDiceEquipSlotCount)
        {
            Debug.Log("[DiceEquipment] 잠긴 슬롯입니다.");
            return;
        }

        selectedEquipSlotIndex = index;
        Debug.Log($"[DiceEquipment] 장착 슬롯 선택: {index}");
    }

    public void EquipSelectedDice(DiceItemData dice)
    {
        if (PlayerDiceInventory.Instance == null) return;

        if (selectedEquipSlotIndex < 0)
        {
            Debug.Log("[DiceEquipment] 먼저 장착 슬롯을 선택하세요.");
            return;
        }

        bool success = PlayerDiceInventory.Instance.EquipDice(selectedEquipSlotIndex, dice);

        if (success)
        {
            Debug.Log($"[DiceEquipment] {dice.diceName} 장착 완료");
            Refresh();
        }
    }

    private void ClearChildren(Transform root)
    {
        if (root == null) return;

        for (int i = root.childCount - 1; i >= 0; i--)
            Destroy(root.GetChild(i).gameObject);
    }
}
