using UnityEngine;
using System.Collections;

public class SlotEnemy : EnemyBase
{
    [Header("슬롯 머신 연결")]
    public SlotMachineManager slotMachine;
    protected override bool UseSlotDeadSound => true;

    // 패턴을 순서대로 기억할 변수 (0: A, 1: A, 2: B)
    private int patternStep = 0;
    private void Update()
    {
        // 보스 테스트용 데미지 키 (P)
        if (Input.GetKeyDown(KeyCode.P))
        {
            Debug.Log("[테스트] P키 누름 보스에게 10 데미지");
            TakeDamage(10);
        }
    }
    public override IEnumerator ExecuteEnemyTurn()
    {
        SlotEnemyData sData = data as SlotEnemyData;
        if (sData == null)
        {
            Debug.LogError("SlotEnemyData가 연결되지 않았습니다");
            yield break;
        }

        Debug.Log($"<color=yellow>{sData.enemyName}의 턴 시작 (현재 패턴 스텝: {patternStep + 1}/3)</color>");

        // 1. A-A-B 패턴에 따라 결과 결정
        // step 0, 1은 성공(OOO), step 2는 실패(XXX)
        SlotResultType result = (patternStep < 2) ? SlotResultType.OOO : SlotResultType.XXX;
        // 다음 턴을 위해 스텝 증가 (0 -> 1 -> 2 -> 0 무한 반복)
        patternStep = (patternStep + 1) % 3;
        // 2. 슬롯 머신 연출 시작

        // 1. 적 턴 시작
        // 2. 공격 대기 자세
        animatorOrNull.Play("Ready");
        yield return new WaitForSeconds(1.0f);
        // 3. 공격 패널 생성
        if (slotMachine != null)
        {
            slotMachine.ShowUI();
            yield return new WaitForSeconds(1.0f);

            slotMachine.StartSpinning(result);
            yield return new WaitUntil(() => !slotMachine.IsSpinning);

            yield return new WaitForSeconds(0.5f);
            // 4. 공격 패널 끝
            slotMachine.CloseUI();
            yield return new WaitForSeconds(0.2f);
        }

        // 3. 결과에 따른 데미지 처리 (패링 불가)
        int finalDamage = (result == SlotResultType.OOO) ? sData.oooDamage : 0;
        if (finalDamage > 0)
        {
            Debug.Log($"<color=red>[패턴 A - 성공] 플레이어에게 {finalDamage} 데미지 공격!</color>");
            yield return new WaitForSeconds(0.4f);
            animatorOrNull.Play("Attack");
            ApplyDamageToPlayer(finalDamage);
            yield return new WaitForSeconds(1.0f);

        }
        else
        {
            Debug.Log("<color=grey>[패턴 B - 실패] 슬롯 실패! 아무 일도 일어나지 않습니다.</color>");
        }
        animatorOrNull.Play("Idle");


        // 5. 적 공격 모션(만약 데미지 있으면)


        // 6. 플레이어 데미지 적용(만약 데미지 있으면)







        //if (finalDamage > 0)
        //{
        //    Debug.Log($"<color=red>[패턴 A - 성공] 플레이어에게 {finalDamage} 데미지 공격!</color>");
            


        //    yield return new WaitForSeconds(0.4f);

        //    // 패링 없이 바로 데미지를 줍니다!
        //    ApplyDamageToPlayer(finalDamage);
        //    yield return new WaitForSeconds(1.0f);
            

        //    if (animatorOrNull != null)
        //    {
        //        animatorOrNull.Play("Idle");
        //    }
        //    else
        //    {
        //        ForceChangeSprite(idleSprite);
        //    }

        //}
        //else
        //{
        //    Debug.Log("<color=grey>[패턴 B - 실패] 슬롯 실패! 아무 일도 일어나지 않습니다.</color>");

        //    animatorOrNull.Play("Idle");

        //    //ForceChangeSprite(idleSprite);
        //    yield return new WaitForSeconds(1.0f);
        //}

        Debug.Log($"{sData.enemyName}의 턴 종료");
    }
}
