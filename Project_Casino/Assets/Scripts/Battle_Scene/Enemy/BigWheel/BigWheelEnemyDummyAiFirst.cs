using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BigWheelEnemyDummyAiFirst : EnemyBase
{
    public AiBigWheelEnemyFirst ai;

    public override IEnumerator ExecuteEnemyTurn()
    {
        yield return ai.ExecuteEnemyTurn();
    }

    public void ApplyDamageToPlayerPublic(int damage)
    {
        ApplyDamageToPlayer(damage);
    }

    //public override bool IsAlive()
    //{
    //    if (pinky == null)
    //    {
    //        return false;
    //    }
    //    return pinky.IsAlive();
    //}



    // Start is called before the first frame update
    //void Start()
    //{
    //    pinky.hitEvent += HitEvent;
    //}

    // Update is called once per frame
    private void Update()
    {
        // 보스 테스트용 데미지 키 (P)
        if (Input.GetKeyDown(KeyCode.P))
        {
            Debug.Log("[테스트] P키 누름 10 데미지");
            TakeDamage(10);
        }
        if (Input.GetKey(KeyCode.LeftShift) && Input.GetKeyDown(KeyCode.K))
        {
            base.Die();
        }
    }

    private void HitEvent()
    {
        animatorOrNull.Play("Hit");
    }

    public void PublicApplyDamageToPlayer(int damage)
    {
        ApplyDamageToPlayer(damage);
    }

}
