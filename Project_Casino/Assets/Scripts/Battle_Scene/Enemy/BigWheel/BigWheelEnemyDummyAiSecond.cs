using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BigWheelEnemyDummyAiSecond : EnemyBase
{
//    public BigWheelEnemyPinky pinky;
//    public BigWheelEnemyRinky rinky;
    public AiBigWheelEnemySecond ai;

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
    //    return (pinky == null && rinky == null) == false;
    //}



    // Start is called before the first frame update
    void Start()
    {
        //if (pinky != null)
        //{
        //    pinky.hitEvent += HitEvent;
        //}
        //if (rinky != null)
        //{
        //    rinky.hitEvent += HitEvent;
        //}
    }

    // Update is called once per frame
    void Update()
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
