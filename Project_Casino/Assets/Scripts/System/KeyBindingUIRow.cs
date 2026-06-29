using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class KeyBindingUIRow : MonoBehaviour
{
    private static readonly List<KeyBindingUIRow> allRows = new List<KeyBindingUIRow>();

    [Header("Binding Info")]
    [SerializeField] private string actionId;
    [SerializeField] private string actionName;
    [SerializeField] private KeyCode defaultKey = KeyCode.None;

    [Header("UI")]
    [SerializeField] private TMP_Text actionNameText;
    [SerializeField] private TMP_Text keyText;
    [SerializeField] private Button bindButton;

    private KeyCode currentKey;
    private bool isWaitingInput;

    public string ActionId => actionId;
    public KeyCode CurrentKey => currentKey;

    private void OnEnable()
    {
        if (!allRows.Contains(this))
            allRows.Add(this);
    }

    private void OnDisable()
    {
        allRows.Remove(this);

        if (isWaitingInput)
        {
            isWaitingInput = false;
            KeyBindingManager.EndRebind();
        }
    }

    private void Awake()
    {
        if (bindButton != null)
            bindButton.onClick.AddListener(StartRebind);
    }

    private void Start()
    {
        Load();
        RefreshUI();
    }

    private void OnDestroy()
    {
        if (bindButton != null)
            bindButton.onClick.RemoveListener(StartRebind);
    }

    private void Update()
    {
        if (!isWaitingInput)
            return;

        if (!Input.anyKeyDown)
            return;

        KeyCode pressedKey = GetPressedKey();

        if (pressedKey == KeyCode.None)
            return;

        if (IsKeyAlreadyUsed(pressedKey))
        {
            if (keyText != null)
                keyText.text = "이미 사용 중";

            return;
        }

        SetKey(pressedKey);

        isWaitingInput = false;
        KeyBindingManager.EndRebind();
    }

    public void StartRebind()
    {
        isWaitingInput = true;
        KeyBindingManager.BeginRebind();

        if (keyText != null)
            keyText.text = "입력 대기...";
    }

    public void Load()
    {
        currentKey = KeyBindingManager.GetKey(actionId, defaultKey);
    }

    public void SetKey(KeyCode newKey)
    {
        currentKey = newKey;

        KeyBindingManager.SetKey(actionId, currentKey);

        RefreshUI();
    }

    public void ResetToDefault()
    {
        if (IsKeyAlreadyUsed(defaultKey))
        {
            if (keyText != null)
                keyText.text = "기본키 중복";

            return;
        }

        SetKey(defaultKey);
    }

    public void RefreshUI()
    {
        if (actionNameText != null)
            actionNameText.text = actionName;

        if (keyText != null)
            keyText.text = currentKey == KeyCode.None ? "-" : currentKey.ToString();
    }

    private bool IsKeyAlreadyUsed(KeyCode key)
    {
        if (key == KeyCode.None)
            return false;

        foreach (KeyBindingUIRow row in allRows)
        {
            if (row == this)
                continue;

            if (row.CurrentKey == key)
                return true;
        }

        return false;
    }

    private KeyCode GetPressedKey()
    {
        foreach (KeyCode keyCode in Enum.GetValues(typeof(KeyCode)))
        {
            if (keyCode >= KeyCode.Mouse0 && keyCode <= KeyCode.Mouse6)
                continue;

            if (Input.GetKeyDown(keyCode))
                return keyCode;
        }

        return KeyCode.None;
    }
}
