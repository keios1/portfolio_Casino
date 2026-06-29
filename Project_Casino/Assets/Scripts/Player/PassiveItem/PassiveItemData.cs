using UnityEngine;

/// <summary>
/// 패시브 아이템 SO
/// </summary>
public abstract class PassiveItemData : ScriptableObject
{
    [Header("ID")]
    public int id;

    [Header("Sprite")]
    public Sprite itemSprite;

    [Header("Info")]
    public string itemName;
    [TextArea] public string itemDescription;

    [Header("Inventory Settings")]
    public int maxStock;
    public int currentStock;

    /// <summary>
    /// 재고 초기화 함수 (최대 갯수로)
    /// </summary>
    public void ResetItemStock() => currentStock = maxStock;

    /// <summary>
    /// 아이템이 획득되었을 때 실행될 함수
    /// </summary>
    public abstract void OnItemEffects();
}
