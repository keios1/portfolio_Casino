using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public enum CardUseRequestResult
{
    Failed,
    Pending,
    Succeeded
}

public class CardManager : MonoBehaviour
{
    public static CardManager instance;

    [SerializeField] private BattleManager battleManager;
    [SerializeField] private Player player;
    [SerializeField] private PlayerCardCollection cardCollection;
    [SerializeField] private List<EnemyBase> enemies = new List<EnemyBase>();

    [Header("Target Selection")]
    [SerializeField] private EnemyBase selectedEnemy;
    public bool selectedEnemyButtonClicked;
    public EnemyBase selectedEnemyDynamic;

    [Header("Dice Selection Pay")]
    [SerializeField] private DiceUI diceUI;

    [Header("Cancel Area")]
    [SerializeField] private GameObject cancelAreaObject;

    private SkillCardData pendingCard;
    private CardUI pendingRequesterUI;
    private int pendingCost;
    private readonly List<int> pendingConsumed = new List<int>(6);
    private int[] pendingDiceSnapshot;

    private bool isWaitingDiceSelection;
    private bool isWaitingEnemySelection;

    public bool IsWaitingDiceSelection => isWaitingDiceSelection;
    public bool IsWaitingEnemySelection => isWaitingEnemySelection;
    public bool IsCardUseInProgress => isWaitingDiceSelection || isWaitingEnemySelection;
    public int PendingNeedCount => IsWaitingDiceSelection ? Mathf.Max(0, pendingCost - pendingConsumed.Count) : 0;

    // 소비형 카드는 "복사본 1장" 단위로 전투 중 1회 사용 후 소멸
    private readonly HashSet<int> consumedLimitedCardInstances = new HashSet<int>();

    // 무제한 카드는 "복사본 1장" 단위로 턴당 1회 사용
    private readonly HashSet<int> usedUnlimitedCardInstancesThisTurn = new HashSet<int>();

    // UI 표시용으로, 이번 전투에서 사용한 소비형 카드 데이터 목록 (복사본이 아닌 원본 데이터 기준)
    private readonly List<SkillCardData> usedLimitedCardsThisBattle = new List<SkillCardData>();
    private void Awake()
    {
        instance = this;

        if (player == null)
            player = FindObjectOfType<Player>();

        if (battleManager == null)
            battleManager = FindObjectOfType<BattleManager>();

        if (cardCollection == null)
            cardCollection = PlayerCardCollection.Instance != null
                ? PlayerCardCollection.Instance
                : FindObjectOfType<PlayerCardCollection>();

        if (cancelAreaObject != null)
            cancelAreaObject.SetActive(false);
    }

    private void Start()
    {
        if (EnemyContainer.instance != null)
        {
            enemies = EnemyContainer.instance.enemies;
        }
        else
        {
            Debug.LogError($"EnemyContainer 프리펩이 씬에 존재하지 않습니다. 해결 방법 Assets>Prefabs>Enemies>EnemyContainer 을 현재 씬에 추가하고, 에너미와 연결하시오.");
        }
    }


    // 전투 시작 시 전체 카드 사용 기록 초기화
    public void ResetBattleUsage()
    {
        consumedLimitedCardInstances.Clear();
        usedUnlimitedCardInstancesThisTurn.Clear();
        usedLimitedCardsThisBattle.Clear();
    }

    // 플레이어 턴 시작 시 무제한 카드의 턴당 사용 기록 초기화
    public void ResetTurnUsage()
    {
        usedUnlimitedCardInstancesThisTurn.Clear();
    }

    public void SetCardCollection(PlayerCardCollection collection)
    {
        cardCollection = collection;
    }

    // 소비형 카드의 현재 보유 장수 반환, 무제한 카드는 int.MaxValue 취급
    private int GetOwnedCardCount(SkillCardData card)
    {
        if (card == null)
            return 0;

        if (card.useLimitType == CardUseLimitType.Unlimited)
            return int.MaxValue;

        if (cardCollection == null)
        {
            Debug.LogWarning("[CardManager] PlayerCardCollection 참조 없음 - 소비형 카드 보유 수량을 0으로 처리합니다.");
            return 0;
        }

        return Mathf.Max(0, cardCollection.GetOwnedCount(card));
    }

    // UI용: -1이면 무제한
    public int GetOwnedCountForUI(SkillCardData card)
    {
        int remain = GetOwnedCardCount(card);
        return remain == int.MaxValue ? -1 : remain;
    }

    // 로직용: -1이면 무제한
    public int GetOwnedCountForLogic(SkillCardData card)
    {
        int remain = GetOwnedCardCount(card);
        return remain == int.MaxValue ? -1 : remain;
    }

    private int GetCardInstanceKey(CardUI requesterUI)
    {
        if (requesterUI == null)
        {
            Debug.LogError("[CardManager] CardUI 없이 카드 복사본을 식별할 수 없습니다.");
            return 0;
        }

        return requesterUI.GetInstanceID();
    }

    // 카드 사용 가능 여부 판단
    // Limited   : 복사본 1장당 전투 중 1회 사용 후 소멸
    // Unlimited : 복사본 1장당 턴당 1회 사용
    public bool CanUseCardByLimit(SkillCardData card, CardUI requesterUI)
    {
        if (card == null || requesterUI == null)
            return false;

        int cardInstanceKey = GetCardInstanceKey(requesterUI);
        if (cardInstanceKey == 0)
            return false;

        if (card.useLimitType == CardUseLimitType.Limited)
        {
            if (consumedLimitedCardInstances.Contains(cardInstanceKey))
                return false;

            int remainOwned = GetOwnedCardCount(card);
            if (remainOwned <= 0)
                return false;

            return true;
        }

        return !usedUnlimitedCardInstancesThisTurn.Contains(cardInstanceKey);
    }

    // 사용 성공 시 카드 복사본 사용 기록 반영
    private void MarkCardUsed(SkillCardData card, CardUI requesterUI)
    {
        if (card == null || requesterUI == null)
            return;

        int cardInstanceKey = GetCardInstanceKey(requesterUI);
        if (cardInstanceKey == 0)
            return;

        if (card.useLimitType == CardUseLimitType.Limited)
            consumedLimitedCardInstances.Add(cardInstanceKey);
        else
            usedUnlimitedCardInstancesThisTurn.Add(cardInstanceKey);
    }

    // 카드 구매/테스트 지급 시 소비형 카드 보유 장수 증가
    public void AddOwnedCardCopy(SkillCardData card, int amount = 1)
    {
        if (card == null || amount <= 0)
            return;

        if (card.useLimitType == CardUseLimitType.Unlimited)
            return;

        if (cardCollection == null)
        {
            Debug.LogWarning("[CardManager] PlayerCardCollection 참조 없음 - AddOwnedCardCopy 무시됨");
            return;
        }

        cardCollection.AddCard(card, amount);
    }

    // 카드 사용 시 주사위 선택이 필요한 경우 선택 모드 진입, 아니면 즉시 실행
    public CardUseRequestResult TryUseCard(SkillCardData card, CardUI requesterUI)
    {
        if (card == null)
            return CardUseRequestResult.Failed;

        if (requesterUI == null)
        {
            Debug.LogError("[CardManager] TryUseCard는 CardUI와 함께 호출되어야 합니다.");
            return CardUseRequestResult.Failed;
        }

        if (TutorialManager.Instance != null && TutorialManager.Instance.IsTutorialActive)
        {
            string requiredAction = "SelectCard"; // 기본 액션 이름

            // 1. 카드 종류(또는 ID)에 따라 매니저에게 물어볼 액션 이름을 다르게 설정합니다.
            if (card.id == 41005)
            {
                requiredAction = "SelectTriumph"; // 트라이엄프 전용
            }
            // 공격 카드 ID
            else if (card.id == 11000)
            {
                requiredAction = "Basic_Card";
            }
            // 방어 카드 ID 
            else if (card.id == 11007)
            {
                requiredAction = "Basic_Shield";
            }

            if (!TutorialManager.Instance.TryAction(requiredAction))
            {
                Debug.Log($"[튜토리얼] 지금은 {card.cardName} 카드를 사용할 타이밍이 아닙니다!");
                return CardUseRequestResult.Failed;
            }
        }

        if (player == null)
        {
            Debug.LogError("[CardManager] Player 참조 없음");
            return CardUseRequestResult.Failed;
        }

        if (card.useLimitType == CardUseLimitType.Limited && cardCollection == null)
        {
            Debug.LogWarning("[CardManager] PlayerCardCollection 참조 없음 - 소비형 카드 사용 불가");
            return CardUseRequestResult.Failed;
        }

        if (IsCardUseInProgress)
        {
            if (pendingCard == card && pendingRequesterUI == requesterUI)
                return CardUseRequestResult.Pending;

            CancelPendingSelection();
            return CardUseRequestResult.Failed;
        }

        if (card.effect == null)
        {
            Debug.LogWarning("[CardManager] card.effect 없음");
            return CardUseRequestResult.Failed;
        }

        if (!CanUseCardByLimit(card, requesterUI))
        {
            Debug.Log("[CardManager] 사용 제한으로 사용 불가");
            return CardUseRequestResult.Failed;
        }

        if (battleManager != null)
        {
            bool isPlayerTurn = battleManager.CurrentTurn == BattleManager.TurnOwner.Player;
            if (card.timing == CardPlayTiming.PlayerTurnOnly && !isPlayerTurn)
            {
                Debug.Log("[CardManager] 내 턴에만 사용 가능");
                return CardUseRequestResult.Failed;
            }
        }

        int cost = Mathf.Clamp(card.diceCost, 0, 6);

        if (cost == 0)
        {
            StartCoroutine(ExecuteZeroCostCard(card, requesterUI));
            return CardUseRequestResult.Pending;
        }

        if (player.GetRemainingDiceCount() < cost)
        {
            Debug.Log("[CardManager] 주사위 부족");
            return CardUseRequestResult.Failed;
        }

        BeginDiceSelection(card, cost, requesterUI);
        Debug.Log($"[Card] 주사위 선택 모드 시작: {card.cardName} / 필요 {cost}개");
        return CardUseRequestResult.Pending;
    }

    private IEnumerator ExecuteZeroCostCard(SkillCardData card, CardUI requesterUI)
    {
        if (card == null)
            yield break;

        if (requesterUI != null)
            requesterUI.EnterPendingUseState();

        yield return new WaitForSeconds(0.7f);

        if (requesterUI != null)
            requesterUI.HideCenterDisplayOnly();

        yield return StartCoroutine(ExecuteCardNow(card, Array.Empty<int>(), requesterUI, null));
    }

    private void BeginDiceSelection(SkillCardData card, int cost, CardUI requesterUI)
    {
        pendingCard = card;
        pendingCost = Mathf.Clamp(cost, 1, 6);
        pendingRequesterUI = requesterUI;
        pendingConsumed.Clear();
        isWaitingDiceSelection = true;
        isWaitingEnemySelection = false;
        pendingDiceSnapshot = player.GetDiceSnapshot();

        RefreshSelectableDiceBlinks();

        if (cancelAreaObject != null)
            cancelAreaObject.SetActive(true);

        if (pendingRequesterUI != null)
            pendingRequesterUI.EnterPendingUseState();
    }

    public void CancelPendingSelection()
    {
        if (!IsCardUseInProgress)
            return;

        Debug.Log("[Card] 주사위 선택 모드 취소");

        if (DiceUI.Instance != null)
        {
            DiceUI.Instance.StopAllBlinks();
        }

        if (cancelAreaObject != null)
            cancelAreaObject.SetActive(false);

        if (player != null && pendingDiceSnapshot != null)
            player.RestoreDiceSnapshot(pendingDiceSnapshot);

        if (pendingRequesterUI != null)
            pendingRequesterUI.CancelPendingUse();

        pendingCard = null;
        pendingCost = 0;
        pendingRequesterUI = null;
        pendingConsumed.Clear();
        pendingDiceSnapshot = null;
        selectedEnemyDynamic = null;
        selectedEnemyButtonClicked = false;

        isWaitingDiceSelection = false;
        isWaitingEnemySelection = false;
    }

    public void ConfirmCancelByBackground()
    {
        CancelPendingSelection();
    }

    private bool IsDiceConditionSatisfied(SkillCardData card, int[] consumed)
    {
        if (card == null)
            return false;

        // 개수 조건
        if (consumed == null || consumed.Length < card.diceCost)
            return false;

        // 합 조건
        if (card.minDiceSum > 0)
        {
            int sum = 0;
            for (int i = 0; i < consumed.Length; i++)
                sum += consumed[i];

            if (sum < card.minDiceSum)
                return false;
        }

        return true;
    }

    public void OnClickDiceSlot(int slotIndex)
    {
        if (!IsWaitingDiceSelection)
            return;

        if (player == null)
        {
            CancelPendingSelection();
            return;
        }

        if (!player.TryConsumeDiceAt(slotIndex, out int value))
            return;

        pendingConsumed.Add(value);

        RefreshSelectableDiceBlinks();

        int need = PendingNeedCount;
        Debug.Log($"[Card] 주사위 선택됨: {value} (남은 필요 {need}개)");

        if (need > 0)
            return;

        if (TutorialManager.Instance != null && TutorialManager.Instance.IsTutorialActive)
        {
            if (!TutorialManager.Instance.TryAction("SelectDice"))
            {
                return; // 주사위 선택 타이밍이 아니면 클릭 무시
            }
        }

        if (DiceUI.Instance != null)
            DiceUI.Instance.StopAllBlinks();

        int[] consumed = pendingConsumed.ToArray();
        SkillCardData card = pendingCard;
        CardUI requester = pendingRequesterUI;
        int[] snapshot = pendingDiceSnapshot;

        if (!IsDiceConditionSatisfied(card, consumed))
        {
            Debug.Log($"[Card] 사용 조건 불충족: {card.cardName}");
            if (DiceUI.Instance != null)
                DiceUI.Instance.StopAllBlinks();

            isWaitingDiceSelection = false;
            isWaitingEnemySelection = false;

            if (player != null && snapshot != null)
                player.RestoreDiceSnapshot(snapshot);

            if (requester != null)
                requester.CancelPendingUse();

            if (cancelAreaObject != null)
                cancelAreaObject.SetActive(false);

            pendingCard = null;
            pendingCost = 0;
            pendingRequesterUI = null;
            pendingConsumed.Clear();
            pendingDiceSnapshot = null;
            return;
        }

        if (cancelAreaObject != null)
            cancelAreaObject.SetActive(false);

        isWaitingDiceSelection = false;
        isWaitingEnemySelection = false;

        if (requester != null)
            requester.EnterResolvingState();

        pendingCard = null;
        pendingCost = 0;
        pendingRequesterUI = null;
        pendingConsumed.Clear();
        pendingDiceSnapshot = null;

        StartCoroutine(ExecuteCardNow(card, consumed, requester, snapshot));
    }
    private void RefreshSelectableDiceBlinks()
    {
        if (DiceUI.Instance == null || player == null)
            return;

        DiceUI.Instance.StopAllBlinks();

        if (!IsWaitingDiceSelection)
            return;

        int[] diceSnapshot = player.GetDiceSnapshot();

        if (diceSnapshot == null)
            return;

        for (int i = 0; i < diceSnapshot.Length; i++)
        {
            if (diceSnapshot[i] > 0)
            {
                DiceUI.Instance.SetSlotBlink(i, true);
            }
        }
    }

    private IEnumerator ExecuteCardNow(SkillCardData card, int[] consumed, CardUI requesterUI, int[] snapshot)
    {
        void RestoreFailState()
        {
            if (cancelAreaObject != null)
                cancelAreaObject.SetActive(false);

            if (player != null && snapshot != null)
                player.RestoreDiceSnapshot(snapshot);

            if (requesterUI != null)
            {
                requesterUI.gameObject.SetActive(true);
                requesterUI.CancelPendingUse();
            }

            if (EnemyContainer.instance != null)
            {
                foreach (EnemyBase e in EnemyContainer.instance.enemies)
                {
                    if (e == null) continue;

                    MouseHover hover = e.GetComponent<MouseHover>();
                    if (hover != null)
                        hover.IsHoverReady = false;
                }
            }

            selectedEnemyDynamic = null;
            selectedEnemyButtonClicked = false;
            isWaitingDiceSelection = false;
            isWaitingEnemySelection = false;

            pendingCard = null;
            pendingCost = 0;
            pendingRequesterUI = null;
            pendingConsumed.Clear();
            pendingDiceSnapshot = null;
        }

        if (card == null || card.effect == null)
        {
            RestoreFailState();
            yield break;
        }

        // 
        // 대충 여기에 에너미 선택
        //-체력이 있는 에너미를 대상으로 함

        if (card.target == CardTarget.EnemySingle && enemies != null && enemies.Count > 0)
        {
            List<EnemyBase> targetableEnemies = new List<EnemyBase>();
            foreach (EnemyBase e in EnemyContainer.instance.enemies)
            {
                if (e.IsAlive() && e.isTargetable)
                {
                    e.GetComponent<MouseHover>().IsHoverReady = true;

                    // e.buttonSelectOrNull.gameObject.SetActive(true);
                }
            }
            selectedEnemyButtonClicked = false;
            //-그런 에너미에게 버튼 나타냄
            //- 카드 쓰고 나면 버튼이 사라짐
            isWaitingEnemySelection = true;
            selectedEnemyButtonClicked = false;
            selectedEnemyDynamic = null;

            // CardManager.cs 내부 ExecuteCardNow 코루틴 안의 while 루프 전체 교체
            while (selectedEnemyButtonClicked == false)
            {
                if (Input.GetMouseButtonDown(0))
                {
                    bool isTutorialActive = TutorialManager.Instance != null && TutorialManager.Instance.IsTutorialActive;

                    // 1. 방패(UI) 클릭을 무시하고 에너미를 클릭할 수 있게 관통
                    if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
                    {
                        if (!isTutorialActive)
                        {
                            yield return null;
                            continue;
                        }
                    }

                    Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

                    if (Physics.Raycast(ray, out RaycastHit hit))
                    {
                        EnemyBase enemy = hit.collider.GetComponentInParent<EnemyBase>();

                        if (enemy != null && enemy.IsAlive() && enemy.isTargetable)
                        {
                            // 2. 에너미를 클릭했을 때 튜토리얼 매니저에게 보고
                            if (isTutorialActive)
                            {
                                Debug.Log("✅ [튜토리얼] 에너미 클릭 감지됨! 매니저에게 SelectEnemy 보고 중...");

                                if (!TutorialManager.Instance.TryAction("SelectEnemy"))
                                {
                                    Debug.Log("❌ [튜토리얼] 매니저가 허락하지 않음! (SO 데이터 확인 필요)");
                                    yield return null;
                                    continue;
                                }

                                Debug.Log("✅ [튜토리얼] 튜토리얼 다음 페이지로 넘어갑니다!");
                            }

                            selectedEnemyDynamic = enemy;
                            selectedEnemyButtonClicked = true;
                            break; // 성공적으로 에너미 선택 후 루프 탈출
                        }
                    }

                    // 3. 허공 클릭 시 취소 방지
                    if (isTutorialActive)
                    {
                        TutorialManager.Instance.TryAction("InvalidClick");
                        yield return null;
                        continue;
                    }
                    else
                    {
                        RestoreFailState();
                        isWaitingEnemySelection = false;
                        yield break;
                    }
                }

                yield return null;
            }

            isWaitingEnemySelection = false;
            foreach (EnemyBase e in EnemyContainer.instance.enemies)
            {
                if (e == null || !e.isTargetable) continue;

                MouseHover hover = e.GetComponent<MouseHover>();
                if (hover != null)
                    hover.IsHoverReady = false;
            }
        }

        BattleContext context = new BattleContext
        {
            Player = player,
            Enemies = enemies,
            SelectedEnemy = selectedEnemyDynamic,
            BattleManager = battleManager //카오스카드 조건추가
        };

        ICardTarget target = null;

        switch (card.target)
        {
            case CardTarget.None:
                target = null;
                break;

            case CardTarget.Self:
                if (context.Player == null)
                {
                    RestoreFailState();
                    yield break;
                }
                target = context.Player;
                break;

            case CardTarget.EnemySingle:
                if (context.SelectedEnemy == null)
                {
                    RestoreFailState();
                    yield break;
                }

                if (context.Enemies == null || !context.Enemies.Contains(context.SelectedEnemy))
                {
                    RestoreFailState();
                    yield break;
                }

                target = context.SelectedEnemy;
                break;

            case CardTarget.EnemyAll:
                if (context.Enemies == null || context.Enemies.Count == 0)
                {
                    RestoreFailState();
                    yield break;
                }
                target = null;
                break;
        }

        bool effectSucceeded = false;
        yield return StartCoroutine(card.effect.Apply(context, target, consumed, result => effectSucceeded = result));

        if (!effectSucceeded)
        {
            RestoreFailState();
            yield break;
        }

        // [주사기] : 공격 직후 플레이어 HP 회복
        if (card.target == CardTarget.EnemySingle || card.target == CardTarget.EnemyAll)
        {
            PassiveItemSyringe syringe = null;

            if (PlayerPassiveItemCollection.Instance != null &&
                PlayerPassiveItemCollection.Instance.ownedPassives != null)
            {
                syringe = PlayerPassiveItemCollection.Instance.ownedPassives
                    .Find(x => x is PassiveItemSyringe) as PassiveItemSyringe;
            }

            if (syringe != null && player != null)
            {
                int healAmount = syringe.GetSyringeHeal();
                player.Heal(healAmount);
            }
        }

        MarkCardUsed(card, requesterUI);

        if (card.useLimitType == CardUseLimitType.Limited)
        {
            usedLimitedCardsThisBattle.Add(card);
        }

        if (requesterUI != null)
            requesterUI.TriggerUsedSuccessfully();
    }
    public void CommitUsedLimitedCards()
    {
        for (int i = 0; i < usedLimitedCardsThisBattle.Count; i++)
        {
            SkillCardData card = usedLimitedCardsThisBattle[i];
            if (card == null)
                continue;

            if (cardCollection == null)
                continue;

            bool isBroken = cardCollection.ConsumeCardDurability(card);

            if (!isBroken)
            {
                Debug.Log($"[CardManager] {card.cardName} 내구도 남음. 상점으로 돌아가지 않음");
                continue;
            }

            bool removedFromCollection = cardCollection.RemoveCard(card, 1);

            if (!removedFromCollection)
            {
                Debug.LogWarning($"[CardManager] 사용 카드 제거 실패: {card.cardName}");
                continue;
            }

            if (PlayerDeckLoadout.Instance != null)
                PlayerDeckLoadout.Instance.RemoveFromDeck(card);

            if (SkillCardShopManager.Instance != null)
                SkillCardShopManager.Instance.ReturnStock(card);
        }

        usedLimitedCardsThisBattle.Clear();
    }
    public void SetSelectedEnemy(EnemyBase enemy)
    {
        selectedEnemy = enemy;
        selectedEnemyDynamic = enemy;
        Debug.Log($"[CardManager] 선택된 적 변경: {(enemy != null ? enemy.name : "없음")}");
    }

    public void OnEnemyRemoved(EnemyBase enemy)
    {
        if (enemy == null)
            return;

        enemies.Remove(enemy);

        if (selectedEnemy == enemy)
        {
            selectedEnemy = null;
            Debug.Log("[CardManager] 선택된 적 제거로 선택 해제");
        }
    }

    private void Update()
    {
#if UNITY_EDITOR
        if (Input.GetKeyDown(KeyCode.F9))
        {
            DebugRefillOwnedCards();
        }
#endif
    }

#if UNITY_EDITOR
    [ContextMenu("DEBUG/Refill Owned Cards")]
    private void DebugRefillOwnedCards()
    {
        if (cardCollection == null)
        {
            Debug.LogWarning("[DEBUG] PlayerCardCollection 없음");
            return;
        }

        CardHandUI hand = FindObjectOfType<CardHandUI>();
        if (hand == null)
        {
            Debug.LogWarning("[DEBUG] CardHandUI 없음");
            return;
        }

        List<SkillCardData> cards = hand.GetAllStartCardsForDebug();

        for (int i = 0; i < cards.Count; i++)
        {
            SkillCardData card = cards[i];
            if (card == null) continue;
            if (card.useLimitType == CardUseLimitType.Unlimited) continue;

            if (cardCollection.GetOwnedCount(card) <= 0)
                cardCollection.AddCard(card, 1);
        }

        Debug.Log("[DEBUG] 소비형 카드 최소 1장 리필 완료");
    }
#endif
}
