using UnityEngine;
using System.Collections;

public class BattleTutorialController : MonoBehaviour
{
    // =========================================================
    [Header("시작할 배틀 튜토리얼 대본")]
    public TutorialDataSO battleTutorialData;

    [Header("튜토리얼 이후 전환할 다음 시나리오")]
    public TutorialDataSO startScenarioData;

    [Header("튜토리얼 대상 적")]
    public EnemyBase targetEnemy;

    [Header("지급할 전설 카드 데이터")]
    public SkillCardData triumphCardData;

    private void Start()
    {
        if (TutorialManager.Instance != null)
        {
            TutorialManager.Instance.OnTutorialCustomEvent += HandleBattleTutorialEvent;
        }

        Invoke("CheckAndStartTutorial", 0.1f);
    }

    private void OnDestroy()
    {
        if (TutorialManager.Instance != null)
        {
            TutorialManager.Instance.OnTutorialCustomEvent -= HandleBattleTutorialEvent;
        }
    }

    private void CheckAndStartTutorial()
    {
        if (TutorialManager.Instance != null && PlayerRuntimeData.Instance != null)
        {
            if (!PlayerRuntimeData.Instance.hasCompletedBattleTutorial && battleTutorialData != null)
            {
                TutorialManager.Instance.ShowTutorial(battleTutorialData);
            }
        }
    }

    private void HandleBattleTutorialEvent(string eventName)
    {
        switch (eventName)
        {
            case "WaitForEnemyAttack":
                StartWaitForEnemyAttack();
                break;
            case "EnemyTurnStart":
                ExecuteEnemyFixedPattern();
                break;

            case "GiveTriumphAndDice":
                StartCoroutine(GiveTriumphCardAndDiceRoutine());
                break;

            case "ReadyForAttackTutorial":
                StartAttackTutorial();
                break;
        }
    }

    public void StartAttackTutorial()
    {
        if (targetEnemy == null || !targetEnemy.gameObject.activeInHierarchy)
        {
            targetEnemy = FindObjectOfType<EnemyBase>();
        }

        if (targetEnemy != null)
        {
            targetEnemy.onAttackedEvent -= OnPlayerAttacked;
            targetEnemy.onAttackedEvent += OnPlayerAttacked;

        }
        else
        {
            Debug.LogError(" 씬에서 에너미를 찾을 수 없습나다");
        }
    }

    private void OnPlayerAttacked()
    {
        if (targetEnemy != null)
            targetEnemy.onAttackedEvent -= OnPlayerAttacked;

        TutorialManager.Instance.TryAction("SelectEnemy");
    }

    private void ExecuteEnemyFixedPattern()
    {
        Debug.Log("에너미가 정해진 고정 패턴을 실행합니다.");
    }

    // ==========================================
    // 트라이엄프 주사위 지급 (데이터 소유권 등록 추가)
    // ==========================================
    private IEnumerator GiveTriumphCardAndDiceRoutine()
    {
        Debug.Log("[튜토리얼] 트라이엄프 카드와 주사위 지급 시작");

        yield return new WaitForSeconds(0.5f);

        Player player = FindObjectOfType<Player>();
        if (player != null)
        {
            player.ClearDice();
            if (DiceUI.Instance != null) DiceUI.Instance.StopAllBlinks();
        }

        yield return new WaitForSeconds(0.3f);

        BattleManager battleManager = FindObjectOfType<BattleManager>();
        if (battleManager != null)
        {
            battleManager.GiveTutorialDice(6, 4); // 주사위 충분히 지급
        }

        yield return new WaitForSeconds(0.8f);

        // 상점 트라이엄프 재고 차감
        if(SkillCardShopManager.Instance != null && triumphCardData != null)
        {
            SkillCardShopManager.Instance.PurchaseCard(triumphCardData);
        }

        if (PlayerCardCollection.Instance != null && triumphCardData != null)
        {
            PlayerCardCollection.Instance.AddCardRuntimeOnly(triumphCardData, 1);
            Debug.Log("PlayerCardCollection 데이터 상에 트라이엄프 1장 강제 추가 완료.");
        }

        // 이제 손패에 그리고 비주얼 갱신
        CardHandUI playerHandUI = FindObjectOfType<CardHandUI>();
        if (playerHandUI != null && triumphCardData != null)
        {
            playerHandUI.Draw(triumphCardData);
            playerHandUI.UpdatePlayableVisuals();
        }
    }

    public void OnEnemyDead()
    {
        if (TutorialManager.Instance != null && TutorialManager.Instance.IsTutorialActive)
        {
            TutorialManager.Instance.ForceNextPage();
        }

        if (PlayerRuntimeData.Instance != null)
        {
            PlayerRuntimeData.Instance.hasCompletedBattleTutorial = true;
            PlayerRuntimeData.Instance.justFinishedTutorialBattle = true;
            PlayerRuntimeData.Instance.SaveToSaveData();
        }
    }

    // ==========================================
    // 플레이어 피격 대기 튜토리얼 로직
    // ==========================================
    public void StartWaitForEnemyAttack()
    {
        Player player = FindObjectOfType<Player>();
        if (player != null)
        {
            // 중복 방지 후 이벤트 구독
            player.onPlayerAttackedEvent -= OnPlayerTakesDamage;
            player.onPlayerAttackedEvent += OnPlayerTakesDamage;
        }
    }

    private void OnPlayerTakesDamage()
    {
        Player player = FindObjectOfType<Player>();
        if (player != null)
        {
            player.onPlayerAttackedEvent -= OnPlayerTakesDamage;
        }

        TutorialManager.Instance.ForceNextPage();
    }
}
