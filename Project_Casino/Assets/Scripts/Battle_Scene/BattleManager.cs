using System.Collections;
using UnityEngine;
using TMPro;
using System.Collections.Generic;
/// <summary>
/// 전투 전체 흐름을 관리하는 매니저.
/// 턴 시작/종료, 플레이어 주사위 지급, 적 행동 실행,
/// 전투 종료 판정, 전투 결과 UI 호출을 담당한다.
/// 
/// 실제 보상 지급은 BattleResultUI에서 처리하고,
/// 여기서는 보상 데이터만 생성해서 넘겨준다.
/// </summary>
public class BattleManager : MonoBehaviour
{
    public enum TurnOwner
    {
        Player,
        Enemy
    }

    [SerializeField] private Player player;
    //[SerializeField] private EnemyBase enemy;
    [SerializeField] private List<EnemyBase> enemy;
    [SerializeField] private BattleResultUI battleResultUI;// 전투 결과 UI 연결

    [Header("Turn")]
    [SerializeField] private TurnOwner currentTurn = TurnOwner.Player;
    private int playerTurnCount = 0;
    public TurnOwner CurrentTurn => currentTurn;

    [Header("Dice Roll")]
    [SerializeField] private DiceRollPresenter dicePresenter;
    [Header("Dice Spawn Preview")]
    [SerializeField] private DiceSpawnPreviewUI diceSpawnPreviewUI;
    //[SerializeField] private float dicePreviewHoldTime = 0.5f;
    [SerializeField] private float diceSpawnInterval = 0.15f;
    [Header("Card Management")]
    [SerializeField] private CardManager cardManager;
    [Header("Turn UI")]
    [SerializeField] private TurnAnnounceUI turnAnnounceUI;
    [Header("Debug")]
    [SerializeField] private bool testRunEnemyAction;
    [SerializeField] private CardHandUI handUI;

    private bool isFirstPlayerTurn = true;// 첫 번째 플레이어 턴 여부 (첫 턴에는 카드 드로우 애니메이션 생략)
    private bool battleEnded = false;// 전투 종료 여부
    private List<EnemyData> cachedEnemyData; // 적 데이터 캐싱 (예시)
    private List<int> pendingDicePreviewSlots = new List<int>();// 주사위 지급 대기 슬롯 인덱스 리스트
    private bool isRollingDice = false;// 주사위 지급 중 여부

    private void Start()
    {
        if (cachedEnemyData == null)
            cachedEnemyData = new List<EnemyData>();

        if (EnemyContainer.instance != null)
        {
            enemy = EnemyContainer.instance.enemies;
            Debug.Log($">> BattleManager.Start() => {EnemyContainer.instance.gameObject.name} 게임오브젝트에 설정된 에너미를 참조합니다.");
        }

        if (enemy != null && player != null)
        {
            foreach (EnemyBase e in enemy)
            {
                if (e == null)
                {
                    Debug.Log($">> BattleManager.Start() => enemy 리스트에 null이 포함되어 있습니다. 인스펙터 창에서 {gameObject.name} 게임오브젝트의 BattleManager 컴포넌트의 Enemy 멤버에, 씬에 존재하는 에너미를 넣어주세요.");
                }


                e.SetPlayerTarget(player);
                cachedEnemyData.Add(e.data); // 적 데이터 캐싱
            }
        }
        // 전투 내 카드 사용 획수 초기화
        //"씬을 다시 로드해서 전투 시작"시에 의미 있음 씬을 유지한 채로 전투를 재시작하면 함수로 만들어서 호출 해주기
        if (cardManager != null)
            cardManager.ResetBattleUsage();

        if (player != null)
            player.OnDiceChanged += HandlePlayerDiceChanged;

        StartTurn(currentTurn);
    }
    private void OnDestroy()
    {
        if (player != null)
            player.OnDiceChanged -= HandlePlayerDiceChanged;
    }
    /// <summary>
    /// 매 프레임마다 승리/패배 조건 체크 및 전투 결과 UI 표시
    /// </summary>
    private void Update()
    {
        CheckBattleEnd();
    }
    /// <summary>
    /// 지정된 턴 소유자(Player/Enemy)의 턴을 시작하는 함수.
    /// 자신의 턴이 시작되면 필요한 초기화 작업을 수행하고,
    /// 적턴이면 적 행동을 실행하는 코루틴을 시작한다.
    /// </summary>
    /// <param name="owner"></param>
    public void StartTurn(TurnOwner owner)
    {
        CheckBattleEnd();
        if (battleEnded) return;

        currentTurn = owner;
        Debug.Log($"[Turn Start] {currentTurn}");

        ShowTurnText();

        if (currentTurn == TurnOwner.Player)
        {
            playerTurnCount++;

            if (cardManager != null)
                cardManager.ResetTurnUsage();

            if (handUI != null)
            {
                handUI.RestoreUnlimitedCards();

                if (!isFirstPlayerTurn)
                    handUI.DrawRandomCardWithAnimation();

                isFirstPlayerTurn = false;
            }

            ClearShieldAtOwnTurnStart(currentTurn);

            StartCoroutine(RollDiceForPlayerRoutine());
        }
        else
        {
            ClearShieldAtOwnTurnStart(currentTurn);

            if (enemy != null)
            {
                StartCoroutine(RunEnemyAction(enemy));
            }
            else
            {
                Debug.LogWarning("[BattleManager] Enemy is null!");
            }
        }
    }
    private void ShowTurnText()
    {
        if (turnAnnounceUI == null)
        {
            Debug.LogWarning("TurnAnnounceUI가 BattleManager에 연결되지 않았습니다.");
            return;
        }

        string message = (currentTurn == TurnOwner.Player) ? "내 턴" : "상대 턴";

        Debug.Log($">> 턴 UI 출력: {message}");
        turnAnnounceUI.Play(message);
    }

    private void ClearShieldAtOwnTurnStart(TurnOwner owner)
    {
        if (owner == TurnOwner.Player)
        {
            if (player != null)
            {
                player.ClearShield();
            }

            Debug.Log("[Shield] 플레이어 턴 시작 → 플레이어 쉴드 제거");
        }
        else
        {
            if (enemy == null) return;

            foreach (EnemyBase e in enemy)
            {
                if (e == null) continue;
                e.ClearShield();
            }

            Debug.Log("[Shield] 적 턴 시작 → 적 쉴드 제거");
        }
    }

    public void EndTurn()
    {
        if (currentTurn == TurnOwner.Player)
        {
            if (TutorialManager.Instance != null && !TutorialManager.Instance.TryAction("EndTurn"))
                return;
        }

        if (cardManager != null && cardManager.IsCardUseInProgress)
        {
            Debug.Log("[BattleManager] 카드 사용 중에는 턴 종료 불가");
            return;
        }
        IEnumerator EndTurnCoroutine()
        {
            ButtonBlockerManager.Instance.SetBlock(
                ButtonBlockerManager.Instance.endTurnBlock, true);
            yield return new WaitForSeconds(2f); // 턴이 끝난 후 잠시 대기

            ButtonBlockerManager.Instance.SetBlock(
                ButtonBlockerManager.Instance.endTurnBlock, false);
        }


        StartCoroutine(EndTurnCoroutine());


        CheckBattleEnd();
        if (battleEnded) return;

        Debug.Log($"[Turn End] {currentTurn}");

        TurnOwner next = (currentTurn == TurnOwner.Player) ? TurnOwner.Enemy : TurnOwner.Player;
        StartTurn(next);
    }
    private void OnEnable()
    {
        if (GameInputManager.Instance != null)
            GameInputManager.Instance.OnEndTurnPressed += HandleEndTurnShortcut;
    }

    private void OnDisable()
    {
        if (GameInputManager.Instance != null)
            GameInputManager.Instance.OnEndTurnPressed -= HandleEndTurnShortcut;
    }

    private void HandleEndTurnShortcut()
    {
        if (TutorialManager.Instance != null && !TutorialManager.Instance.TryAction("EndTurn"))
            return;

        if (cardManager != null && cardManager.IsCardUseInProgress)
            return;

        if (battleEnded)
            return;

        if (Time.timeScale == 0f)
            return;

        if (currentTurn != TurnOwner.Player)
            return;

        EndTurn();
    }

    private IEnumerator RunEnemyAction(List<EnemyBase> enemyLogic)
    {
        foreach (EnemyBase e in enemyLogic) // 에너미가 여러명일때, 에너미 턴에 한꺼번에 돌아가면서 실행합니다.
        {
            if (e == null)
            {
                Debug.LogError(">> IEnumerator RunEnemyAction(List<EnemyBase> enemyLogic) => enemyLogic에 null이 포함되어 있습니다.");
            }

            if (e.IsAlive())
            {
                if (testRunEnemyAction)
                {
                    Debug.Log($">> BattleManager.RunEnemyAction 현재 에너미 턴 : {e.gameObject}");
                }

                Debug.Log($">> 배틀매니저 : {e.gameObject.name}의 턴");

                yield return StartCoroutine(e.ExecuteEnemyTurn());

                CheckBattleEnd();
                if (battleEnded) yield break;
            }
            else
            {
                CheckBattleEnd();
                if (battleEnded) yield break;
            }

        }

        EndTurn();
    }

    /// <summary>
    /// 플레이어에게 주사위를 지급하는 함수, 기본 1개 + 보너스 주사위 개수만큼 지급.
    /// </summary>
    private IEnumerator RollDiceForPlayerRoutine()
    {
        if (player == null) yield break;

        isRollingDice = true;

        int rollCount = GetBaseDiceRollCount();
        rollCount += player.ConsumeBonusDice();



        PassiveItemCheatingDice cheatingDice = null;
        PassiveItemCloneDice cloneDice = null;

        if (PlayerPassiveItemCollection.Instance != null &&
            PlayerPassiveItemCollection.Instance.ownedPassives != null)
        {
            cheatingDice = PlayerPassiveItemCollection.Instance.ownedPassives
                .Find(x => x is PassiveItemCheatingDice) as PassiveItemCheatingDice;

            cloneDice = PlayerPassiveItemCollection.Instance.ownedPassives
                .Find(x => x is PassiveItemCloneDice) as PassiveItemCloneDice;
        }

        for (int i = 0; i < rollCount; i++)
        {
            int spawnIndex = -1;

            if (i < pendingDicePreviewSlots.Count)
                spawnIndex = pendingDicePreviewSlots[i];

            if (spawnIndex < 0)
                spawnIndex = FindFirstEmptyIndex(player.GetDiceSnapshot());

            if (spawnIndex < 0)
                continue;

            int v = cheatingDice != null
                ? cheatingDice.GetCheatingValue(cheatingDice.currentStock)
                : Random.Range(1, 7);

            if (diceSpawnPreviewUI != null)
                yield return StartCoroutine(diceSpawnPreviewUI.PlayEndAndHide(spawnIndex));

            AddDicePlayer(v);

            if (cloneDice != null && cloneDice.TryDuplicate())
                AddDicePlayer(v);

            yield return new WaitForSeconds(diceSpawnInterval);
        }

        isRollingDice = false;
        RefreshNextDicePreview();
    }
    private void StartNextDicePreview(int nextRollCount)
    {
        if (diceSpawnPreviewUI == null || player == null)
            return;

        int[] predictedSlots = player.GetDiceSnapshot();

        for (int i = 0; i < nextRollCount; i++)
        {
            int index = FindFirstEmptyIndex(predictedSlots);

            if (index < 0)
                break;

            predictedSlots[index] = -1;
            pendingDicePreviewSlots.Add(index);

            StartCoroutine(diceSpawnPreviewUI.PlayStartThenLoop(index));
        }
    }
    private int GetBaseDiceRollCount()
    {
        return Mathf.Min(playerTurnCount, 3);
    }
    private int FindFirstEmptyIndex(int[] slots)
    {
        if (slots == null) return -1;

        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i] == 0)
                return i;
        }

        return -1;
    }
    /// <summary>
    /// 주사위 추가
    /// </summary>
    /// <param name="v"></param>
    private void AddDicePlayer(int v)
    {

        /* if (player.TryAddRolledDice(v, out int addedIndex))
         {
             Debug.Log($"[Dice Added] v={v}, index={addedIndex}");

             if (dicePresenter != null)
                 dicePresenter.Play(v, addedIndex);
         }*/

        // 결과를 변수에 담아서 한 번만 실행합니다.
        bool success = player.TryAddRolledDice(v, out int addedIndex);

        if (success)
        {
            Debug.Log($"[Dice Added Success] v={v}, index={addedIndex}");

            if (dicePresenter != null)
                dicePresenter.Play(v, addedIndex);
        }
        else
        {
            Debug.LogWarning($"[Dice Added Failed] 주사위 슬롯이 가득 찼거나 추가할 수 없습니다. v={v}");
        }

        // 이제 변수를 사용해 로그를 찍으세요.
        Debug.Log($"주사위 추가 실행 결과: {success}");
    }
    private int GetNextBaseDiceRollCount()
    {
        return Mathf.Min(playerTurnCount + 1, 3);
    }
    public void RefreshNextDicePreview()
    {
        if (diceSpawnPreviewUI == null || player == null)
            return;

        diceSpawnPreviewUI.HideAll();
        pendingDicePreviewSlots.Clear();

        int nextRollCount = GetNextBaseDiceRollCount() + player.PeekBonusDiceNextTurn();

        StartNextDicePreview(nextRollCount);
    }

    private void HandlePlayerDiceChanged(int[] diceSlots)
    {
        if (battleEnded) return;
        if (currentTurn != TurnOwner.Player) return;
        if (isRollingDice) return;

        RefreshNextDicePreview();
    }
    /// <summary>
    /// 승리/패배 조건 체크 및 전투 결과 UI 표시
    /// </summary>
    ///
    private List<RewardData> CreateRewards()
    {
        List<RewardData> rewards = new List<RewardData>();

        foreach (EnemyData e in cachedEnemyData)
        {
            if (e != null && e.rewardCoin > 0)
            {
                rewards.Add(new RewardData
                {
                    rewardType = RewardType.Coin,
                    rewardName = "Coin",
                    amount = e.rewardCoin,
                    rewardIcon = null
                });
            }
        }
        // 카오스카드 추가보상합치기
        rewards.AddRange(extraRewards);
        extraRewards.Clear();

        return rewards;
    }

    private void CheckBattleEnd()
    {
        if (battleEnded)
            return;

        if (isEscape) // 전투도망
        {
            battleEnded = true;

            Debug.Log("[battle] Escape!");

            if (battleResultUI != null)
            {
                battleResultUI.ShowDefeat();
            }
        }

        if (player != null && player.CurrentHp <= 0)
        {
            var lifeChip = PlayerPassiveItemCollection.Instance.ownedPassives
                .Find(x => x is PassiveItemLifeChip) as PassiveItemLifeChip;

            if(lifeChip != null && lifeChip.currentStock > 0)
            {
                lifeChip.Revive(player);
                return;
            }

            battleEnded = true;
            if (battleResultUI != null)
            {
                battleResultUI.ShowDefeat();
            }
            return;
        }

        if (EnemyContainer.instance.GetEnemyCount() <= 0)
        {
            battleEnded = true;

            if (TutorialManager.Instance != null && TutorialManager.Instance.IsTutorialActive)
            {
                BattleTutorialController tutorialCtrl = FindObjectOfType<BattleTutorialController>();
                if (tutorialCtrl != null)
                {
                    tutorialCtrl.OnEnemyDead();
                }
            }

            StartCoroutine(ShowVictoryAfterDelay());
            return;
        }

        //// 상점 리롤 횟수 초기화
        //SkillCardShopManager.Instance.ResetRerollCount();
    }
    private IEnumerator ShowVictoryAfterDelay()
    {
        yield return new WaitForSeconds(1.5f);

        List<RewardData> rewards = CreateRewards();

        if (battleResultUI != null)
        {
            battleResultUI.ShowVictory(rewards);
        }
    }
    private bool isEscape = false;
    public void RequestEsacape()
    {
        isEscape = true;
    }

    // 카오스카드 보상데이터
    private List<RewardData> extraRewards = new List<RewardData>();

    public void AddExtraReward(RewardData reward)
    {
        if (reward == null) return;
        extraRewards.Add(reward);
        Debug.Log($"[BattleManager] 추가 보상 등록: {reward.rewardName}");
    }

    /// <summary>
    /// [튜토리얼 전용] 특정 눈금의 주사위를 원하는 개수만큼 지급합니다.
    /// </summary>
    public void GiveTutorialDice(int value, int count)
    {
        for (int i = 0; i < count; i++)
        {
            AddDicePlayer(value); 
        }
    }
}
