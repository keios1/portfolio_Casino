using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class BigWheelEnemy : EnemyBase
{
    [Header("슬롯 머신 연결")]
    //public SlotMachineManager slotMachine; // 하이어라키에 있는 슬롯매니저를 연결해야 함
    public BigWheelSpin spin; // 하이어라키에 있는 슬롯매니저를 연결해야 함
    private bool isSnipeReady = false;

    public override IEnumerator ExecuteEnemyTurn()
    {
        BigWheelEnemyData wheelData = data as BigWheelEnemyData;
        if (wheelData == null)
        {
            Debug.LogError("BigWheelEnemyData가 연결되지 않았습니다");
            yield break;
        }
        if (spin == null)
        {
            Debug.LogError("BigWheelSpin가 연결되지 않았습니다");
            yield break;
        }

        Debug.Log($"{wheelData.enemyName}의 턴 시작");


        //int finalDamage = CalculateDamage(spin.GetDamage(), default);
        int finalDamage = 0;

        // 여기서 AI 작업 수행
        // 1. is hp under 50% && changed == false? -> C
        // 2. is snipeReady? -> B
        // 3. changed? D
        // 4. A

        bool IsPatternC()
        {
            if (spin.IsChanged) return false;
            if (currentHP / maxHP < 0.5f) return true;
            return false;
        }
        bool IsPatternB()
        {
            return isSnipeReady;
        }

        if (IsPatternC())
        {
            Debug.Log("패턴 C");
            yield return PatternC();
        }
        else if (IsPatternB())
        {
            Debug.Log("패턴 B");
            yield return PatternB();
            finalDamage = spin.GetDamage();
            isSnipeReady = false;
        }
        else
        {
            Debug.Log("패턴 A / D");
            yield return PatternAD();
            Debug.Log("패턴 A / D끝");
            finalDamage = spin.GetDamage();
            isSnipeReady = true;
            //
        }

        if (finalDamage > 0)
        {
            // 🌟 기를 모으는 동작 (Ready) 시작
            ChangeAnimation(EAnimationRequestArgument.ready);
            // ForceChangeSprite(readySprite);

            // ==========================================
            // 패링 타이밍 (0.5초 동안 스페이스바 입력 대기)
            // ==========================================
            float parryTimer = 0f;
            bool parryAttempted = false;

            while (parryTimer < 0.5f)
            {
                if (Input.GetKeyDown(KeyCode.K))
                {
                    parryAttempted = true; // 패링 시도
                    break;
                }
                parryTimer += Time.deltaTime;
                yield return null;
            }

            // ==========================================
            // 패링 결과 처리
            // ==========================================
            if (parryAttempted)
            {
                ParryResult pResult = ParryResult.None;

                // 패링 매니저 호출 후 카드 선택이 끝날 때까지 대기
                yield return StartCoroutine(ParryManager.Instance.StartParrySequence(finalDamage, res => pResult = res));

                if (pResult == ParryResult.Nullify)
                {
                    Debug.Log("[무효화] 적의 공격을 막아냈습니다");
                    ChangeAnimation(EAnimationRequestArgument.idle);
                    // ForceChangeSprite(idleSprite); // 타격 없이 원래 자세로 복귀
                }
                else if (pResult == ParryResult.Reflect)
                {
                    Debug.Log("[반사] 적에게 데미지를 되돌려줍니다");
                    ChangeAnimation(EAnimationRequestArgument.hitParry);
                    TakeParryDamage(finalDamage); // 몬스터 본인이 데미지를 입음
                }
                else
                {
                    // 카드가 없거나 시간 초과 등으로 실패했을 때
                    Debug.Log("[패링 실패] 적의 공격을 그대로 맞습니다");
                    ApplyDamageToPlayer(finalDamage);
                }
                yield return new WaitForSeconds(1.0f);
            }
            else
            {
                // 패링을 아예 시도하지 않고 그냥 맞을 때
                ApplyDamageToPlayer(finalDamage);
                yield return new WaitForSeconds(1.0f);
            }
        }
        else
        {
            // ForceChangeSprite(idleSprite);
            ChangeAnimation(EAnimationRequestArgument.idle);
            yield return new WaitForSeconds(0.5f);
        }

        Debug.Log($"{wheelData.enemyName}의 턴 종료");
    }

    private IEnumerator PatternAD()
    {
        Debug.Log("PatternAD()");
        spin.ShowUI();
        Debug.Log("슬롯 머신 준비 중...");
        yield return new WaitForSeconds(1.0f);

        yield return spin.SpinRandomCoroutine();
        yield return new WaitUntil(() => !spin.IsSpinning);

        yield return new WaitForSeconds(0.5f);

        spin.CloseUI();
        yield return new WaitForSeconds(0.2f);
    }

    private IEnumerator PatternB()
    {
        Debug.Log("PatternB()");
        spin.ShowUI();
        Debug.Log("슬롯 머신 준비 중...");
        yield return new WaitForSeconds(1.0f);

        yield return spin.SpinTargetCoroutine(3);
        yield return new WaitUntil(() => !spin.IsSpinning);

        yield return new WaitForSeconds(0.5f);

        spin.CloseUI();
        yield return new WaitForSeconds(0.2f);
    }

    private IEnumerator PatternC()
    {
        Debug.Log("PatternC()");
        spin.Change();

        yield return new WaitForSeconds(1.0f);
    }
}
