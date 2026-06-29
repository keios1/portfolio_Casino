using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class SkillCardShopUI : MonoBehaviour
{
    [Tooltip("인스펙터에서 슬롯 4개를 순서대로 연결")]
    public SkillCardSlot[] slots;
    public TextMeshProUGUI rerollCostText;

    [Header("카드 이미지 크게 보여줄 오브젝트")]
    public GameObject bigImage;    

    private CardUI bigCardUI;
    private CardView bigCardView;

    private void Awake()
    {
        if (bigImage != null)
        {
            bigCardUI = bigImage.GetComponent<CardUI>();
            if (bigCardUI == null) bigCardUI = bigImage.GetComponentInChildren<CardUI>();

            bigCardView = bigImage.GetComponent<CardView>();
            if (bigCardView == null) bigCardView = bigImage.GetComponentInChildren<CardView>();

            // 시작할 때 꺼두기
            bigImage.SetActive(false);
        }
    }

    private void OnEnable()
    {
        StartCoroutine(WaitAndRefresh());
    }

    private IEnumerator WaitAndRefresh()
    {
        while (SkillCardShopManager.Instance == null)
            yield return null;

        UpdateRerollText();

        var currentCards = SkillCardShopManager.Instance.currentDisplayedCards;
        for (int i = 0; i < currentCards.Count; i++)
        {
            if (currentCards[i] != null)
            {
                // 현재 카드의 재고가 0인지 확인
                if (SkillCardShopManager.Instance.GetStockCount(currentCards[i]) <= 0)
                {
                    // 재고가 0이면 다른 카드로 교체
                    List<SkillCardData> exclude = GetAllDisplayedCards();
                    SkillCardData nextCard = SkillCardShopManager.Instance.GetSingleCardByRarity(exclude);

                    // 매니저 데이터 갱신
                    SkillCardShopManager.Instance.UpdateDisplayedCard(i, nextCard);
                }
            }
        }

        // 만약 리스트가 아예 비어있다면 새로 생성
        bool isEmpty = true;
        foreach (var card in SkillCardShopManager.Instance.currentDisplayedCards)
        {
            if (card != null) { isEmpty = false; break; }
        }
        if (isEmpty) SkillCardShopManager.Instance.GetRandomCards(4);

        DisplayCurrentData();
        ValidateAllSlots();
        RefreshAllSlotButtons();
    }

    private void UpdateRerollText()
    {
        int cost = SkillCardShopManager.Instance.GetCurrentRerollCost();
        rerollCostText.text = cost.ToString() + "G";
    }

    /// <summary>
    /// 상점 리롤 버튼 클릭시 호출
    /// </summary>
    public void OnClickRefreshShop()
    {
        if (TutorialManager.Instance != null && TutorialManager.Instance.IsTutorialActive)
        {
            Debug.Log("튜토리얼 진행 중에는 리롤을 할 수 없습니다!");
            return; 
        }

        if (SkillCardShopManager.Instance == null)
            return;

        if (PlayerRuntimeData.Instance == null)
            return;

        int currentRerollCost = SkillCardShopManager.Instance.GetCurrentRerollCost();

        if (!PlayerRuntimeData.Instance.SpendCoin(currentRerollCost))
        {
            Debug.Log("플레이어 코인 부족 리롤 실패");
            return;
        }
        Debug.Log($"{PlayerRuntimeData.Instance.coin} 남은 플레이어 코인");
        ExecuteRefresh();

        SkillCardShopManager.Instance.IncreaseRerollCount();
        UpdateRerollText();

        RefreshAllSlotButtons();

        bigImage.SetActive(false);
    }

    /// <summary>
    /// 카드 데이터 슬롯에 배분
    /// </summary>
    public void ExecuteRefresh()
    {
        SkillCardShopManager.Instance.GetRandomCards(4);

        DisplayCurrentData();
    }

    /// <summary>
    /// 매니저에 저장된 데이터를 UI 슬롯에 반영만 하는 함수
    /// </summary>
    private void DisplayCurrentData()
    {
        var managerCards = SkillCardShopManager.Instance.currentDisplayedCards;
        for (int i = 0; i < slots.Length; i++)
        {
            slots[i].slotIndex = i; // 인덱스 부여

            slots[i].SetCardData(i < managerCards.Count ? managerCards[i] : null);
        }
    }

    /*/// <summary>
    /// 현재 화면에 떠 있는 카드들을 재고로 다시 넣음
    /// </summary>
    private void ReturnAllSlotsToStock()
    {
        foreach (var slot in slots)
        {
            if (slot != null && slot.GetCurrentData() != null)
            {
                SkillCardShopManager.Instance.ReturnStock(slot.GetCurrentData());
            }
        }
    }*/

    /// <summary>
    /// 현재 화면에 떠 있는 카드들 반환
    /// </summary>
    /// <returns></returns>
    public List<SkillCardData> GetAllDisplayedCards()
    {
        List<SkillCardData> displayed = new List<SkillCardData>();
        foreach (var slot in slots)
        {
            if (slot != null && slot.GetCurrentData() != null)
            {
                displayed.Add(slot.GetCurrentData());
            }
        }
        return displayed;
    }

    /// <summary>
    /// 모든 슬롯을 돌면서 재고가 0인 카드가 있는지 확인하고 SOLD OUT 시킴
    /// </summary>
    public void ValidateAllSlots()
    {
        for (int i = 0; i < slots.Length; i++)
        {
            SkillCardData card = slots[i].GetCurrentData();
            if (card != null)
            {
                // 현재 재고 확인
                int currentStock = SkillCardShopManager.Instance.GetStockCount(card);

                if (currentStock <= 0)
                {
                    // 재고가 없으면 즉시 새 카드로 교체 (등급 재추첨 로직 포함된 함수 호출)
                    List<SkillCardData> exclude = GetAllDisplayedCards();
                    SkillCardData nextCard = SkillCardShopManager.Instance.GetSingleCardByRarity(exclude);

                    // 매니저 데이터와 슬롯 UI 모두 갱신
                    SkillCardShopManager.Instance.UpdateDisplayedCard(i, nextCard);
                    slots[i].SetCardData(nextCard);
                }
            }
        }
    }

    /// <summary>
    /// 반납된 카드 빈자리에 넣기
    /// </summary>
    /// <param name="data"></param>
    public void RefillEmptySlotWith(SkillCardData data)
    {
        foreach (var slot in slots)
        {
            if (slot.GetCurrentData() == null)
            {
                slot.SetCardData(data);
                break;
            }
        }
    }

    /// <summary>
    /// 모든 슬롯의 버튼 상태를 플레이어 보유 코인에 맞춰 갱신
    /// </summary>
    public void RefreshAllSlotButtons()
    {
        if (PlayerRuntimeData.Instance == null)
            return;

        int playerCoin = PlayerRuntimeData.Instance.coin;

        foreach (var slot in slots)
        {
            if (slot != null)
            {
                slot.DisablePurchaseButton(playerCoin);
            }
        }
    }

    public void ShowBigCardPreview(SkillCardData data)
    {
        if (data == null || bigImage == null) return;

        // 콘솔 창에 선택한 카드의 실제 이름이 정상적으로 찍히는지 확인용
        Debug.Log($"[상점 프리뷰] 선택된 카드 데이터 이름: {data.cardName}");

        // 캐싱이 풀렸을 경우를 대비해 실시간으로 컴포넌트를 한 번 더 검사합니다.
        if (bigCardView == null) bigImage.TryGetComponent(out bigCardView);
        if (bigCardUI == null) bigImage.TryGetComponent(out bigCardUI);

        if (bigCardView != null) bigCardView.Bind(data, null);
        if (bigCardUI != null) bigCardUI.Setup(null, data);

        bigImage.SetActive(true);
    }

    /// <summary>
    /// 큰 카드 외부나 닫기 배경을 클릭했을 때 호출할 함수
    /// </summary>
    public void HideBigCardPreview()
    {
        if (bigImage != null)
        {
            bigImage.SetActive(false);
        }
    }
}
