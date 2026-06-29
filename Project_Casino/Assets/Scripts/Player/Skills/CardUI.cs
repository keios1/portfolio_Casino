using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 개별 카드의 UI 및 상호작용을 처리하는 클래스.
/// 드래그, 호버, 중앙 확대 표시, 카드 사용 요청 등의 사용자 입력을 처리하며
/// CardManager와 연동하여 카드 사용을 수행한다.
/// </summary>
public class CardUI : MonoBehaviour,
    IBeginDragHandler, IDragHandler, IEndDragHandler,
    IPointerEnterHandler, IPointerExitHandler
{
    [Header("Settings")]
    [SerializeField] private bool canDrag = true;

    [Header("Injected")]
    public SkillCardData cardData;
    [SerializeField] private CardManager cardManager;
    [SerializeField] private CardHandUI handUI;

    [Header("Refs")]
    [SerializeField] private RectTransform visualRoot;

    [Header("FX")]
    public bool useCenterDisplay = true;
    [SerializeField] private Transform showCenterParent;
    [SerializeField] private float centerScale = 1.5f;

    [Header("Hover")]
    [SerializeField] private bool useHover = true;
    //[SerializeField] private int hoverSortingOrder = 100;
    [Header("Card Visual Refs")]
    [SerializeField] private Image cardImage;
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text descText;
    [SerializeField] private TMP_Text remainText;
    [SerializeField] private TMP_Text maxText;

    [Header("Shader Glow Effect")]
    [Tooltip("빛이 서서히 켜지고 꺼지는 속도")]
    public float fadeSpeed = 12f;

    [Header("Colors")]
    [Tooltip("사용 불가능할 때 카드에 덮어씌울 색상")]
    public Color disabledColor = Color.gray;

    [Header("Sound")]
    [SerializeField] private AudioClip useDragDropClip;

    private Material mat;
    private float currentHover;
    private float targetHover;

    private RectTransform rect;
    private Canvas rootCanvas;
    private CanvasGroup canvasGroup;
    private Canvas hoverCanvas;

    private Vector2 originalAnchoredPos;
    private Transform originalParent;
    private Vector3 originalScale;
    private Quaternion originalRotation;
    private int originalSiblingIndex;

    private Vector2 dragOffset;
    private RectTransform dragParentRect;

    private Vector2 visualNormalPos;
    private Vector3 visualNormalScale;

    private bool isHovered;
    private bool isDragging;
    private bool isCenterDisplaying;
    private bool isPendingUse;
    private bool hasCachedOriginalTransform;

    private bool isPlayableCard; // 현재 낼 수 있는 상태인지 기억하는 변수

    private static CardUI currentHovered;

    private bool wasDroppedOnUseArea;// 드래그 종료 시 카드가 UseArea에 떨어졌는지 추적하는 변수
    private void Awake()
    {
        rect = GetComponent<RectTransform>();
        Canvas parentCanvas = GetComponentInParent<Canvas>();
        rootCanvas = parentCanvas != null ? parentCanvas.rootCanvas : null;

        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();

        hoverCanvas = GetComponent<Canvas>();
        if (hoverCanvas == null)
            hoverCanvas = gameObject.AddComponent<Canvas>();

        hoverCanvas.overrideSorting = false;

        if (GetComponent<GraphicRaycaster>() == null)
            gameObject.AddComponent<GraphicRaycaster>();

        if (visualRoot == null)
        {
            Transform child = transform.Find("VisualRoot");
            visualRoot = child != null ? child as RectTransform : rect;
        }

        if (cardManager == null)
            cardManager = FindObjectOfType<CardManager>();

        if (handUI == null)
            handUI = FindObjectOfType<CardHandUI>();

        visualNormalPos = visualRoot != null ? visualRoot.anchoredPosition : Vector2.zero;
        visualNormalScale = visualRoot != null ? visualRoot.localScale : Vector3.one;

        if (cardImage != null)
        {
            mat = new Material(cardImage.material);
            cardImage.material = mat;
            mat.SetFloat("_Hover", 0f);
        }

        RefreshUI();
    }

    private void Update()
    {
        if (mat != null)
        {
            currentHover = Mathf.MoveTowards(currentHover, targetHover, Time.deltaTime * fadeSpeed);
            mat.SetFloat("_Hover", currentHover);
        }
    }
    private void OnDisable()
    {
        isDragging = false;
        isHovered = false;
        targetHover = 0f;
    }

    public void Setup(CardManager manager, SkillCardData data)
    {
        cardManager = manager;
        cardData = data;
        RefreshUI();
    }
    public void SetDraggingEnabled(bool enabled)
    {
        canDrag = enabled;

        if (!enabled)
        {
            isDragging = false;
            isHovered = false;
            targetHover = 0f;

            if (canvasGroup != null)
                canvasGroup.blocksRaycasts = false;

            if (hoverCanvas != null)
                hoverCanvas.overrideSorting = false;
        }
    }
    public void RefreshUI()
    {
        if (cardData == null)
            return;

        if (cardImage != null)
            cardImage.sprite = cardData.cardSprite;

        if (titleText != null)
            titleText.text = cardData.cardName;

        if (descText != null)
            descText.text = cardData.description;

        RefreshDurabilityUI();
    }
    private void RefreshDurabilityUI()
    {
        if (cardData == null)
            return;

        // 기본 카드 / 무한 카드
        if (cardData.useLimitType == CardUseLimitType.Unlimited)
        {
            if (remainText != null)
                remainText.text = "∞";

            if (maxText != null)
                maxText.text = "∞";

            return;
        }

        int maxDurability = Mathf.Max(1, cardData.durability);
        int remainDurability = maxDurability;

        PlayerCardCollection collection = PlayerCardCollection.Instance;
        if (collection != null)
            remainDurability = collection.GetRemainingDurability(cardData);

        if (remainText != null)
            remainText.text = remainDurability.ToString();

        if (maxText != null)
            maxText.text = maxDurability.ToString();
    }
    public bool IsInHandLayout()
    {
        return !isDragging && !isPendingUse && !isCenterDisplaying && gameObject.activeInHierarchy;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!useHover || isDragging || isCenterDisplaying || isPendingUse)
            return;

        if (currentHovered != null && currentHovered != this)
            currentHovered.RestoreHoverImmediate();

        currentHovered = this;

        // 마우스를 올렸을 때 카드를 낼 수 있는 상태라면 빛을 켭니다.
        if (isPlayableCard)
            targetHover = 1f;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (currentHovered == this)
            currentHovered = null;

        RestoreHoverImmediate();
        // 마우스를 떼면, 카드를 낼 수 있는 상태라도 빛을 기본 상태(또는 0)로 끕니다.
        // 만약 낼 수 있는 카드가 항상 빛나길 원한다면 isPlayableCard ? 1f : 0f 로 두셔도 좋습니다.
        targetHover = isPlayableCard ? 1f : 0f;
    }
    private void RestoreHoverImmediate()
    {
        if (!isHovered)
            return;

        isHovered = false;

        if (hoverCanvas != null)
            hoverCanvas.overrideSorting = false;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (!canDrag)
            return;

        if (rect == null || isCenterDisplaying || isPendingUse)
            return;

        isDragging = true;
        wasDroppedOnUseArea = false;

        if (handUI != null)
            handUI.SetDraggingCard(true);

        if (currentHovered == this)
            currentHovered = null;

        RestoreHoverImmediate();

        if (handUI != null)
            handUI.ClearHoverForce();

        CacheOriginalTransformIfNeeded();

        if (rootCanvas != null)
            rect.SetParent(rootCanvas.transform, true);

        rect.localRotation = Quaternion.identity;
        rect.localScale = Vector3.one;

        rect.SetAsLastSibling();
        canvasGroup.blocksRaycasts = false;

        dragParentRect = rect.parent as RectTransform;

        if (dragParentRect == null)
        {
            dragOffset = Vector2.zero;
            return;
        }

        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
            dragParentRect,
            eventData.position,
            eventData.pressEventCamera,
            out Vector2 localMousePos))
        {
            dragOffset = localMousePos - rect.anchoredPosition;
        }
        else
        {
            dragOffset = Vector2.zero;
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!isDragging || rect == null || dragParentRect == null)
            return;

        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
            dragParentRect,
            eventData.position,
            eventData.pressEventCamera,
            out Vector2 localMousePos))
        {
            rect.anchoredPosition = localMousePos - dragOffset;
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (!isDragging)
            return;

        isDragging = false;
        canvasGroup.blocksRaycasts = true;

        // DropArea에서 OnDrop이 호출되지 않았다면 원위치
        if (!wasDroppedOnUseArea)
        {
            ResetPosition();
        }

        wasDroppedOnUseArea = false;

        if (handUI != null)
        {
            handUI.SetDraggingCard(false);
        }

        targetHover = 0f;
    }

    public void TryUseByDropArea()
    {
        wasDroppedOnUseArea = true;

        isDragging = false;
        canvasGroup.blocksRaycasts = true;

        if (cardManager == null || cardData == null)
        {
            if (handUI != null)
                handUI.SetDraggingCard(false);

            ResetPosition();
            return;
        }

        CardUseRequestResult result = cardManager.TryUseCard(cardData, this);

        if (result == CardUseRequestResult.Failed)
        {
            if (handUI != null)
                handUI.SetDraggingCard(false);

            ResetPosition();
            return;
        }

        if (handUI != null)
            handUI.SetDraggingCard(false, false);

        PlayUseDragDropSound();
    }

    public void EnterPendingUseState()
    {
        if (rect == null)
            return;

        CacheOriginalTransformIfNeeded();

        isPendingUse = true;
        isCenterDisplaying = useCenterDisplay;
        isDragging = false;
        canvasGroup.blocksRaycasts = false;

        if (currentHovered == this)
            currentHovered = null;

        RestoreHoverImmediate();

        Transform targetParent = null;

        if (showCenterParent != null)
            targetParent = showCenterParent;
        else if (rootCanvas != null)
            targetParent = rootCanvas.transform;

        if (targetParent != null)
            rect.SetParent(targetParent, false);

        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);

        rect.anchoredPosition = Vector2.zero;
        rect.localRotation = Quaternion.identity;
        rect.localScale = Vector3.one * centerScale;

        rect.SetAsLastSibling();
    }

    public void EnterResolvingState()
    {
        isPendingUse = false;
        isCenterDisplaying = false;
        isDragging = false;

        RestoreHoverImmediate();
        canvasGroup.blocksRaycasts = false;
        gameObject.SetActive(false);
    }

    public void CancelPendingUse()
    {
        isPendingUse = false;
        isCenterDisplaying = false;
        isDragging = false;

        gameObject.SetActive(true);
        ResetPosition();
        canvasGroup.blocksRaycasts = true;
    }

    public void TriggerUsedSuccessfully()
    {
        if (handUI != null)
            handUI.NotifyCardUsed(cardData);

        Destroy(gameObject);
    }

    private void CacheOriginalTransformIfNeeded()
    {
        if (rect == null || hasCachedOriginalTransform)
            return;

        originalAnchoredPos = rect.anchoredPosition;
        originalParent = rect.parent;
        originalScale = rect.localScale;
        originalRotation = rect.localRotation;
        originalSiblingIndex = transform.GetSiblingIndex();
        hasCachedOriginalTransform = true;
    }

    private void ResetPosition()
    {
        if (rect == null)
            return;

        if (handUI != null && handUI.GetHandRoot() != null)
        {
            rect.SetParent(handUI.GetHandRoot(), false);
            rect.localRotation = Quaternion.identity;
            rect.localScale = Vector3.one;
            handUI.RefreshLayout();
        }

        hasCachedOriginalTransform = false;
    }

    public void HideCenterDisplayOnly()
    {
        isPendingUse = false;
        isCenterDisplaying = false;
        isDragging = false;

        RestoreHoverImmediate();
        canvasGroup.blocksRaycasts = false;
        gameObject.SetActive(false);
    }

    public void SetPlayableVisual(bool isPlayable)
    {
        isPlayableCard = isPlayable;

        if (cardImage != null)
        {
            cardImage.color = isPlayable ? Color.white : disabledColor;
        }

        if (canvasGroup != null)
        {
            canvasGroup.alpha = 1.0f;
        }

        targetHover = isPlayable ? 1f : 0f;
    }

    private void PlayUseDragDropSound()
    {
        if (useDragDropClip == null)
            return;

        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayUI(useDragDropClip);
    }

    /// <summary>
    /// </summary>
}
