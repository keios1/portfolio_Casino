using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DealerInteractionUI : MonoBehaviour
{
    public static DealerInteractionUI Instance;

    private enum InteractionType
    {
        Shop,
        Safe,
        Transfusion,
        Gamble
    }

    [Tooltip("장비창 버튼")]
    public GameObject InventoryButton;

    [Header("UI")]
    [Tooltip("뒷 배경 블러 이미지")]
    public GameObject blurImage;
    [Tooltip("상점 UI")]
    public GameObject shopUI;
    [Tooltip("장비창 전체")]
    public GameObject equipmentUI;
    [Tooltip("장비창 카드 UI")]
    public GameObject equipmentCardUI;
    [Tooltip("장비창 패시브 아이템 UI")]
    public GameObject equipmentPassiveUI;
    [Tooltip("장비창 선택 카드 UI")]
    public ScrollRect equipmentCardUIscrollRect;
    [Tooltip("금고 UI")]
    public GameObject safeUI;
    [Tooltip("헌혈기 UI")]
    public GameObject transfusionUI;
    [Tooltip("베팅 UI")]
    public GameObject gambleUI;

    [Header("배경 카메라")]
    public GameObject shopCam;
    public GameObject gamebleCam;
    public GameObject safeCam;
    public GameObject transfusionCam;

    [Header("위아래 화살표")]
    public GameObject upArrow;
    public GameObject downArrow;

    private GameObject currentActiveUI;
    private bool isTransitioning = false;

    private bool wasUpArrowActiveBeforeOpen = false;
    private bool wasDownArrowActiveBeforeOpen = false;

    private void Awake()
    {
        Instance = this;

        InitUI();
    }

    private void InitUI()
    {
        if (blurImage != null) blurImage.SetActive(false);
        if (shopUI != null) shopUI.SetActive(false);
        if (safeUI != null) safeUI.SetActive(false);
        if (transfusionUI != null) transfusionUI.SetActive(false);
        if (gambleUI != null) gambleUI.SetActive(false);

        if (equipmentUI != null) equipmentUI.SetActive(false);
        if (equipmentCardUI != null) equipmentCardUI.SetActive(true);
        if (equipmentPassiveUI != null) equipmentPassiveUI.SetActive(false);
        if (InventoryButton != null) InventoryButton.SetActive(true);

        if (shopCam != null) shopCam.SetActive(false);
        if (gamebleCam != null) gamebleCam.SetActive(false);
        if (safeCam != null) safeCam.SetActive(false);
        if (transfusionCam != null) transfusionCam.SetActive(false);

        isTransitioning = false;
    }    

    private void OnEnable()
    {
        if (GameInputManager.Instance != null)
            GameInputManager.Instance.OnInventoryPressed += ToggleInventoryUI;
    }

    private void OnDisable()
    {
        if (GameInputManager.Instance != null)
            GameInputManager.Instance.OnInventoryPressed -= ToggleInventoryUI;
    }

    private void ActivationInteractionUI(InteractionType type)
    {
        if (isTransitioning) return;

        if (upArrow != null) wasUpArrowActiveBeforeOpen = upArrow.activeSelf;
        if (downArrow != null) wasDownArrowActiveBeforeOpen = downArrow.activeSelf;

        if (upArrow != null) upArrow.SetActive(false);
        if (downArrow != null) downArrow.SetActive(false);

        if (TutorialManager.Instance != null && TutorialManager.Instance.IsTutorialActive)
        {
            string requiredAction = "";
            switch (type)
            {
                case InteractionType.Shop: requiredAction = "ClickShop"; break;
                case InteractionType.Safe: requiredAction = "ClickSafe"; break;
                case InteractionType.Transfusion: requiredAction = "ClickBloodMachine"; break;
                case InteractionType.Gamble: requiredAction = "ClickGamble"; break;
            }

            if (!TutorialManager.Instance.TryAction(requiredAction)) return;
        }

        GameObject targetUI = null;
        GameObject targetCam = null;
        string tutorialPath = "";
        Func<bool> checkTutorialShown = null;
        Action setTutorialShown = null;
        Action extraLogic = null;

        switch (type)
        {
            case InteractionType.Shop:
                targetUI = shopUI;
                targetCam = shopCam;
                checkTutorialShown = () => PlayerRuntimeData.Instance.hasShownShopTutorial;
                setTutorialShown = () => PlayerRuntimeData.Instance.hasShownShopTutorial = true;
                break;
            case InteractionType.Safe:
                targetUI = safeUI;
                targetCam = safeCam;
                checkTutorialShown = () => PlayerRuntimeData.Instance.hasShownSafeTutorial;
                setTutorialShown = () => PlayerRuntimeData.Instance.hasShownSafeTutorial = true;
                break;
            case InteractionType.Transfusion:
                targetUI = transfusionUI;
                targetCam = transfusionCam;
                checkTutorialShown = () => PlayerRuntimeData.Instance.hasShownBloodCenterTutorial;
                setTutorialShown = () => PlayerRuntimeData.Instance.hasShownBloodCenterTutorial = true;
                break;
            case InteractionType.Gamble:
                targetUI = gambleUI;
                targetCam = gamebleCam;
                checkTutorialShown = () => PlayerRuntimeData.Instance.hasShownBettingTutorial;
                setTutorialShown = () => PlayerRuntimeData.Instance.hasShownBettingTutorial = true;
                extraLogic = () => { if (GambleManager.instance != null) GambleManager.instance.PrepareGame(); };
                break;
        }

        if (targetUI == null) return;

        ExecuteWithTransition(() =>
        {
            // 1. 암전 상태가 되었을 때 상호작용 UI 컨텐츠 활성화
            targetUI.SetActive(true);
            if (blurImage != null) blurImage.SetActive(true);
            currentActiveUI = targetUI;

            if (targetCam != null) targetCam.SetActive(true);

            if (NodeSceneCameraController.Instance != null)
                NodeSceneCameraController.Instance.SetInteractionLock(true);

            extraLogic?.Invoke();

            // 2. 대기 코루틴 없이 페이드 아웃 연출과 동시에 즉시 튜토리얼을 실행합니다.
            if (PlayerRuntimeData.Instance != null && !checkTutorialShown())
            {
                setTutorialShown();
                PlayerRuntimeData.Instance.SaveToSaveData();
                ShowInteractionTutorial(tutorialPath);
            }

            // 연출이 진행 중이더라도 UI 세팅이 끝났으므로 트랜지션 락 상태를 해제합니다.
            isTransitioning = false;
        });
    }

    public void ActivateShopUI() => ActivationInteractionUI(InteractionType.Shop);
    public void ActivateSafeUI() => ActivationInteractionUI(InteractionType.Safe);
    public void ActivateTransfusionUI() => ActivationInteractionUI(InteractionType.Transfusion);
    public void ActivateGambleUI() => ActivationInteractionUI(InteractionType.Gamble);

    private void ExecuteWithTransition(Action uiActivationLogic)
    {
        ClearUIPocus();

        if (CartoonTransitionManager.Instance != null)
        {
            isTransitioning = true;

            // 암전이 완전히 끝난 센터 시점에 호출될 콜백 래핑
            Action wrappedLogic = () =>
            {
                uiActivationLogic?.Invoke();
            };

            StartCoroutine(CartoonTransitionManager.Instance.AnimateUiFadeCoroutine(Input.mousePosition, wrappedLogic));
        }
        else
        {
            uiActivationLogic?.Invoke();
            isTransitioning = false;
        }
    }

    private void ClearUIPocus()
    {
        if (EventSystem.current != null && EventSystem.current.currentSelectedGameObject != null)
        {
            EventSystem.current.SetSelectedGameObject(null);
        }
    }

    private void ShowInteractionTutorial(string resourcePath)
    {
        if (TutorialManager.Instance != null)
        {
            TutorialDataSO tutorialData = Resources.Load<TutorialDataSO>(resourcePath);
            if (tutorialData != null)
            {
                TutorialManager.Instance.ShowTutorial(tutorialData);
            }
            else
            {
                Debug.LogWarning($"[튜토리얼] 파일을 찾을 수 없습니다: {resourcePath}");
            }
        }
    }

    public void CloseUI()
    {
        if (TutorialManager.Instance != null && TutorialManager.Instance.IsTutorialActive)
        {
            if (TutorialManager.Instance.GetCurrentProgressType() != TutorialProgressType.ClickTargetUI)
            {
                Debug.Log("튜토리얼 진행 중에는 임의로 창을 닫을 수 없습니다.");
                return; 
            }
        }

        if (currentActiveUI != null)
        {
            currentActiveUI.SetActive(false);
            currentActiveUI = null;
        }

        if (shopCam != null) shopCam.SetActive(false);
        if (gamebleCam != null) gamebleCam.SetActive(false);
        if (safeCam != null) safeCam.SetActive(false);
        if (transfusionCam != null) transfusionCam.SetActive(false);

        blurImage.SetActive(false);
        equipmentUI.SetActive(false);
        InventoryButton.SetActive(true);

        if (NodeSceneCameraController.Instance != null)
        {
            NodeSceneCameraController.Instance.SetInteractionLock(false);
        }

        isTransitioning = false;

        if (upArrow != null) upArrow.SetActive(wasUpArrowActiveBeforeOpen);
        if (downArrow != null) downArrow.SetActive(wasDownArrowActiveBeforeOpen);
    }

    private void ToggleInventoryUI()
    {
        if (equipmentUI == null)
            return;

        if (equipmentUI.activeSelf)
            CloseUI();
        else
            ActivateequipmentUI();
    }

    public void ActivateequipmentUI()
    {
        if (isTransitioning) return;

        if (TutorialManager.Instance != null && TutorialManager.Instance.IsTutorialActive)
        {
            if (!TutorialManager.Instance.TryAction("ClickInven"))
            {
                Debug.Log("지금은 인벤토리를 열 타이밍이 아닙니다!");
                return;
            }

        }

        if (equipmentUI != null)
        {
            ClearUIPocus();

            if (upArrow != null) wasUpArrowActiveBeforeOpen = upArrow.activeSelf;
            if (downArrow != null) wasDownArrowActiveBeforeOpen = downArrow.activeSelf;

            if (upArrow != null) upArrow.SetActive(false);
            if (downArrow != null) downArrow.SetActive(false);

            equipmentUI.SetActive(true);
            if (blurImage != null) blurImage.SetActive(true);
            if (InventoryButton != null) InventoryButton.SetActive(false);

            ActivateEquipmentCardUI();
            currentActiveUI = equipmentUI;

            if (NodeSceneCameraController.Instance != null)
                NodeSceneCameraController.Instance.SetInteractionLock(true);
        }
    }

    public void ActivateEquipmentCardUI()
    {
        if (equipmentCardUI != null) equipmentCardUI.SetActive(true);
        if (equipmentPassiveUI != null) equipmentPassiveUI.SetActive(false);

        if (equipmentCardUIscrollRect != null)
        {
            equipmentCardUIscrollRect.verticalNormalizedPosition = 1f;
        }
    }

    public void ActivateEquipmentPassiveUI()
    {
        if (equipmentPassiveUI != null) equipmentPassiveUI.SetActive(true);
        if (equipmentCardUI != null) equipmentCardUI.SetActive(false);
    }

    public void CloseGambleUI()
    {
        if (GambleManager.instance != null)
        {
            if (GambleManager.instance.BackGround != null) GambleManager.instance.BackGround.SetActive(false);
            if (GambleManager.instance.playerHand != null) GambleManager.instance.playerHand.PrepareHand();
            if (GambleManager.instance.dealerHand != null) GambleManager.instance.dealerHand.PrepareHand();
        }
        CloseUI();
    }

    public bool TryHandleBackShortcut()
    {
        if (isTransitioning || !HasAnyPopupOpen()) return false;

        CloseUI();
        return true;
    }

    public bool HasAnyPopupOpen()
    {
        return (equipmentUI != null && equipmentUI.activeSelf) ||
               (shopUI != null && shopUI.activeSelf) ||
               (safeUI != null && safeUI.activeSelf) ||
               (transfusionUI != null && transfusionUI.activeSelf) ||
               (gambleUI != null && gambleUI.activeSelf);
    }
}
