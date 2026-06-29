using UnityEngine;
using System.Collections;

public class EnhancedSlotEnemy : EnemyBase
{
    [Header("슬롯 머신 연결")]
    public SlotMachineManager slotMachine;

    private bool isPhase2 = false;
    private int patternStep = 0; // 현재 패턴의 순서를 기억
    protected override bool UseSlotDeadSound => true;

    public void Update()
    {
        if (slotMachine != null)
        {
            if (Input.GetKeyDown(KeyCode.P))
            {
                TakeDamage(10);
            }
        }
    }

    public override IEnumerator ExecuteEnemyTurn()
    {
        SlotEnemyData sData = data as SlotEnemyData;
        if (sData == null) yield break;

        // 페이즈 전환 체크 (체력 50% 이하)
        if (!isPhase2 && currentHP <= data.maxHP * 0.5f)
        {
            isPhase2 = true;
            patternStep = 0; // 페이즈가 넘어가면 패턴 순서를 처음(0)부터 다시 시작
            Debug.Log("[강화 슬롯머신] 체력 50% 이하 2페이즈 패턴(C-D-B-A)");
        }

        if (!isPhase2)
        {
            // 1페이즈: A(일반성공) -> C(실패) -> B(대성공) 순환
            yield return StartCoroutine(ExecutePhase1Pattern());
        }
        else
        {
            // 2페이즈: C(실패) -> D(수리) -> B(대성공) -> A(일반성공) 순환
            yield return StartCoroutine(ExecutePhase2Pattern());
        }
    }

    /// <summary>
    /// 1페이즈 패턴 로직 (A - C - B)
    /// </summary>
    private IEnumerator ExecutePhase1Pattern()
    {
        SlotResultType result = SlotResultType.XXX;
        int damage = 0;
        string patternName = "";

        if (patternStep == 0) { result = SlotResultType.OOO; damage = 15; patternName = "A - 일반성공"; }
        else if (patternStep == 1) { result = SlotResultType.XXX; damage = 0; patternName = "C - 실패"; }
        else if (patternStep == 2) { result = SlotResultType.STAR; damage = 30; patternName = "B - 대성공"; }

        // 순서 1칸 전진 (0 -> 1 -> 2 -> 0 반복)
        patternStep = (patternStep + 1) % 3;

        yield return StartCoroutine(PlaySlotAndApplyEffect(result, damage, 0, patternName));
    }

    /// <summary>
    /// 2페이즈 패턴 로직 (C - D - B - A)
    /// </summary>
    private IEnumerator ExecutePhase2Pattern()
    {
        SlotResultType result = SlotResultType.XXX;
        int damage = 0;
        int heal = 0;
        string patternName = "";

        if (patternStep == 0)
        {
            result = SlotResultType.XXX;
            damage = 0;
            patternName = "C - 실패";
        }
        else if (patternStep == 1)
        {
            result = SlotResultType.CHERRY;
            heal = 5; // 힐량 10 -> 5 수정
            patternName = "D - 수리";
        }
        else if (patternStep == 2)
        {
            result = SlotResultType.STAR;
            damage = 25; // 대성공 딜량 30 -> 25 수정
            patternName = "B - 대성공";
        }
        else if (patternStep == 3)
        {
            result = SlotResultType.OOO;
            damage = 15;
            patternName = "A - 일반성공";
        }

        // 4단계 순환으로 변경 (% 4)
        patternStep = (patternStep + 1) % 4;

        yield return StartCoroutine(PlaySlotAndApplyEffect(result, damage, heal, patternName));
    }

    /// <summary>
    /// 슬롯 연출 및 효과 적용 공통 함수
    /// </summary>
    private IEnumerator PlaySlotAndApplyEffect(SlotResultType result, int damage, int heal, string patternName)
    {
        Debug.Log($"[강화 슬롯머신] {patternName} 실행");

        // 1. 슬롯 돌리기 연출
        if (slotMachine != null)
        {
            animatorOrNull.Play("Ready");
            yield return new WaitForSeconds(0.5f);

            slotMachine.ShowUI();
            yield return new WaitForSeconds(1.0f);

            slotMachine.StartSpinning(result);
            yield return new WaitUntil(() => !slotMachine.IsSpinning);

            yield return new WaitForSeconds(0.5f);
            slotMachine.CloseUI();
            yield return new WaitForSeconds(0.2f);
        }

        // 2. 효과 적용 (패링 불가)
        if (heal > 0)
        {
            currentHP += heal;
            if (currentHP > data.maxHP) currentHP = data.maxHP;

            UpdateHPUI();

            spriteRenderer.material.color = new Color(0, 1, 0);
            animatorOrNull.Play("Heal");
            yield return new WaitForSeconds(0.5f);

            spriteRenderer.material.color = new Color(1, 1, 1);

            Debug.Log($"체력 {heal} 회복 (현재 HP: {currentHP}/{data.maxHP})");
            yield return new WaitForSeconds(1.0f);
        }
        else if (damage > 0)
        {
            yield return new WaitForSeconds(0.4f);

            ApplyDamageToPlayer(damage); // 패링 없이 바로 타격
            animatorOrNull.Play("Attack");

            Debug.Log($"플레이어에게 {damage} 데미지");
            yield return new WaitForSeconds(1.0f);

            animatorOrNull.Play("Idle");
        }
        else
        {
            animatorOrNull.Play("Idle");

            Debug.Log("실패로 인해 아무 효과가 없습니다");
            yield return new WaitForSeconds(1.0f);
        }
    }
}
