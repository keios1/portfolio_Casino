using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UIElements;

public class BigWheelEnemyDummyAiThird : EnemyBase
{
    public class Node
    {
        public System.Func<Node> getNext;
        public System.Func<IEnumerator> function;
    }

    public GameObject canvasPannelOutside;
    public BigWheelSpin spin; // 하이어라키에 있는 슬롯매니저를 연결해야 함
    public BigWheelSpinSecond secondSpin;
    public GameObject handRelease;
    public GameObject handHold;
    public GameObject arrow;
    public GameObject back;

    private bool isChanged = false;
    private bool isCharging = false;
    private Node next;
    private Node[] normal;
    private Node[] changed;

    [Header("패턴 E")]
    public GameObject patternEBullet;
    public Transform startPosition;
    public Transform endPosition;
    public float bulletMoveTime = 0.5f;
    public float shotTerm = 0.3f;

    [Header("발악 패턴")]
    public GameObject finalWheel;
    public int[] slotValue = new int[] { 5, 5, 10, 10, 20, 0 };
    public string[] slotLore = new string[] { "-5", "-5", "-10", "-10", "-20", "WIN" };
    public TextMeshProUGUI[] textList;
    public bool isEnd = false;
    public RectTransform cursorRect;
    public RectTransform dartRect;
    public Canvas targetCanvas;
    public BigWheelDartUI dartUI;
    public bool isCursorChanged = false;
    public bool isDartShot = false;

    // 이제 프라이빗
    private BigWheelEnemyDataBoss convertedData;
    private AddHPEventArgs addHPEventArgs;

    public override IEnumerator ExecuteEnemyTurn()
    {
        if ((isChanged == false) && (CurrentHP * 2 < data.maxHP))
        {
            if (AudioManager.Instance != null &&
                convertedData != null &&
                convertedData.phase2BGM != null)
            {
                AudioManager.Instance.CrossFadeBGM(
                    convertedData.phase2BGM,
                    convertedData.bgmFadeTime
                );
            }
            // yield return changed[3].function();
            next = changed[0].getNext();
            isChanged = true;
            yield return changed[0].function();
        }
        else
        {
            yield return next.function();
            next = next.getNext();
        }

        if (IsAlive())
        {
            if (isCharging)
            {
                animatorOrNull.Play("Charge");
            }
            else
            {
                animatorOrNull.Play("Idle");
            }
        }
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
        isCharging = false;
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

        changed = new Node[5];
        changed[0] = new Node
        {
            function = PatternD,
            getNext = () => changed[1]
        };
        changed[1] = new Node
        {
            function = PatternE,
            getNext = () => changed[2]
        };
        changed[2] = new Node
        {
            function = PatternD,
            getNext = () => changed[3]
        };
        changed[3] = new Node
        {
            function = PatternE,
            getNext = () => changed[4]
        };
        changed[4] = new Node
        {
            function = PatternC,
            getNext = () => changed[0]
        };

        
        spin.gameObject.SetActive(false);
        secondSpin.gameObject.SetActive(false);
        next = normal[0];
        back.SetActive(false);
        handRelease.SetActive(false);
        handHold.SetActive(false);
        arrow.SetActive(false);
        finalWheel.SetActive(false);
        cursorRect.gameObject.SetActive(false);

        convertedData = data as BigWheelEnemyDataBoss;

        Debug.Assert(
            convertedData != null,
            $"<!> {gameObject.name} : data를 BigWheelEnemyDataBoss로 변환할 수 없음"
        );

        addHPEventArgs = new EnemyBase.AddHPEventArgs();
        addHPEventArgs.Initialize(transform.position);
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
        if (isCursorChanged)
        {
            Vector2 localPoint;

            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                targetCanvas.transform as RectTransform,
                Input.mousePosition,
                targetCanvas.worldCamera,
                out localPoint
            );

            cursorRect.localPosition = localPoint;
        }
        if (isDartShot == false) // 다트 발사한 상태에선 고정한상태로 보여져야 하기 때문
        {
            Vector2 localPoint;

            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                targetCanvas.transform as RectTransform,
                Input.mousePosition,
                targetCanvas.worldCamera,
                out localPoint
            );

            dartRect.localPosition = localPoint;
        }
    }

    IEnumerator PatternA()
    {
        Debug.Log("PatternA()");

        spin.ChangeThirdPatternAB();
        spin.ShowUI();
        arrow.SetActive(true);
        canvasPannelOutside.gameObject.SetActive(true);
        handRelease.SetActive(true);
        back.SetActive(true);
        animatorOrNull.Play("Ready");

        yield return new WaitForSeconds(1.0f);
        yield return spin.SpinTargetCoroutine(0);
        yield return new WaitUntil(() => !spin.IsSpinning);

        handHold.SetActive(true);
        handRelease.SetActive(false);
        
        yield return new WaitForSeconds(0.5f);

        arrow.SetActive(false);
        spin.CloseUI();
        handHold.SetActive(false);
        back.SetActive(false);
        canvasPannelOutside.gameObject.SetActive(false);
        yield return new WaitForSeconds(0.2f);

        // yield return PlayerParry(spin.GetDamage(), true);
        yield return PlayerParryNormal(spin.GetDamage());
    }

    IEnumerator PatternB()
    {
        Debug.Log("PatternB()");

        arrow.SetActive(true);
        spin.ChangeThirdPatternAB();
        spin.ShowUI();
        secondSpin.ShowUI();

        canvasPannelOutside.gameObject.SetActive(true);
        handRelease.SetActive(true);
        back.SetActive(true);

        Debug.Log("슬롯 머신 준비 중...");

        // rinky.animatorOrNull.Play("Ready");
        animatorOrNull.Play("Ready");
        yield return new WaitForSeconds(1.0f);

        yield return spin.SpinTargetCoroutine(0);
        StartCoroutine(secondSpin.SpinTargetCoroutine(1)); // 별도 코루틴 실행
        yield return new WaitUntil(() => (!spin.IsSpinning && !secondSpin.IsSpinning));

        handHold.SetActive(true);
        handRelease.SetActive(false);
        yield return new WaitForSeconds(0.5f);

        arrow.SetActive(false);
        spin.CloseUI();
        secondSpin.CloseUI();
        back.SetActive(false);
        canvasPannelOutside.gameObject.SetActive(false);
        handHold.SetActive(false);
        yield return new WaitForSeconds(0.2f);
        // 패링 없음
        yield return PlayerParryNormal(spin.GetDamage() * 2);
    }

    IEnumerator PatternC() // 힐 
    {
        addHPEventArgs.isUsed = false;
        addHPEventArgs.amount = 15;

        Debug.Log("PatternC()");

        arrow.SetActive(true);
        spin.ChangeThirdPatternC();
        spin.ShowUI();
        canvasPannelOutside.gameObject.SetActive(true);
        back.SetActive(true);
        handRelease.SetActive(true);

        Debug.Log("슬롯 머신 준비 중...");

        animatorOrNull.Play("Ready");
        yield return new WaitForSeconds(1.0f);

        yield return spin.SpinTargetCoroutine(0);
        yield return new WaitUntil(() => !spin.IsSpinning);

        handHold.SetActive(true);
        handRelease.SetActive(false);
        yield return new WaitForSeconds(0.5f);

        arrow.SetActive(false);
        spin.CloseUI();
        back.SetActive(false);
        canvasPannelOutside.gameObject.SetActive(false);
        handHold.SetActive(false);
        yield return new WaitForSeconds(0.2f);

        animatorOrNull.Play("Recover");

        addHPEventArgs.isUsed = false;
        addHPEventArgs.amount = spin.GetDamage();

        WheelHealSound();
        AddHP(addHPEventArgs);
        yield return new WaitForSeconds(1.0f);

        animatorOrNull.Play("Idle");
    }

    IEnumerator PatternD()
    {
        Debug.Log("PatternD()");

        // 차지 모션
        isCharging = true;
        animatorOrNull.Play("Charge");
        animatorOrNull.SetBool("IsCharging", true);
        yield return new WaitForSeconds(1.5f);
    }

    IEnumerator PatternE()
    {
        Debug.Log("PatternE()");
        isCharging = false;
        //animatorOrNull.Play("Attack");
        animatorOrNull.SetBool("IsCharging", false);

        int attackCount = UnityEngine.Random.Range(2, 5);
        yield return PlayerParryPatternE(attackCount);
    }

    IEnumerator PlayerParryNormal(int finalDamage)
    {
        animatorOrNull.Play("ReadyParryable");

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
                    TakeParryDamage(finalDamage);
                    if (IsAlive())
                    {
                        animatorOrNull.Play("HitParry");
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
                        animatorOrNull.Play("Idle");
                        yield return new WaitForSeconds(1.0f);
                    }
                    else
                    {
                        Debug.Log("[승부수] 실패 → 이번 턴 데미지 2배");
                        ApplyDamageToPlayer(finalDamage * 2);
                        animatorOrNull.Play("Attack");
                        yield return new WaitForSeconds(1.0f);
                    }
                }
                if (IsAlive())
                {
                    yield return new WaitForSeconds(1.0f);
                }
            }
            else
            {
                bool useNormalAnim = (attackSprite == null);
                ApplyDamageToPlayer(finalDamage, useNormalAnim);
                animatorOrNull.Play("Attack");
            }
        }
        else
        {
            bool useNormalAnim = (attackSprite == null);
            ApplyDamageToPlayer(finalDamage, useNormalAnim);
            animatorOrNull.Play("Attack");
        }
        if (IsAlive())
        {
            yield return new WaitForSeconds(0.5f);
            animatorOrNull.Play("Idle");
            yield return new WaitForSeconds(0.5f);
        }
    }

    IEnumerator PlayerParryPatternE(int attackCount)
    {
        int finalDamage = attackCount * 10;
        animatorOrNull.Play("ReadyParryable2");

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
            yield return StartCoroutine(ParryManager.Instance.StartParrySequence(0, res => pResult = res)); // 코드 뜯어보니 첫번째 매개변수 아예 안 씀

            if (pResult == ParryResult.Nullify || pResult == ParryResult.Reflect || pResult == ParryResult.Absorb || pResult == ParryResult.GameWinner)
            {
                if (pResult == ParryResult.Nullify) Debug.Log("[보스 공격 무효화]");
                else if (pResult == ParryResult.Reflect)
                {
                    Debug.Log("[보스 공격 반사]");
                    TakeParryDamage(attackCount * 10);
                    if (IsAlive())
                    {
                        animatorOrNull.Play("HitParry");
                    }
                }
                else if (pResult == ParryResult.Absorb)
                {
                    Debug.Log($"[보스 공격 흡수] 데미지 무효화 & 플레이어 {finalDamage} 회복");
                    Player player = FindObjectOfType<Player>();
                    if (player != null) player.Heal(attackCount * 2, false);
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
                        animatorOrNull.Play("Idle");
                        yield return new WaitForSeconds(1.0f);
                    }
                    else
                    {
                        Debug.Log("[승부수] 실패 → 이번 턴 데미지 2배");
                        yield return HitPatternE(attackCount, 2f);
                        animatorOrNull.Play("Attack");
                        yield return new WaitForSeconds(1.0f);
                    }
                }
                if (IsAlive())
                {
                    yield return new WaitForSeconds(1.0f);
                }
            }
            else
            {
                bool useNormalAnim = (attackSprite == null);
                yield return HitPatternE(attackCount);
                animatorOrNull.Play("Attack");
            }
        }
        else
        {
            bool useNormalAnim = (attackSprite == null);
            yield return HitPatternE(attackCount);
            animatorOrNull.Play("Attack");
        }
        if (IsAlive())
        {
            yield return new WaitForSeconds(0.5f);
            animatorOrNull.Play("Idle");
            yield return new WaitForSeconds(0.5f);
        }
    }

    IEnumerator HitPatternE(int hitCount, float multiplyDamage = 1.0f)
    {
        for (int i = 0; i < hitCount; i++)
        {
            animatorOrNull.Play("AttackCharge");
            GameObject _g = Instantiate(patternEBullet, startPosition.position, Quaternion.identity);
            BigWheelBullet _bullet = _g.GetComponent<BigWheelBullet>();
            _bullet.beginPosition = startPosition;
            _bullet.endPosition = endPosition;
            _bullet.moveTime = bulletMoveTime;
            _bullet.Begin(() => { ApplyDamageToPlayer((int)(10f * multiplyDamage), useBaseAnim: false); });
            yield return new WaitForSeconds(shotTerm);
            
        }
    }

    IEnumerator DesperationRoutine()
    {
        int GetSlotIndex(Vector2 mouseScreenPos, float spinedAmount, float maxLength)
        {
            //Vector3 clickPosRaw = Input.mousePosition;
            Vector3 clickPosRaw = new Vector3(mouseScreenPos.x, mouseScreenPos.y, 0);
            Vector3 clickPos = clickPosRaw - (new Vector3(Screen.width, Screen.height, 0) / 2);

            Debug.Log($">> mag = {clickPos.magnitude} / rawPos {Input.mousePosition} / clickPos {clickPos} / mid {(new Vector3(Screen.width, Screen.height, 0) / 2)}");
            if (maxLength < clickPos.magnitude) return -1;


            float angle = Mathf.Atan2(clickPos.y, -clickPos.x) * Mathf.Rad2Deg - 60f + spinedAmount;
            for (; angle < 0;) angle += 360f;
            for (; angle >= 360;) angle -= 360f;


            // X값을 반전한 이유는. 일단 룰렛의 배치는 시계 방향으로 돌아가면서 인덱스가 증가하는 반면,
            // 유니티 상 시계 반대방향으로 돌아가면서 각도값이 커짐
            // 스핀드어마운트를 추가하는 이유는 원판이 돌아가는 방향이랑 앵글값이 상승하는 방향이 서로 다르기 때문임.
            // 그러니 원판이 더 회전할수록 앵글값 기준으로는 더 앞으로 나아간것처럼 느껴지는 것

            // 화면 상태는 이러함
            // 6 \ 1 / 2
            // ====X====
            // 5 / 4 \ 3

            int result = (int)(angle / 60f);
            return result;

            //Vector3 wheelScreenPos =
            //    Camera.main.WorldToScreenPoint(finalWheel.transform.position);

            //Vector2 dir = mouseScreenPos - (Vector2)wheelScreenPos;

            //float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

            //// 0~360으로 변환
            //angle = (angle + 360f) % 360f;

            //// 룰렛 회전 보정
            //angle -= finalWheel.transform.eulerAngles.z;

            //angle = (angle + 360f) % 360f;

            //// 60도씩 분할
            //int slot = Mathf.FloorToInt(angle / 60f);

            //return slot;
        }
        void BeginCursor()
        {
            cursorRect.gameObject.SetActive(true);
            UnityEngine.Cursor.visible = false;
            isCursorChanged = true;
        }
        void EndCursor()
        {
            cursorRect.gameObject.SetActive(false);
            UnityEngine.Cursor.visible = true;
            isCursorChanged = false;
        }
        void ShotDart()
        {
            dartUI.Show();
            isDartShot = true;
        }
        void ResetDart()
        {
            dartUI.Hide();
            isDartShot = false;
        }
        if (AudioManager.Instance != null && convertedData.desperationBGM != null)
        {
            AudioManager.Instance.CrossFadeBGM(convertedData.desperationBGM, convertedData.bgmFadeTime);
        }

        yield return null;
        back.SetActive(true);
        arrow.SetActive(true);
        //룰렛이 돌아가며 
        // 룰렛 칸에는  -5HP, -5HP, -10HP, -10HP, -20HP, 승리가 존재
        // 플레이어가 좌클릭 입력 시 입력한 위치에 다트가 날라가며 맞춘 곳 결과 적용
        bool isDefeated = false;
        finalWheel.SetActive(true);
        float speed = UnityEngine.Random.Range(360.0f, 720.0f);
        //float speed = 0.0f;
        for (int index = 0; index < 6; ++index)
        {
            textList[index].text = slotLore[index];
        }

        while (isDefeated == false)
        {
            // 룰렛 초기화

            BeginCursor();

            // 룰렛 클릭 대기
            while (true)
            {
                // 룰렛 클릭함
                if (Input.GetMouseButtonDown(0))
                {
                    //int _slotIndex = GetSlotIndex(Input.mousePosition, finalWheel.transform.eulerAngles.z, 166f);
                    //Debug.Log($"맞춘 슬롯 : {_slotIndex} / {slotLore[_slotIndex]}");

                    break;
                }
                // 스핀 적용
                Vector3 _angleValue = finalWheel.transform.eulerAngles;
                _angleValue.z += speed * Time.deltaTime;
                finalWheel.transform.eulerAngles = _angleValue;
                yield return null;
            }
            EndCursor();
            int slotIndex = GetSlotIndex(Input.mousePosition, finalWheel.transform.eulerAngles.z, 260f);
            if(slotIndex != -1) ShotDart();
            // 판정
            yield return new WaitForSeconds(1.0f);
            
            // 1. 범위 내에 있는지 판정 / 아니면 실패
            // 2. 각도 기준 판정
            // 2.1. 0이면 즉시 종료
            // 2.2. 아니면 데미지 주입

            
            if (slotIndex != -1) Debug.Log($"맞춘 슬롯 : {slotIndex} / {slotLore[slotIndex]}");
            if (slotIndex == -1) continue;

            if (slotValue[slotIndex] == 0) // WIN
            {
                isDefeated = true;
                CleanupDesperationUI();

                isEnd = true;
                base.Die();

                yield break;
            }
            else
            {
                int damage = slotValue[slotIndex];
                ResetDart();
                finalWheel.SetActive(false);
                back.SetActive(false);
                arrow.SetActive(false);
                animatorOrNull.Play("Attack");
                yield return new WaitForSeconds(0.3f);


                ApplyDamageToPlayer(damage);
                Player player = FindObjectOfType<Player>();

                if (player != null && player.CurrentHp <= 0)
                {
                    CleanupDesperationUI();
                    yield break;
                }

                yield return new WaitForSeconds(0.7f);

                Debug.Log($"{damage} 데미지");
                finalWheel.SetActive(true);
                back.SetActive(true);
                arrow.SetActive(true);
            }

            if (isDefeated)
            {
                break;
            }
            // 실패
            // 데미지 가함
            
            continue;
        }
        CleanupDesperationUI();

        isEnd = true;
        base.Die();
    }
    private void CleanupDesperationUI()
    {
        if (dartUI != null)
            dartUI.Hide();

        if (finalWheel != null)
            finalWheel.SetActive(false);

        if (back != null)
            back.SetActive(false);

        if (arrow != null)
            arrow.SetActive(false);

        if (cursorRect != null)
            cursorRect.gameObject.SetActive(false);

        isDartShot = false;
        isCursorChanged = false;
        UnityEngine.Cursor.visible = true;
    }
    protected override void Die()
    {
        StartCoroutine(DesperationRoutine());
    }

    public override bool IsAlive()
    {
        if (isEnd) return false;
        return true;
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
