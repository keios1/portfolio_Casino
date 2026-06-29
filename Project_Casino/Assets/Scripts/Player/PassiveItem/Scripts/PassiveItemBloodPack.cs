using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New BloodPack", menuName = "Items/Passive/Blood Pack")]
[Tooltip("[혈액팩] : 획득시 바로 적용")]
public class PassiveItemBloodPack : PassiveItemData
{
    [Header("단계별 체력 증가량 (0, 1, 2, 3)")]
    public int[] healthBoosts = { 0, 20, 60, 400 }; // 재고 3->20, 2->60, 1->400 (순서 주의)

    public override void OnItemEffects()
    {
        Debug.Log($"혈액팩 효과 시작");
        
        int index = Mathf.Clamp(currentStock, 0, maxStock);
        int amount = healthBoosts[index];

        if (PlayerRuntimeData.Instance != null)
        {
            PlayerRuntimeData.Instance.UpdateMaxHpFromPassive(amount);
        }

        Player player = FindObjectOfType<Player>();
        if (player != null)
        {
            player.ModifyMaxHP(amount);
        }

        Debug.Log($"최대 체력 {amount}로 변경");
    }
}
