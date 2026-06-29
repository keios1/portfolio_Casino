using System.Collections;
using UnityEngine;

public class BossSlotEnemy : EnemyBase
{
    [Header("보스 슬롯 머신 연결")]
    public BossSlotMachineManager bossSlotMachine;

    [Header("보스 전용 스프라이트")]
    public Sprite parrySprite;       // 패링(반격) 당했을 때 아파하는/튕겨나는 모습
    public Sprite chargeSprite;      // 기를 모을 때 (패턴 B)
    public Sprite chargeAttackSprite;// 기를 모으고 공격할 때 (패턴 C)
    public Sprite stunSprite;        // 과열되어 지쳤을 때 (패턴 D)
    public Sprite healSprite;        // 수리(회복) 중일 때 (패턴 E)

    // 페이즈 및 패턴 추적용 변수
    private bool isPhase2 = false;
    private int phase2Step = 0; // 0:충전(B), 1:잭팟(C), 2:과열(D), 3:수리(E)

    private bool isCharging = false; // 차지 공격 하는 중인지 확인 + 노랑이 이펙트

    private bool isRealDied = false; // 진짜 죽음 여부 추적용

    [Header("튜토리얼 지급 카드")]
    public SkillCardData absorbParryCardData; // 여기에 42056(흡수) 

    // 튜토리얼 SO 파일 경로 (Resources 폴더 기준)
    private const string PARRY_TUTORIAL_PATH = "TutorialSO/BattleTutorial_BossParry";
    private const string STRUGGLING_TUTORIAL_PATH = "TutorialSO/BattleTutorial_BossStruggling";

    protected override bool UseSlotDeadSound => true;

    public override bool IsAlive()
    {
        return !isRealDied;
    }

    private void Start()
    {
        isRealDied = false;
        if (bossSlotMachine == null)
            bossSlotMachine = FindObjectOfType<BossSlotMachineManager>();
    }

    private void Update()
    {
        // 보스 테스트용 데미지 키 (P)
        if (Input.GetKeyDown(KeyCode.P))
        {
            Debug.Log("[테스트] P키 누름 보스에게 10 데미지");
            TakeDamage(10);
        }
        if (Input.GetKey(KeyCode.LeftShift) && Input.GetKeyDown(KeyCode.K))
        {
            base.Die();
        }
    }

    /// <summary>
    /// 1. 턴 시작 라우터 (페이즈 분기)
    /// </summary>
    public override IEnumerator ExecuteEnemyTurn()
    {
        BossSlotEnemyData bData = data as BossSlotEnemyData;

        // 체력이 50% 이하로 떨어지면 2페이즈(고정 패턴) 돌입
        if (!isPhase2 && currentHP <= bData.maxHP * 0.5f)
        {
            isPhase2 = true;
            Debug.Log("보스의 체력이 50% 이하 2페이즈 고정 패턴");

            CardHandUI playerHandUI = FindObjectOfType<CardHandUI>();
            if (playerHandUI != null && absorbParryCardData != null)
            {
                playerHandUI.Draw(absorbParryCardData);
                Debug.Log("플레이어의 손패에 '흡수(42056)' 패링 카드를 지급");
            }

            // 2페이즈 진입 시 패링 튜토리얼 띄우기
            yield return StartCoroutine(ShowTutorialCoroutine(PARRY_TUTORIAL_PATH));

            if (AudioManager.Instance != null && bData.phase2BGM != null)
            {
                AudioManager.Instance.CrossFadeBGM(bData.phase2BGM, bData.bgmFadeTime);
            }
        }

        if (!isPhase2)
        {
            yield return StartCoroutine(PatternA_Roulette(bData));
        }
        else
        {
            yield return StartCoroutine(ExecutePhase2Pattern());
        }
    }

    // 리소스에서 튜토리얼을 불러와서 띄우는 전용 코루틴
    private IEnumerator ShowTutorialCoroutine(string resourcePath)
    {
        if (TutorialManager.Instance != null)
        {
            TutorialDataSO tutorialData = Resources.Load<TutorialDataSO>(resourcePath);

            if (tutorialData != null)
            {
                TutorialManager.Instance.ShowTutorial(tutorialData);

                // 튜토리얼 창이 완전히 닫힐 때까지 보스의 다음 행동을 멈추고 대기
                yield return new WaitUntil(() => TutorialManager.Instance.tutorialCanvas.activeSelf == false);
            }
            else
            {
                Debug.LogError($"튜토리얼 파일을 찾을 수 없습니다. 경로와 이름을 확인하세요: {resourcePath}");
            }
        }
    }

    /// <summary>
    /// 1페이즈: 패턴 A (룰렛 - 패링 불가)
    /// </summary>
    private IEnumerator PatternA_Roulette(BossSlotEnemyData bData)
    {
        Debug.Log($"[패턴 A] {bData.enemyName}의 일반 룰렛 턴 시작 (패링 불가)");

        if (isCharging == false)
        {
            animatorOrNull.Play("Ready");
            yield return new WaitForSeconds(0.5f);
        }
        else
        {
            animatorOrNull.Play("Charge");
        }
        bossSlotMachine.ShowUI();
        yield return new WaitForSeconds(1.0f);

        int finalDamage = 0;
        yield return StartCoroutine(bossSlotMachine.SpinRoutine(damage => finalDamage = damage));
        int calculatedDamage = (finalDamage / 10) * bData.bingoDamagePerLine;

        bossSlotMachine.CloseUI();
        yield return new WaitForSeconds(0.5f);

        if (calculatedDamage > 0)
        {

            // ChangeAnimation(EAnimationRequestArgument.ready);

            // ForceChangeSprite(readySprite);
            yield return new WaitForSeconds(0.4f);

            ApplyDamageToPlayer(calculatedDamage);
            animatorOrNull.Play("Attack");
            yield return new WaitForSeconds(1.0f);
        }

        // 공격이 끝났거나 데미지가 없으면 다시 Idle
        if (isCharging == false)
        {
            animatorOrNull.Play("Idle");
        }
        else
        {
            animatorOrNull.Play("Charge");
        }

        // ChangeAnimation(EAnimationRequestArgument.idle);
        // ForceChangeSprite(idleSprite);
        yield return new WaitForSeconds(1.0f);
    }

    /// <summary>
    /// 2페이즈: 상태 머신 (패턴 B~E 순환)
    /// </summary>
    private IEnumerator ExecutePhase2Pattern()
    {
        switch (phase2Step)
        {
            case 0: yield return StartCoroutine(PatternB_Charge()); break;
            case 1: yield return StartCoroutine(PatternC_Jackpot()); break;
            case 2: yield return StartCoroutine(PatternD_Overheat()); break;
            case 3: yield return StartCoroutine(PatternE_Repair()); break;
        }

        phase2Step = (phase2Step + 1) % 4;
    }

    private IEnumerator PatternB_Charge()
    {
        Debug.Log("[패턴 B - 충전중] 다음 턴에 강력한 공격");
        animatorOrNull.Play("Charge");
        // ForceChangeSprite(chargeSprite);
        isCharging = true;
        animatorOrNull.SetBool("IsCharging", true);


        yield return new WaitForSeconds(1.5f);
    }

    private IEnumerator PatternC_Jackpot()
    {
        animatorOrNull.SetBool("IsCharging", false);
        animatorOrNull.Play("Ready");
        yield return new WaitForSeconds(0.5f);


        Debug.Log("[패턴 C - 잭팟] 올 빙고 80 데미지 공격 (패링 가능)");
        isCharging = false;
        bossSlotMachine.ShowUI();

        yield return StartCoroutine(bossSlotMachine.ForceSpinRoutine(bossSlotMachine.jackpotSprite, AudioManager.Instance.enemySounds.slotJackpot));
        bossSlotMachine.CloseUI();
        yield return new WaitForSeconds(0.5f);

        //yield return StartCoroutine(HandleAttackWithParry(1)); // 패링 타격 이펙트 확인용
        yield return StartCoroutine(HandleAttackWithParry(80));
    }

    private IEnumerator PatternD_Overheat()
    {
        Debug.Log("[패턴 D - 과열] 보스가 턴을 넘깁니다");
        animatorOrNull.Play("Stun");
        yield return new WaitForSeconds(1.5f);
    }

    private IEnumerator PatternE_Repair()
    {
        Debug.Log("[패턴 E - 수리] 올 빙고 체력을 10 회복합니다");

        animatorOrNull.Play("Ready");
        yield return new WaitForSeconds(0.5f);


        bossSlotMachine.ShowUI();
        yield return StartCoroutine(bossSlotMachine.ForceSpinRoutine(bossSlotMachine.cherrySprite, AudioManager.Instance.enemySounds.slotHeal));
        bossSlotMachine.CloseUI();
        yield return new WaitForSeconds(0.5f);

        animatorOrNull.Play("Heal");

        currentHP += 10;
        if (currentHP > data.maxHP) currentHP = data.maxHP;

        UpdateHPUI();

        if (DamageTextManager.Instance != null)
        {
            DamageTextManager.Instance.ShowDamage(10, transform.position + Vector3.up * 3f, Color.green);
        }

        Debug.Log($"보스 체력 회복 완료 현재 HP: {currentHP}/{data.maxHP}");
        yield return new WaitForSeconds(1.0f);

        animatorOrNull.Play("Idle");
    }

    /// <summary>
    /// 체력 0 도달 시 발악 패턴
    /// </summary>
    protected override void Die()
    {
        Debug.Log("보스 체력 0 죽지 않고 발악 패턴");
        StartCoroutine(DesperationRoutine());
    }

    private IEnumerator DesperationRoutine()
    {
        BossSlotEnemyData bData = data as BossSlotEnemyData;
        bool isDefeated = false;

        if (AudioManager.Instance != null && bData.desperationBGM != null)
        {
            AudioManager.Instance.CrossFadeBGM(bData.desperationBGM, bData.bgmFadeTime);
        }

        Debug.Log("보스의 발악 패턴");

        // 발악 패턴 시작 시 발악 튜토리얼 띄우기
        yield return StartCoroutine(ShowTutorialCoroutine(STRUGGLING_TUTORIAL_PATH));

        bossSlotMachine.ShowUI();

        while (!isDefeated)
        {
            bool isSuccess = false;
            yield return StartCoroutine(bossSlotMachine.DesperationSpinRoutine(success => isSuccess = success));

            if (isSuccess)
            {
                Debug.Log("3칸 일치 성공 보스를 처치");
                isDefeated = true;
                bossSlotMachine.CloseUI();
                yield return new WaitForSeconds(0.5f);
                isRealDied = true;
                base.Die();
            }
            else
            {
                Debug.Log($"실패 플레이어에게 페널티 {bData.penaltyDamage} 데미지");
                bossSlotMachine.CloseUI();
                yield return new WaitForSeconds(0.2f);

                // 발악 패턴은 일반 readySprite 사용

                ApplyDamageToPlayer(bData.penaltyDamage);
                // yield return StartCoroutine(HandleAttackWithParry(bData.penaltyDamage));

                Debug.Log("보스가 다시 슬롯을 돌립니다...");
                bossSlotMachine.ShowUI();
                yield return new WaitForSeconds(0.5f);
            }
        }
        bossSlotMachine.CloseUI();
    }

    /// <summary>
    /// 보스 전용 패링 처리 공통 로직
    /// </summary>
    // private IEnumerator HandleAttackWithParry(int damage, Sprite attackSprite = null)
    private IEnumerator HandleAttackWithParry(int damage)
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
            yield return StartCoroutine(ParryManager.Instance.StartParrySequence(damage, res => pResult = res));

            if (pResult == ParryResult.Nullify || pResult == ParryResult.Reflect || pResult == ParryResult.Absorb || pResult == ParryResult.GameWinner)
            {
                if (pResult == ParryResult.Nullify) Debug.Log("[보스 공격 무효화]");
                else if (pResult == ParryResult.Reflect)
                {
                    Debug.Log("[보스 공격 반사]");
                    TakeParryDamage(damage);
                    animatorOrNull.Play("HitParry");
                }
                else if (pResult == ParryResult.Absorb)
                {
                    Debug.Log($"[보스 공격 흡수] 데미지 무효화 & 플레이어 {damage} 회복");
                    Player player = FindObjectOfType<Player>();
                    if (player != null) player.Heal(damage, false);
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
                        ApplyDamageToPlayer(damage * 2);
                        animatorOrNull.Play("Attack");
                        yield return new WaitForSeconds(1.0f);
                    }
                }


                yield return new WaitForSeconds(1.0f);
            }
            else
            {
                bool useNormalAnim = (attackSprite == null);
                ApplyDamageToPlayer(damage, useNormalAnim);
                animatorOrNull.Play("Attack");
            }
        }
        else
        {
            bool useNormalAnim = (attackSprite == null);
            ApplyDamageToPlayer(damage, useNormalAnim);
            animatorOrNull.Play("Attack");
        }

        yield return new WaitForSeconds(0.5f);
        animatorOrNull.Play("Idle");
        yield return new WaitForSeconds(0.5f);
    }
    private void PlayEnemySlotSFX(AudioClip clip)
    {
        if (AudioManager.Instance == null) return;
        if (AudioManager.Instance.enemySounds == null) return;
        if (clip == null) return;

        AudioManager.Instance.PlaySFX(clip);
    }
}
