using System;
using UnityEngine;

public class GameInputManager : MonoBehaviour
{
    public static GameInputManager Instance { get; private set; }

    public event Action OnPausePressed;
    public event Action OnInventoryPressed;
    public event Action OnEndTurnPressed;
    public event Action OnParryPressed;
    public event Action<int> OnDiceSlotPressed;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Update()
    {
        if (KeyBindingManager.IsRebinding)
            return;

        HandlePauseInput();
        HandleInventoryInput();
        HandleBattleInput();
        HandleDiceInput();
        HandleParryInput();
    }

    private void HandlePauseInput()
    {
        if (Input.GetKeyDown(KeyBindingManager.GetKey(InputActionId.Pause, KeyCode.Escape)))
        {
            OnPausePressed?.Invoke();
        }
    }

    private void HandleInventoryInput()
    {
        if (Input.GetKeyDown(KeyBindingManager.GetKey(InputActionId.Inventory, KeyCode.I)))
        {
            OnInventoryPressed?.Invoke();
        }
    }

    private void HandleBattleInput()
    {
        if (Input.GetKeyDown(KeyBindingManager.GetKey(InputActionId.EndTurn, KeyCode.G)))
        {
            OnEndTurnPressed?.Invoke();
        }
    }

    private void HandleDiceInput()
    {
        if (Input.GetKeyDown(KeyBindingManager.GetKey(InputActionId.Dice1, KeyCode.Alpha1)))
            OnDiceSlotPressed?.Invoke(0);

        if (Input.GetKeyDown(KeyBindingManager.GetKey(InputActionId.Dice2, KeyCode.Alpha2)))
            OnDiceSlotPressed?.Invoke(1);

        if (Input.GetKeyDown(KeyBindingManager.GetKey(InputActionId.Dice3, KeyCode.Alpha3)))
            OnDiceSlotPressed?.Invoke(2);

        if (Input.GetKeyDown(KeyBindingManager.GetKey(InputActionId.Dice4, KeyCode.Alpha4)))
            OnDiceSlotPressed?.Invoke(3);

        if (Input.GetKeyDown(KeyBindingManager.GetKey(InputActionId.Dice5, KeyCode.Alpha5)))
            OnDiceSlotPressed?.Invoke(4);

        if (Input.GetKeyDown(KeyBindingManager.GetKey(InputActionId.Dice6, KeyCode.Alpha6)))
            OnDiceSlotPressed?.Invoke(5);
    }

    private void HandleParryInput()
    {
        if (Input.GetKeyDown(KeyBindingManager.GetKey(InputActionId.Parry, KeyCode.Space)))
        {
            OnParryPressed?.Invoke();
        }
    }

    public bool GetActionDown(string actionId, KeyCode defaultKey)
    {
        if (KeyBindingManager.IsRebinding)
            return false;

        return Input.GetKeyDown(KeyBindingManager.GetKey(actionId, defaultKey));
    }
}
