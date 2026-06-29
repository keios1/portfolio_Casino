using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

/// <summary>
/// 카드 및 상점 UI에 데이터 정보(카드 정보, 가격 등) 띄우는 스크립트
/// </summary>
public class SkillCardSlot : MonoBehaviour
{
    public int slotIndex;

    [Header("UI 연결")]
    [SerializeField] private TextMeshProUGUI cardPriceText; // 카드 가격 텍스트
    [SerializeField] private Button purchaseButton;         // 카드 가격표에 붙은 버튼

    [Header("카드 생성 위치")]
    [SerializeField] private Transform cardSpawnPoint;      // 카드가 배치될 부모 객체

    [Header("카드 프리팹")]
    [SerializeField] private GameObject shopCardPrefab;     // 카드 프리팹

    [Header("카드 솔드아웃 아이콘")]
    [SerializeField] private GameObject soldOutImage;

    private SkillCardData currentData;
    private GameObject spawnedCard;
    private SkillCardShopUI shopUI;

    private void Start()
    {
        shopUI = GetComponentInParent<SkillCardShopUI>();
        soldOutImage.SetActive(false);
    }

    public void OnClickCardSlot()
    {
        if (TutorialManager.Instance != null && TutorialManager.Instance.IsTutorialActive)
        {
            Debug.Log("튜토리얼 진행 중에는 카드를 크게 볼 수 없습니다.");
            return;

        }
        if (currentData == null || shopUI == null) return;

        shopUI.ShowBigCardPreview(currentData);
    }

    /// <summary>
    /// Shop Manager나 UI 스크립트에서 데이터를 넣어주는 함수
    /// </summary>
    public void SetCardData(SkillCardData data)
    {
        currentData = data;

        if (spawnedCard != null) Destroy(spawnedCard);

        if (data == null)
        {
            if (cardPriceText != null)
            {
                // cardPriceText.text = "SOLD OUT";
                soldOutImage.SetActive(true);
            }

            if(purchaseButton != null )
                purchaseButton.interactable = false;

            return;
        }

        if (cardPriceText != null)
            cardPriceText.text = GetDiscountedPrice().ToString() + "G";

        if (purchaseButton != null)
            purchaseButton.interactable = true;

        spawnedCard = Instantiate(shopCardPrefab, cardSpawnPoint);

        if (spawnedCard.TryGetComponent(out CardView view))
            view.Bind(data, null);

        if (spawnedCard.TryGetComponent(out CardUI ui))
            ui.Setup(null, data);


        if (spawnedCard.TryGetComponent(out Button cardButton))
        {
            // 기존에 혹시 들어있을지 모를 이벤트를 깨끗이 비우고
            cardButton.onClick.RemoveAllListeners();
            // 카드가 클릭되면 이 슬롯의 OnClickCardSlot()이 실행되도록 등록합니다.
            cardButton.onClick.AddListener(OnClickCardSlot);
        }
    }

    public SkillCardData GetCurrentData() => currentData;

    /// <summary>
    /// 가격표 클릭시 구매 호출
    /// </summary>
    public void OnClickPurchase()
    {
        if (TutorialManager.Instance != null && TutorialManager.Instance.IsTutorialActive)
        {
            Debug.Log("튜토리얼 진행 중에는 카드를 구매할 수 없습니다.");
            return; 
        }

        if (currentData == null)
            return;

        if (PlayerRuntimeData.Instance == null)
            return;

        int finalPrice = GetDiscountedPrice();

        if (!PlayerRuntimeData.Instance.SpendCoin(finalPrice))
        {
            Debug.Log("플레이어 코인 부족 구매 실패");
            return;
        }
        Debug.Log($"{PlayerRuntimeData.Instance.coin} 남은 플레이어 코인");

        SkillCardShopManager.Instance.PurchaseCard(currentData);

        if (PlayerCardCollection.Instance != null)
        {
            PlayerCardCollection.Instance.AddCard(currentData, 1);
            Debug.Log($"{currentData.cardName}구매 (현재 보유량: {PlayerCardCollection.Instance.GetOwnedCount(currentData)})");
        }

        SkillCardShopUI shopUI = GetComponentInParent<SkillCardShopUI>();

        if (shopUI != null)
        {
            // 현재 화면에 떠 있는 카드들 가져오기
            List<SkillCardData> exclude = shopUI.GetAllDisplayedCards();

            // 떠 있는 카드들을 제외한 카드들 중에서 새 카드 뽑기
            SkillCardData nextCard = SkillCardShopManager.Instance.GetSingleCardByRarity(exclude);

            SkillCardShopManager.Instance.UpdateDisplayedCard(slotIndex, nextCard);

            // 슬롯 갱신
            SetCardData(nextCard);

            // 재고 확인 후 0인 카드 품절처리
            shopUI.ValidateAllSlots();

            shopUI.RefreshAllSlotButtons();
        }
    }

    /// <summary>
    /// 구매 버튼 비활성화
    /// </summary>
    public void DisablePurchaseButton(int playerCoin)
    {
        if(currentData == null) return;

        int finalPrice = GetDiscountedPrice();
        bool canAfford = playerCoin >= finalPrice;

        if (purchaseButton != null)
        {
            purchaseButton.interactable = canAfford;

            Image image = purchaseButton.GetComponent<Image> ();

            if (image != null)
            {
                image.color = canAfford ? Color.white : Color.gray;
            }
        }
    }

    /// <summary>
    /// 할인된 가격 계산
    /// </summary>
    /// <returns></returns>
    public int GetDiscountedPrice()
    {
        if(currentData == null) return 0;

        var discountCoupon = PlayerPassiveItemCollection.Instance.ownedPassives
            .Find(x => x is PassiveItemDiscountCoupon) as PassiveItemDiscountCoupon;

        float rate = (discountCoupon != null) ? discountCoupon.GetDiscount() : 0f;

        return Mathf.Max(0, Mathf.RoundToInt(currentData.price * (1f - rate)));
    }
}
