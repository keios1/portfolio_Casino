using System.Collections;
using System.Collections.Generic;
//using UnityEditor.ShaderKeywordFilter;
using UnityEngine;

public class AiBigWheelEnemySecond : MonoBehaviour
{
    public class Node
    {
        public System.Func<Node> getNext;
        public System.Func<IEnumerator> function;
    }

    //public BigWheelEnemyPinky pinky;
    //public BigWheelEnemyRinky rinky;
    public BigWheelEnemyDummyAiSecond dummy;
    public BigWheelSpin spin; // 하이어라키에 있는 슬롯매니저를 연결해야 함
    public BigWheelSpinSecond secondSpin;
    public GameObject canvasPannelOutside;
    public GameObject arrow;

    private Node next;
    private bool isDidPatternD = false;

    private Node[] normal;
    private Node[] changed;

    // 이제 프라이빗
    private EnemyBase.AddHPEventArgs addHPEventArgs;

    public IEnumerator ExecuteEnemyTurn()
    {
        Debug.Log($">> AiBigWheelEnemySecond {gameObject.name}에서 실행");

        // 패턴 판단
        // 만약 핑키 사망하고, D패턴을 안 돌림
        // -> D패턴 강제실행 + next를 P1으로 설정
        // 만약 링키 사망하고, D패턴을 안 돌림
        // -> D패턴 강제실행 + next를 R1으로 설정

        // if ((isDidPatternD == false) && (pinky.IsAlive() == false))
        if ((isDidPatternD == false) && (dummy.CurrentHP * 2 < dummy.data.maxHP))
        {
            yield return changed[3].function();
            next = changed[3].getNext();
            isDidPatternD = true;
        }
        else
        {
            yield return next.function();
            next = next.getNext();
        }
        if (dummy.IsAlive())
        {
            dummy.animatorOrNull.Play("Idle");
        }
    }


    // Start is called before the first frame update
    void Start()
    {
        normal = new Node[4];
        normal[0] = new Node
        {
            function = PatternA,
            getNext = () => normal[1]
        };
        normal[1] = new Node
        {
            function = PatternA,
            getNext = () => normal[2]
        };
        normal[2] = new Node
        {
            function = PatternB,
            getNext = () => normal[3]
        };
        normal[3] = new Node
        {
            function = PatternC,
            getNext = () => normal[0]
        };

        changed = new Node[4];
        changed[0] = new Node
        {
            function = PatternE,
            getNext = () => changed[1]
        };
        changed[1] = new Node
        {
            function = PatternE,
            getNext = () => changed[2]
        };
        changed[2] = new Node
        {
            function = PatternB,
            getNext = () => changed[0]
        };
        changed[3] = new Node
        {
            function = PatternD,
            getNext = () => changed[0]
        };

        next = normal[0];
        arrow.SetActive(false);
        canvasPannelOutside.SetActive(false);
        spin.gameObject.SetActive(false);
        secondSpin.gameObject.SetActive(false);

        addHPEventArgs = new EnemyBase.AddHPEventArgs();
        addHPEventArgs.Initialize(dummy.transform.position);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.J))
        {
            //dummy.AddHP(1);
        }
    }

    IEnumerator PatternA()
    {
        Debug.Log("PatternA()");
        canvasPannelOutside.gameObject.SetActive(true);

        spin.ChangeSecondPatternAC();
        spin.ShowUI();
        arrow.SetActive(true);

        Debug.Log("슬롯 머신 준비 중...");
        // pinky.animatorOrNull.Play("Ready");
        dummy.animatorOrNull.Play("Ready");

        yield return new WaitForSeconds(1.0f);

        yield return spin.SpinRandomCoroutine();
        yield return new WaitUntil(() => !spin.IsSpinning);

        yield return new WaitForSeconds(0.5f);

        spin.CloseUI();
        canvasPannelOutside.gameObject.SetActive(false);
        arrow.SetActive(false);
        yield return new WaitForSeconds(0.2f);

        // yield return PlayerParry(spin.GetDamage(), true);
        yield return PlayerParry(spin.GetDamage());
    }

    IEnumerator PatternB() // 힐
    {
        Debug.Log("PatternB()");
        canvasPannelOutside.gameObject.SetActive(true);
        dummy.animatorOrNull.Play("Ready");

        spin.ChangeSecondPatternB();
        spin.ShowUI();
        arrow.SetActive(true);

        Debug.Log("슬롯 머신 준비 중...");

        // rinky.animatorOrNull.Play("Ready");
        yield return new WaitForSeconds(1.0f);

        yield return spin.SpinRandomCoroutine();
        yield return new WaitUntil(() => !spin.IsSpinning);
        // if (rinky != null) rinky.animatorOrNull.Play("Attack");

        yield return new WaitForSeconds(0.5f);

        spin.CloseUI();
        canvasPannelOutside.gameObject.SetActive(false);
        arrow.SetActive(false);
        yield return new WaitForSeconds(0.2f);
        dummy.animatorOrNull.Play("Recover");

        addHPEventArgs.isUsed = false;
        addHPEventArgs.amount = spin.GetDamage();
        WheelHealSound();
        dummy.AddHP(addHPEventArgs);


        yield return new WaitForSeconds(1.0f);
        yield return EnemyActionTiming();
    }

    IEnumerator PatternC()
    {
        Debug.Log("PatternC()");
        canvasPannelOutside.gameObject.SetActive(true);
        spin.ChangeSecondPatternAC();
        spin.ShowUI();
        arrow.SetActive(true);
        Debug.Log("슬롯 머신 준비 중...");
        // pinky.animatorOrNull.Play("Ready");
        // rinky.animatorOrNull.Play("Ready");
        dummy.animatorOrNull.Play("Ready");
        yield return new WaitForSeconds(1.0f);

        yield return spin.SpinTargetCoroutine(3);
        yield return new WaitUntil(() => !spin.IsSpinning);

        yield return new WaitForSeconds(0.5f);

        spin.CloseUI();
        arrow.SetActive(false);
        canvasPannelOutside.gameObject.SetActive(false);
        yield return new WaitForSeconds(0.2f);

        yield return PlayerParry(spin.GetDamage());
        // yield return PlayerParry(spin.GetDamage(), true);
    }

    IEnumerator PatternD()
    {
        // 패링 없음
        dummy.animatorOrNull.Play("Ready");
        yield return new WaitForSeconds(1.0f);
        dummy.animatorOrNull.Play("Idle");

        Debug.Log(">> PatternD()");
        yield return EnemyActionTiming();
    }

    IEnumerator PatternE()
    {
        Debug.Log("PatternE()");
        canvasPannelOutside.gameObject.SetActive(true);
        spin.ShowUI();
        secondSpin.ShowUI();
        spin.ChangeSecondPatternE();
        arrow.SetActive(true);
        Debug.Log("슬롯 머신 준비 중...");
        // if (pinky != null) pinky.animatorOrNull.Play("Ready");
        // if (rinky != null) rinky.animatorOrNull.Play("Ready");
        dummy.animatorOrNull.Play("Ready");
        yield return new WaitForSeconds(1.0f);

        yield return spin.SpinRandomCoroutine();
        StartCoroutine(secondSpin.SpinRandomCoroutine()); // 별도 코루틴 실행
        yield return new WaitUntil(() => (!spin.IsSpinning && !secondSpin.IsSpinning));

        yield return new WaitForSeconds(0.5f);

        spin.CloseUI();
        secondSpin.CloseUI();
        arrow.SetActive(false);
        canvasPannelOutside.gameObject.SetActive(false);
        yield return new WaitForSeconds(0.2f);

        Debug.Log($">> {gameObject.name} 에서 AiBigWheelEnemySecond() : 바깥 = {spin.GetDamage()} / 안쪽 = {secondSpin.GetDamage()} / 결과 = {spin.GetDamage() * secondSpin.GetDamage()}");

        yield return PlayerParry(
            spin.GetDamage() * secondSpin.GetDamage());
            //spin.GetDamage() * secondSpin.GetDamage(),
            //pinky.IsAlive());
    }

    IEnumerator EnemyActionTiming()
    {
        yield return null;
    }
    IEnumerator PlayerParry(int finalDamage)
    {
        dummy.animatorOrNull.Play("ReadyParryable");

        float parryTimer = 0f;
        bool parryAttempted = false;

        while (parryTimer < 2f)
        {
            if (GameInputManager.Instance != null &&
                GameInputManager.Instance.GetActionDown(InputActionId.Parry, KeyCode.Space))
            {
                parryAttempted = true;
                break;
            }

            parryTimer += Time.deltaTime;
            yield return null;
        }
        if (parryAttempted)
        {
            ParryResult pResult = ParryResult.None;
            yield return StartCoroutine(ParryManager.Instance.StartParrySequence(finalDamage, res => pResult = res));

            if (pResult == ParryResult.Nullify || pResult == ParryResult.Reflect || pResult == ParryResult.Absorb || pResult == ParryResult.GameWinner)
            {
                if (pResult == ParryResult.Nullify) Debug.Log("[보스 공격 무효화]");
                else if (pResult == ParryResult.Reflect)
                {
                    Debug.Log($"[보스 공격 반사] : {finalDamage} 만큼");
                    dummy.TakeParryDamage(finalDamage);
                    if (dummy.IsAlive())
                    {
                        dummy.animatorOrNull.Play("HitParry");
                    }
                }
                else if (pResult == ParryResult.Absorb)
                {
                    Debug.Log($"[보스 공격 흡수] 데미지 무효화 & 플레이어 {finalDamage} 회복");
                    Player player = FindObjectOfType<Player>();
                    if (player != null) player.Heal(finalDamage, false);
                }
                else if (pResult == ParryResult.GameWinner)
                {
                    bool isSuccess = false;
                    bool isDone = false;

                    GameWinnerUI.Instance.Open(result =>
                    {
                        isSuccess = result;
                        isDone = true;
                    });

                    yield return new WaitUntil(() => isDone);

                    if (isSuccess)
                    {
                        Debug.Log("[승부수] 성공 → 이번 턴 데미지 0");
                        dummy.animatorOrNull.Play("Idle");
                        yield return new WaitForSeconds(1.0f);
                    }
                    else
                    {
                        Debug.Log("[승부수] 실패 → 이번 턴 데미지 2배");
                        dummy.PublicApplyDamageToPlayer(finalDamage * 2);
                        dummy.animatorOrNull.Play("Attack");
                        yield return new WaitForSeconds(1.0f);
                    }
                }
                if (dummy.IsAlive())
                {
                    yield return new WaitForSeconds(1.0f);
                }
            }
            else
            {
                dummy.PublicApplyDamageToPlayer(finalDamage);
                dummy.animatorOrNull.Play("Attack");
            }
        }
        else
        {
            dummy.PublicApplyDamageToPlayer(finalDamage);
            dummy.animatorOrNull.Play("Attack");
        }
        if (dummy.IsAlive())
        {
            yield return new WaitForSeconds(0.5f);
            dummy.animatorOrNull.Play("Idle");
            yield return new WaitForSeconds(0.5f);
        }
    }

    private void WheelHealSound()
    {
        if (AudioManager.Instance != null &&
        AudioManager.Instance.enemySounds != null &&
        AudioManager.Instance.enemySounds.wheelHeal != null)
        {
            AudioManager.Instance.PlaySFX(AudioManager.Instance.enemySounds.wheelHeal);
        }
    }
}
