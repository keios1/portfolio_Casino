using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 플레이어의 카드 핸드를 관리하는 UI 클래스.
/// 덱으로부터 카드를 가져와 생성하고, 카드 사용/제거에 따른 UI 갱신을 담당한다.
/// 카드 프리팹을 생성하여 CardUI, CardView 등과 연결하며,
/// 카드 사용 결과에 따라 핸드 상태를 동기화한다.
/// </summary>
public class CardHandUI : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Transform handRoot;
    [SerializeField] private GameObject cardPrefab;

    [Header("System")]
    [SerializeField] private CardManager cardManager;

    [Header("Start Cards")]
    [SerializeField] private List<SkillCardData> startCards = new List<SkillCardData>();

    [Header("Hand Layout")]
    [Tooltip("손패가 차지할 수 있는 전체 최대 가로 폭입니다. 카드 수가 많아질수록 이 범위 안에 맞춰 간격이 줄어듭니다.")]
    [SerializeField] private float maxHandWidth = 850f;
    [Tooltip("카드 사이의 최대 간격입니다. 카드 수가 적을 때는 이 값까지 벌어지고, 많아지면 maxHandWidth에 맞춰 줄어듭니다.")]
    [SerializeField] private float maxSpacing = 75f;
    [Tooltip("손패를 부채꼴 곡선으로 배치할 때 위아래로 휘어지는 정도입니다. 값이 클수록 곡선이 더 강해집니다.")]
    [SerializeField] private float curveHeight = 60f;
    [Tooltip("손패 양 끝 카드의 최대 회전 각도입니다. 값이 클수록 부채꼴 느낌이 강해집니다.")]
    [SerializeField] private float maxAngle = 15f;
    [Tooltip("손패 양끝 카드를 추가로 아래로 내리는 정도입니다. 값이 클수록 양끝 카드가 더 내려갑니다.")]
    [SerializeField] private float edgeLowerAmount = 15f;
    [Tooltip("손패 전체의 기본 Y 위치입니다. 값이 작을수록 아래로 내려가고, 클수록 위로 올라갑니다.")]
    [SerializeField] private float centerY = -80f;
    [Tooltip("손패 곡선의 형태를 조절합니다. 값이 클수록 가운데 쪽 곡선 변화가 강해집니다.")]
    [SerializeField] private float curvePower = 1.5f;
    [Tooltip("카드 하나에 Hover 되었을 때, 주변 카드들을 좌우로 밀어내는 거리입니다.")]
    [SerializeField] private float hoverSidePush = 100f;
    [Tooltip("Hover 된 카드의 회전 각도입니다. 0이면 Hover 시 카드가 똑바로 섭니다.")]
    [SerializeField] private float hoverStraightenAngle = 0f;

    [Header("Hover Position")]
    [Tooltip("Hover 된 카드가 이동할 Y 위치입니다. 값이 클수록 카드가 위로 더 올라옵니다.")]
    [SerializeField] private float hoverTargetY = 120f;
    [Tooltip("Hover 된 카드의 확대 배율입니다.")]
    [SerializeField] private float hoverScale = 2f;

    [Header("Hover Stability")]
    [Tooltip("Hover 인식을 시작할 수 있는 최소 Y 범위입니다. 이 값보다 아래에 마우스가 있으면 Hover가 해제됩니다.")]
    [SerializeField] private float hoverAreaYMin = -180f;
    [Tooltip("Hover 인식을 허용하는 최대 Y 범위입니다. 이 값보다 위에 마우스가 있으면 Hover가 해제됩니다.")]
    [SerializeField] private float hoverAreaYMax = 35f;
    [Tooltip("Hover 판정 흔들림을 줄이기 위한 여유값입니다.")]
    //[SerializeField] private float hoverDeadZone = 8f;
    //[Tooltip("Hover 판정이 시작되는 최대 X 거리입니다. 이 값보다 멀리 있으면 Hover가 해제됩니다.")]
    [SerializeField] private float hoverMaxXDistance = 80f;

    [Header("Draw Animation")]
    [SerializeField] private RectTransform deckDrawPoint;
    [SerializeField] private RectTransform drawAnimationRoot;
    [SerializeField] private float drawMoveTime = 0.35f;
    [SerializeField] private float drawStartScale = 0.25f;
    [SerializeField] private float drawEndScale = 1f;

    [Header("Draw Seed")]
    [SerializeField] private bool useFixedShuffleSeed = true;
    [SerializeField] private int testShuffleSeed = 12345;

    private readonly List<Vector2> basePositions = new List<Vector2>();
    private bool isDraggingCard;
    private RectTransform handRootRect;
    private int hoveredIndex = -1;
    private readonly List<SkillCardData> drawDeck = new List<SkillCardData>();
    private readonly List<SkillCardData> hand = new List<SkillCardData>();
    private readonly List<GameObject> spawned = new List<GameObject>();
    private readonly List<SkillCardData> returnNextTurnCards = new();

    private Player player;

    private void Awake()
    {
        if (cardManager == null)
            cardManager = FindObjectOfType<CardManager>();
        handRootRect = handRoot as RectTransform;
    }

    private void Start()
    {
        player = FindObjectOfType<Player>();
        if (player != null)
        {
            player.OnDiceChanged += HandleDiceChanged;
        }

        InitializeBattleDeck();
    }
    private void Update()
    {
        UpdateHoverByMousePosition();
    }

    private void OnDestroy()
    {
        if (player != null)
        {
            player.OnDiceChanged -= HandleDiceChanged;
        }
    }

    // 주사위 배열이 변할 때마다 호출되는 함수
    private void HandleDiceChanged(int[] dummyArray)
    {
        UpdatePlayableVisuals();
    }

    /// <summary>
    /// 손에 있는 모든 카드를 검사해서 투명도를 조절하는 함수
    /// </summary>
    public void UpdatePlayableVisuals()
    {
        if (player == null || cardManager == null) return;

        int currentDice = player.GetRemainingDiceCount();

        for (int i = 0; i < spawned.Count; i++)
        {
            if (spawned[i] == null) continue;

            CardUI ui = spawned[i].GetComponent<CardUI>();
            if (ui != null && ui.cardData != null)
            {
                bool hasEnoughDice = currentDice >= ui.cardData.diceCost;
                bool withinLimit = cardManager.CanUseCardByLimit(ui.cardData, ui);

                // 이 카드가 패링 전용 카드인지 확인 (ID로 구별)
                bool isParryCard = (ui.cardData.id == 32050 || ui.cardData.id == 42050 || ui.cardData.id == 42056 || ui.cardData.id == 22050);

                if (isParryCard)
                {
                    ui.SetPlayableVisual(false);
                }
                else
                {
                    ui.SetPlayableVisual(hasEnoughDice && withinLimit);
                }
            }
        }
    }

    //public void InitializeFromDeck()
    //{
    //    hand.Clear();

    //    List<SkillCardData> sourceDeck = null;

    //    if (PlayerDeckLoadout.Instance != null)
    //        sourceDeck = PlayerDeckLoadout.Instance.GetCurrentDeck();
    //    else
    //        sourceDeck = startCards; // 디버그/예비용

    //    if (sourceDeck == null) return;

    //    for (int i = 0; i < sourceDeck.Count; i++)
    //        Draw(sourceDeck[i]);
    //}

    public void Draw(SkillCardData card)
    {
        if (card == null)
            return;

        hand.Add(card);
        RefreshUI();
    }

    private bool IsBasicCard(SkillCardData card)
    {
        if (card == null)
            return false;

        return card.id == 11000 || card.id == 11007;
    }

    // CardUI가 실제 사용 성공 후 호출
    public void NotifyCardUsed(SkillCardData card)
    {
        if (card == null)
            return;

        // 소비형
        if (card.useLimitType == CardUseLimitType.Limited)
        {
            hand.Remove(card);
        }

        // Unlimited
        else
        {
            hand.Remove(card);

            // 다음 턴 복귀 대기열
            returnNextTurnCards.Add(card);
        }

        RefreshUI();
    }
    public void RestoreUnlimitedCards()
    {
        if (returnNextTurnCards.Count == 0)
            return;

        foreach (var card in returnNextTurnCards)
        {
            hand.Add(card);
        }

        returnNextTurnCards.Clear();

        RefreshUI();
    }
    public void RefreshLayout()
    {
        RefreshHandLayout();
    }

    public Transform GetHandRoot()
    {
        return handRoot;
    }

    public void ClearHoverForce()
    {
        hoveredIndex = -1;
        RefreshHandLayout();
    }
    public void SetDraggingCard(bool dragging, bool refreshWhenEnd = true)
    {
        isDraggingCard = dragging;

        if (!dragging)
        {
            hoveredIndex = -1;

            if (refreshWhenEnd)
                RefreshHandLayout();
        }
    }

    private void UpdateHoverByMousePosition()
    {
        if (isDraggingCard)
            return;

        if (handRootRect == null || spawned.Count == 0)
        {
            SetHoverIndex(-1);
            return;
        }

        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
            handRootRect,
            Input.mousePosition,
            null,
            out Vector2 mouseLocal))
        {
            SetHoverIndex(-1);
            return;
        }

        if (mouseLocal.y < hoverAreaYMin || mouseLocal.y > hoverAreaYMax)
        {
            SetHoverIndex(-1);
            return;
        }

        int nearestIndex = -1;
        float nearestDistance = float.MaxValue;

        for (int i = 0; i < spawned.Count; i++)
        {
            if (spawned[i] == null)
                continue;

            CardUI cardUI = spawned[i].GetComponent<CardUI>();
            if (cardUI == null || !cardUI.IsInHandLayout())
                continue;

            if (i >= basePositions.Count)
                continue;

            float distance = Mathf.Abs(mouseLocal.x - basePositions[i].x);

            if (distance < nearestDistance)
            {
                nearestDistance = distance;
                nearestIndex = i;
            }
        }

        if (nearestIndex >= 0 && nearestDistance <= hoverMaxXDistance)
        {
            SetHoverIndex(nearestIndex);
        }
        else
        {
            SetHoverIndex(-1);
        }
    }

    private void SetHoverIndex(int index)
    {
        if (hoveredIndex == index)
            return;

        hoveredIndex = index;
        RefreshHandLayout();
    }
    private int GetCardIndex(CardUI cardUI)
    {
        for (int i = 0; i < spawned.Count; i++)
        {
            if (spawned[i] == null)
                continue;

            if (spawned[i].GetComponent<CardUI>() == cardUI)
                return i;
        }

        return -1;
    }
    private void RefreshHandLayout()
    {
        if (!gameObject.activeInHierarchy)
            return;

        if (handRoot == null || !handRoot.gameObject.activeInHierarchy)
            return;

        int count = spawned.Count;

        if (count == 0)
            return;

        float spacing = maxSpacing;

        if (count > 1)
        {
            float widthBasedSpacing = maxHandWidth / (count - 1);
            spacing = Mathf.Min(maxSpacing, widthBasedSpacing);
        }
        while (basePositions.Count < count)
            basePositions.Add(Vector2.zero);

        while (basePositions.Count > count)
            basePositions.RemoveAt(basePositions.Count - 1);
        for (int i = 0; i < count; i++)
        {
            if (spawned[i] == null)
                continue;

            RectTransform rect = spawned[i].GetComponent<RectTransform>();
            if (rect == null)
                continue;
            CardUI cardUI = spawned[i].GetComponent<CardUI>();
            if (cardUI == null || !cardUI.IsInHandLayout())
                continue;
            float t = count == 1 ? 0.5f : (float)i / (count - 1);

            float x = (i - (count - 1) * 0.5f) * spacing;

            float curve = Mathf.Sin(t * Mathf.PI);
            curve = Mathf.Clamp01(curve);
            curve = Mathf.Pow(curve, curvePower);

            float y = centerY + (curve - 0.5f) * curveHeight;

            if (count >= 2 && (i == 0 || i == count - 1))
            {
                y -= edgeLowerAmount;
            }

            float zRot = Mathf.Lerp(maxAngle, -maxAngle, t);
            basePositions[i] = new Vector2(x, y);
            Vector3 targetScale = Vector3.one;

            if (hoveredIndex >= 0)
            {
                if (i == hoveredIndex)
                {
                    y = hoverTargetY;
                    zRot = hoverStraightenAngle;
                    targetScale = Vector3.one * hoverScale;
                }
                else if (i < hoveredIndex)
                {
                    x -= hoverSidePush;
                }
                else if (i > hoveredIndex)
                {
                    x += hoverSidePush;
                }
            }
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);

            rect.anchoredPosition = new Vector2(x, y);
            rect.localRotation = Quaternion.Euler(0f, 0f, zRot);
            rect.localScale = targetScale;

            if (rect.gameObject.activeInHierarchy && handRoot.gameObject.activeInHierarchy)
                rect.SetSiblingIndex(i);
        }

        if (hoveredIndex >= 0 && hoveredIndex < spawned.Count && spawned[hoveredIndex] != null)
        {
            spawned[hoveredIndex].transform.SetAsLastSibling();
        }
    }
    private void RefreshUI()
    {
        for (int i = 0; i < spawned.Count; i++)
        {
            if (spawned[i] != null)
                Destroy(spawned[i]);
        }
        spawned.Clear();

        if (handRoot == null || cardPrefab == null)
        {
            Debug.LogError("[CardHandUI] handRoot 또는 cardPrefab이 NULL");
            return;
        }

        for (int i = 0; i < hand.Count; i++)
        {
            GameObject go = Instantiate(cardPrefab, handRoot);
            spawned.Add(go);

            SkillCardData card = hand[i];

            //CardButtonProxy proxy = go.GetComponent<CardButtonProxy>();
            //if (proxy != null)
            //    proxy.Setup(cardManager, card);

            CardUI ui = go.GetComponent<CardUI>();
            if (ui != null)
                ui.Setup(cardManager, card);

            //CardView view = go.GetComponent<CardView>();
            //if (view != null)
            //    view.Bind(card, UseCard);
        }
        RefreshHandLayout();
        // 새로 카드가 뽑히거나 UI가 갱신된 직후에도 알파값 상태를 한번 맞춤
        UpdatePlayableVisuals();
    }

    //private void UseCard(CardView view)
    //{
    //    if (view == null || view.Data == null)
    //        return;

    //    GameObject go = view.gameObject;

    //    // 1순위: 프록시가 있으면 프록시로 처리
    //    CardButtonProxy proxy = go.GetComponent<CardButtonProxy>();
    //    if (proxy != null)
    //    {
    //        proxy.OnClickUseCard();
    //        return;
    //    }

    //    // 2순위: CardUI가 있으면 requesterUI와 함께 실행
    //    CardUI ui = go.GetComponent<CardUI>();
    //    if (ui != null && cardManager != null)
    //    {
    //        CardUseRequestResult result = cardManager.TryUseCard(view.Data, ui);
    //        switch (result)
    //        {
    //            case CardUseRequestResult.Failed:
    //                return;

    //            case CardUseRequestResult.Pending:
    //                return;

    //            case CardUseRequestResult.Succeeded:
    //                return;
    //        }
    //        return;
    //    }
    //}

    public List<SkillCardData> GetAllStartCardsForDebug()
    {
        return startCards;
    }

    // 패링 전용 헬퍼 함수
    public List<SkillCardData> GetParryCards()
    {
        // 32050(무효), 42050(반사), 42056(흡수) 세 종류의 카드를 모두 찾습니다
        return hand.FindAll(card => card.id == 32050 || card.id == 42050 || card.id == 42056 || card.id == 22050);
    }

    public void ConsumeParryCard(SkillCardData card)
    {
        // 패링에 쓴 카드를 손패에서 지우고 UI를 새로고침합니다.
        if (hand.Contains(card))
        {
            hand.Remove(card);
            RefreshUI();
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public void InitializeBattleDeck()
    {
        hand.Clear();
        drawDeck.Clear();
        returnNextTurnCards.Clear();

        List<SkillCardData> sourceDeck = null;

        if (PlayerDeckLoadout.Instance != null)
            sourceDeck = PlayerDeckLoadout.Instance.GetCurrentDeck();
        else
            sourceDeck = startCards;

        if (sourceDeck == null)
            return;

        for (int i = 0; i < sourceDeck.Count; i++)
        {
            if (sourceDeck[i] != null)
                drawDeck.Add(sourceDeck[i]);
        }

        ShuffleDrawDeck();

        if (HasAnyBasicCardInDeck())
            DrawStartingHandWithBasicCards();
        else
            DrawRandomCards(4);
    }

    private void ShuffleDrawDeck()
    {
        System.Random rng = useFixedShuffleSeed
            ? new System.Random(testShuffleSeed)
            : new System.Random();

        for (int i = drawDeck.Count - 1; i > 0; i--)
        {
            int rand = rng.Next(0, i + 1);

            SkillCardData temp = drawDeck[i];
            drawDeck[i] = drawDeck[rand];
            drawDeck[rand] = temp;
        }
    }
    public void DrawRandomCards(int count)
    {
        for (int i = 0; i < count; i++)
            DrawRandomCard();
    }

    public void DrawRandomCard()
    {
        if (drawDeck.Count <= 0)
            return;

        SkillCardData card = drawDeck[0];
        drawDeck.RemoveAt(0);

        hand.Add(card);
        RefreshUI();
    }
    public void DrawRandomCardWithAnimation()
    {
        StartCoroutine(DrawRandomCardRoutine());
    }

    private IEnumerator DrawRandomCardRoutine()
    {
        if (drawDeck.Count <= 0)
        {
            Debug.Log("[CardHandUI] 더 이상 뽑을 카드가 없습니다.");
            yield break;
        }

        if (deckDrawPoint == null || drawAnimationRoot == null || cardPrefab == null)
        {
            DrawRandomCard();
            yield break;
        }

        SkillCardData card = drawDeck[0];
        drawDeck.RemoveAt(0);

        RectTransform rootRect = drawAnimationRoot;

        Vector2 startPos = ConvertToLocalPosition(deckDrawPoint, rootRect);
        Vector2 endPos = GetNextHandCardWorldPositionAsLocal(rootRect);

        GameObject animCard = Instantiate(cardPrefab, drawAnimationRoot);
        RectTransform animRect = animCard.GetComponent<RectTransform>();

        CardUI animUI = animCard.GetComponent<CardUI>();
        if (animUI != null)
        {
            animUI.Setup(cardManager, card);
            animUI.SetDraggingEnabled(false);
        }

        animRect.anchorMin = new Vector2(0.5f, 0.5f);
        animRect.anchorMax = new Vector2(0.5f, 0.5f);
        animRect.pivot = new Vector2(0.5f, 0.5f);

        animRect.anchoredPosition = startPos;
        animRect.localScale = Vector3.one * drawStartScale;
        animRect.localRotation = Quaternion.identity;

        float timer = 0f;

        while (timer < drawMoveTime)
        {
            timer += Time.deltaTime;

            float t = Mathf.Clamp01(timer / drawMoveTime);
            t = Mathf.SmoothStep(0f, 1f, t);

            animRect.anchoredPosition = Vector2.Lerp(startPos, endPos, t);
            animRect.localScale = Vector3.Lerp(
                Vector3.one * drawStartScale,
                Vector3.one * drawEndScale,
                t
            );

            yield return null;
        }

        Destroy(animCard);

        hand.Add(card);
        RefreshUI();
    }
    private Vector2 ConvertToLocalPosition(RectTransform target, RectTransform root)
    {
        Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(null, target.position);

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            root,
            screenPoint,
            null,
            out Vector2 localPoint
        );

        return localPoint;
    }
    private Vector2 GetNextHandCardWorldPositionAsLocal(RectTransform root)
    {
        Vector2 nextHandLocalPos = GetNextHandCardLocalPosition();

        Vector3 worldPos = handRootRect.TransformPoint(nextHandLocalPos);

        Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(null, worldPos);

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            root,
            screenPoint,
            null,
            out Vector2 localPoint
        );

        return localPoint;
    }
    private Vector2 GetNextHandCardLocalPosition()
    {
        int nextCount = hand.Count + 1;

        float spacing = maxSpacing;

        if (nextCount > 1)
        {
            float widthBasedSpacing = maxHandWidth / (nextCount - 1);
            spacing = Mathf.Min(maxSpacing, widthBasedSpacing);
        }

        int index = nextCount - 1;
        float x = (index - (nextCount - 1) * 0.5f) * spacing;

        float t = nextCount == 1 ? 0.5f : (float)index / (nextCount - 1);

        float curve = Mathf.Sin(t * Mathf.PI);
        curve = Mathf.Clamp01(curve);
        curve = Mathf.Pow(curve, curvePower);

        float y = centerY + (curve - 0.5f) * curveHeight;

        if (nextCount >= 2 && index == nextCount - 1)
            y -= edgeLowerAmount;

        return new Vector2(x, y);
    }
    private bool HasAnyBasicCardInDeck()
    {
        for (int i = 0; i < drawDeck.Count; i++)
        {
            if (IsBasicCard(drawDeck[i]))
                return true;
        }

        return false;
    }

    private void DrawStartingHandWithBasicCards()
    {
        List<SkillCardData> startHand = new List<SkillCardData>();

        // Basic 계열 카드 먼저 뽑기: 11000, 11007
        for (int i = drawDeck.Count - 1; i >= 0; i--)
        {
            if (startHand.Count >= 2)
                break;

            if (IsBasicCard(drawDeck[i]))
            {
                startHand.Add(drawDeck[i]);
                drawDeck.RemoveAt(i);
            }
        }

        // 나머지 2장은 Basic 제외 카드
        for (int i = drawDeck.Count - 1; i >= 0; i--)
        {
            if (startHand.Count >= 4)
                break;

            if (!IsBasicCard(drawDeck[i]))
            {
                startHand.Add(drawDeck[i]);
                drawDeck.RemoveAt(i);
            }
        }

        // 혹시 덱 구성이 부족하면 남은 카드로 채움
        while (startHand.Count < 4 && drawDeck.Count > 0)
        {
            startHand.Add(drawDeck[0]);
            drawDeck.RemoveAt(0);
        }

        for (int i = 0; i < startHand.Count; i++)
            hand.Add(startHand[i]);

        RefreshUI();
    }
}
