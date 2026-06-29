using System.Collections.Generic;
using UnityEngine;

public class PlayerDiceInventory : MonoBehaviour
{
    public static PlayerDiceInventory Instance { get; private set; }

    public const int MinBattleDiceSlotCount = 6;
    public const int MaxBattleDiceSlotCount = 12;

    public const int MinDiceEquipSlotCount = 1;
    public const int MaxDiceEquipSlotCount = 6;

    [Header("Default")]
    [SerializeField] private DiceItemData defaultDice;

    [Header("Runtime")]
    [SerializeField] private int battleDiceSlotCount = 6;
    [SerializeField] private int unlockedDiceEquipSlotCount = 1;

    [SerializeField] private List<DiceItemData> ownedDice = new List<DiceItemData>();
    [SerializeField] private List<DiceItemData> equippedDice = new List<DiceItemData>();

    public int BattleDiceSlotCount => battleDiceSlotCount;
    public int UnlockedDiceEquipSlotCount => unlockedDiceEquipSlotCount;
    public IReadOnlyList<DiceItemData> OwnedDice => ownedDice;
    public IReadOnlyList<DiceItemData> EquippedDice => equippedDice;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        InitDefaultDice();
    }

    private void InitDefaultDice()
    {
        battleDiceSlotCount = Mathf.Clamp(
            battleDiceSlotCount,
            MinBattleDiceSlotCount,
            MaxBattleDiceSlotCount);

        unlockedDiceEquipSlotCount = Mathf.Clamp(
            unlockedDiceEquipSlotCount,
            MinDiceEquipSlotCount,
            MaxDiceEquipSlotCount);

        if (defaultDice != null && ownedDice.Count == 0)
            ownedDice.Add(defaultDice);

        while (equippedDice.Count < MaxDiceEquipSlotCount)
            equippedDice.Add(null);

        if (equippedDice[0] == null && defaultDice != null)
            equippedDice[0] = defaultDice;
    }

    public bool UpgradeBattleDiceSlot()
    {
        if (battleDiceSlotCount >= MaxBattleDiceSlotCount)
            return false;

        battleDiceSlotCount++;
        return true;
    }

    public bool UpgradeDiceEquipSlot()
    {
        if (unlockedDiceEquipSlotCount >= MaxDiceEquipSlotCount)
            return false;

        unlockedDiceEquipSlotCount++;
        return true;
    }

    public void AddDice(DiceItemData dice)
    {
        if (dice == null) return;
        ownedDice.Add(dice);
    }

    public bool EquipDice(int equipIndex, DiceItemData dice)
    {
        if (dice == null) return false;
        if (equipIndex < 0 || equipIndex >= unlockedDiceEquipSlotCount) return false;
        if (!ownedDice.Contains(dice)) return false;

        while (equippedDice.Count < MaxDiceEquipSlotCount)
            equippedDice.Add(null);

        equippedDice[equipIndex] = dice;
        return true;
    }

    public List<DiceItemData> GetActiveEquippedDice()
    {
        List<DiceItemData> result = new List<DiceItemData>();

        for (int i = 0; i < unlockedDiceEquipSlotCount; i++)
        {
            if (i < equippedDice.Count && equippedDice[i] != null)
                result.Add(equippedDice[i]);
        }

        return result;
    }
}
