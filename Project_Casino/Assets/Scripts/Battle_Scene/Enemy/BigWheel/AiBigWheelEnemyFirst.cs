using System.Collections;
using System.Collections.Generic;
using TMPro.Examples;
//using UnityEditor.ShaderKeywordFilter;
using UnityEngine;

// 핑키에 부착
public class AiBigWheelEnemyFirst : MonoBehaviour
{
    public class Node
    {
        public System.Func<Node> getNext;
        public System.Func<IEnumerator> function;
    }

    // public BigWheelEnemyPinky pinky;
    public BigWheelEnemyDummyAiFirst dummy;
    public BigWheelSpin spin; // 하이어라키에 있는 슬롯매니저를 연결해야 함
    public GameObject canvasPannelOutside;
    public GameObject arrow;

    private Node next;
    //private bool isDidPatternD = false;

    private Node[] normalPattern;
    private Node[] changedPattern;

    private int _finalDamage = 0;

    public IEnumerator ExecuteEnemyTurn()
    {
        Debug.Log($">> AiBigWheelEnemyFirst {gameObject.name}에서 실행");

        // 여기서 AI 작업 수행
        // 1. is hp under 50% && changed == false? -> C
        // 2. is snipeReady? -> B
        // 3. changed? D
        // 4. A

        _finalDamage = 0;

        bool IsPatternC()
        {
            if (spin.IsChanged) return false;
            if (dummy.CurrentHP * 2 < dummy.data.maxHP) return true;
            return false;
        }


        if (IsPatternC())
        {
            yield return changedPattern[2].function();
            next = changedPattern[0];
        }
        else
        {
            yield return next.function();
            next = next.getNext();
        }

        if (_finalDamage > 0)
        {
            yield return PlayerParry(_finalDamage);
        }
        if (dummy.IsAlive())
        {
            dummy.animatorOrNull.Play("Idle");
        }
    }


    // Start is called before the first frame update
    void Start()
    {
        normalPattern = new Node[3];
        normalPattern[0] = new Node
        {
            function = PatternAD,
            getNext = () => normalPattern[1]
        };
        normalPattern[1] = new Node
        {
            function = PatternAD,
            getNext = () => normalPattern[2]
        };
        normalPattern[2] = new Node
        {
            function = PatternB,
            getNext = () => normalPattern[0]
        };

        changedPattern = new Node[3];
        changedPattern[0] = new Node
        {
            function = PatternAD,
            getNext = () => changedPattern[1]
        };
        changedPattern[1] = new Node
        {
            function = PatternB,
            getNext = () => changedPattern[0]
        };
        changedPattern[2] = new Node
        {
            function = PatternC,
            getNext = () => changedPattern[0]
        };

        next = normalPattern[0];
        spin.gameObject.SetActive(false);
        arrow.SetActive(false);
        canvasPannelOutside.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {

    }

    // 패턴
    private IEnumerator PatternAD()
    {
        Debug.Log("PatternAD()");
        canvasPannelOutside.gameObject.SetActive(true);
        spin.gameObject.SetActive(true);
        Debug.Log($"spin.gameObject.name = {spin.gameObject.name}");
        Debug.Log($"spin activeSelf = {spin.gameObject.activeSelf}");
        Debug.Log($"spin activeInHierarchy = {spin.gameObject.activeInHierarchy}");
        Debug.Log($"spin.gameObject.scene.name = {spin.gameObject.scene.name}");
        Debug.Log($"spin.transform.root.name = {spin.transform.root.name}");
        Debug.Log($"spin.gameObject.GetInstanceID() = {spin.gameObject.GetInstanceID()}");

        spin.ShowUI();
        Debug.Log($"2spin activeSelf = {spin.gameObject.activeSelf}");
        Debug.Log($"2spin activeInHierarchy = {spin.gameObject.activeInHierarchy}");
        arrow.SetActive(true);
        Debug.Log($"3spin activeSelf = {spin.gameObject.activeSelf}");
        Debug.Log($"3spin activeInHierarchy = {spin.gameObject.activeInHierarchy}");
        Debug.Log("슬롯 머신 준비 중...");

        yield return new WaitForSeconds(1.0f);
        Debug.Log($"4spin activeSelf = {spin.gameObject.activeSelf}");
        Debug.Log($"4spin activeInHierarchy = {spin.gameObject.activeInHierarchy}");
        yield return spin.SpinRandomCoroutine();
        yield return new WaitUntil(() => !spin.IsSpinning);
        Debug.Log($"5spin activeSelf = {spin.gameObject.activeSelf}");
        Debug.Log($"5spin activeInHierarchy = {spin.gameObject.activeInHierarchy}");
        yield return new WaitForSeconds(0.5f);


        int damage = spin.GetDamage();
        _finalDamage = damage;
        Debug.Log($"6spin _finalDamage = {_finalDamage}");



        spin.CloseUI();
        arrow.SetActive(false);
        canvasPannelOutside.gameObject.SetActive(false);
        yield return new WaitForSeconds(0.2f);

    }

    private IEnumerator PatternB()
    {
        Debug.Log("PatternB()");
        spin.ShowUI();
        canvasPannelOutside.gameObject.SetActive(true);
        arrow.SetActive(true);
        Debug.Log("슬롯 머신 준비 중...");
        yield return new WaitForSeconds(1.0f);

        yield return spin.SpinTargetCoroutine(3);
        yield return new WaitUntil(() => !spin.IsSpinning);

        yield return new WaitForSeconds(0.5f);

        _finalDamage = spin.GetDamage();


        spin.CloseUI();
        arrow.SetActive(false);
        canvasPannelOutside.gameObject.SetActive(false);
        yield return new WaitForSeconds(0.2f);
    }

    private IEnumerator PatternC()
    {
        Debug.Log("PatternC()");
        spin.Change();
        dummy.animatorOrNull.Play("Draw");
        yield return new WaitForSeconds(1.0f);
        dummy.animatorOrNull.Play("Idle");
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

        //while (parryTimer < 0.5f)
        while (parryTimer < 2f)
        {
            if (GameInputManager.Instance != null &&
                GameInputManager.Instance.GetActionDown(InputActionId.Parry, KeyCode.Space))
            {
                parryAttempted = true;
                // Instantiate(parryEffectPrefab, transform.position + new Vector3(0, 0, -1), Quaternion.identity);
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
                    Debug.Log("[보스 공격 반사]");
                    dummy.TakeParryDamage(finalDamage);
                    if (dummy.IsAlive())
                    {
                        dummy.animatorOrNull.Play("HitParry");
                    }
                    else
                    {
                        dummy.animatorOrNull.Play("Dead");
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

}
