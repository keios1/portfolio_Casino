using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 아이템 소유 여부 및 재고 관리 스크립트
/// </summary>
public class PlayerPassiveItemCollection : MonoBehaviour
{
    public static PlayerPassiveItemCollection Instance {get; private set;}

    [SerializeField] private List<PassiveItemData> items = new List<PassiveItemData>();

    // 현재 플레이어가 가지고 있는 아이템 리스트
    [SerializeField] public List<PassiveItemData> ownedPassives = new List<PassiveItemData>();

    private void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            /*foreach (var item in items)
            {
                if (item != null) item.currentStock = 0;
            }*/
        }
        else
            DestroyImmediate(gameObject);
    }

    public void DrawAndAddPassive()
    {
        // 맥스 재고가 아닌 아이템 중 뽑기
        List<PassiveItemData> availableItems = items.FindAll(x => x.currentStock < x.maxStock);

        if (availableItems.Count <= 0)
            return;

        PassiveItemData selected = availableItems[Random.Range(0, availableItems.Count)];

        AddPassive(selected);
    }

    public void AddPassive(PassiveItemData item)
    {
        if(item == null) return;

        if(item.currentStock == 0)
        {
            ownedPassives.Add(item);
        }

        if(item.currentStock < item.maxStock)
        {
            item.currentStock++;
        }


        item.OnItemEffects();
    }

    public void RemovePassiveItem(PassiveItemData item)
    {
        if (ownedPassives.Contains(item))
        {
            ownedPassives.Remove(item);
        }
    }

    public List<PassiveItemData> GetOwnedPassiveItemes()
    {
        return ownedPassives;
    }

    [ContextMenu("현재 리스트 아이템 강제 적용")]
    public void ForceApplyPassives()
    {
        Debug.Log("아이템 강제 적용 호출");
        foreach (var item in ownedPassives)
        {
            // 0단계라면 효과가 없으므로 최소 1로 설정
            item.currentStock = 2;
            item.OnItemEffects(); // 아이템 효과(체력 증가 등) 강제 실행
        }
    }
}
