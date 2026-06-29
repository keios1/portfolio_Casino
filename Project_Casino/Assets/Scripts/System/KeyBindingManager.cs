using System;
using UnityEngine;

public static class KeyBindingManager
{
    private const string Prefix = "KEY_BINDING_";

    public static bool IsRebinding { get; private set; }

    public static void BeginRebind()
    {
        IsRebinding = true;
    }

    public static void EndRebind()
    {
        IsRebinding = false;
    }

    public static KeyCode GetKey(string actionId, KeyCode defaultKey)
    {
        string savedKey = PlayerPrefs.GetString(Prefix + actionId, defaultKey.ToString());

        if (Enum.TryParse(savedKey, out KeyCode key))
            return key;

        return defaultKey;
    }

    public static void SetKey(string actionId, KeyCode key)
    {
        PlayerPrefs.SetString(Prefix + actionId, key.ToString());
        PlayerPrefs.Save();
    }

    public static void ResetKey(string actionId)
    {
        PlayerPrefs.DeleteKey(Prefix + actionId);
        PlayerPrefs.Save();
    }
}
