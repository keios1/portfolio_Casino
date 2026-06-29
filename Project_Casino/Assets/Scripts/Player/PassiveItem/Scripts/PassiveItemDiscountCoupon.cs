using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New DiscountCoupon", menuName = "Items/Passive/Discount Coupon")]
public class PassiveItemDiscountCoupon : PassiveItemData
{
    public float GetDiscount()
    {
        return currentStock switch
        {
            1 => 0.2f,
            2 => 0.5f,
            3 => 1.0f,
            _ => 0f
        };
    }

    public override void OnItemEffects() { }
}
