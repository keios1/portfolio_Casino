using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System;

public class TutorialManager : MonoBehaviour
{
    public static TutorialManager Instance;
    private TutorialDataSO pendingTutorial;

    [Header("UI Panels")]
    public GameObject tutorialCanvas;
    public GameObject blockerPanel;

    [Header("전투 씬 튜토리얼 UI (기본)")]
    public GameObject battleTutorialPanel;
    public Image battleImage;
    public TMP_Text battleTitleText;
    public TMP_Text battleContentText;

    [Header("노드 씬 튜토리얼 UI")]
    public GameObject nodeTutorialPanel;
    public Image nodeImage;
    public TMP_Text nodeTitleText;
    public TMP_Text nodeContentText;

    private Image currentImage;
    private TMP_Text currentTitleText;
    private TMP_Text currentContentText;

    [Header("경고 UI (틀린 행동 시)")]
    public TMP_Text warningText;
    private Coroutine warningCoroutine;

    [Header("Input Settings")]
    public float spaceCooldown = 2f; 
    private float lastSpaceTime = 0f; 

    private List<TutorialPage> currentPages;
    private int currentPageIndex = 0;
    public bool IsTutorialActive { get; private set; }
    public Action<string> OnTutorialCustomEvent;
    private List<Button> currentTargetButtons = new List<Button>();

    private void Awake()
    {
        if (Instance == null) { Instance = this; DontDestroyOnLoad(gameObject); }
        else { Destroy(gameObject); }
    }

    private void Start()
    {
        tutorialCanvas.SetActive(false);
        if (warningText != null) warningText.gameObject.SetActive(false);
        if (TutorialArrowUI.Instance != null) TutorialArrowUI.Instance.HideArrow();
    }

    private void OnEnable() { SceneManager.sceneLoaded += OnSceneLoaded; }
    private void OnDisable() { SceneManager.sceneLoaded -= OnSceneLoaded; }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (pendingTutorial != null) { ShowTutorial(pendingTutorial); pendingTutorial = null; }
    }

    public void ReserveTutorial(TutorialDataSO tutorialData) { pendingTutorial = tutorialData; }

    public void ShowTutorial(TutorialDataSO tutorialData)
    {
        if (tutorialData == null || tutorialData.pages.Count == 0) return;

        IsTutorialActive = true;
        currentPages = tutorialData.pages;
        currentPageIndex = 0;

        lastSpaceTime = Time.time;

        tutorialCanvas.SetActive(true);
        blockerPanel.SetActive(true);

        SetupActivePanel();
        UpdatePageUI();
    }

    private void SetupActivePanel()
    {
        if (SceneManager.GetActiveScene().name == "NodeScene")
        {
            if (battleTutorialPanel != null) battleTutorialPanel.SetActive(false);
            if (nodeTutorialPanel != null) nodeTutorialPanel.SetActive(true);

            currentImage = nodeImage;
            currentTitleText = nodeTitleText;
            currentContentText = nodeContentText;
        }
        else
        {
            // 노드 씬이 아니면 무조건 전투 씬(기본) 패널 사용
            if (nodeTutorialPanel != null) nodeTutorialPanel.SetActive(false);
            if (battleTutorialPanel != null) battleTutorialPanel.SetActive(true);

            currentImage = battleImage;
            currentTitleText = battleTitleText;
            currentContentText = battleContentText;
        }
    }

    private void UpdatePageUI()
    {
        if (UnityEngine.EventSystems.EventSystem.current != null)
        {
            UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(null);
        }
        lastSpaceTime = Time.time;

        ResetCurrentTarget();
        TutorialPage page = currentPages[currentPageIndex];

        if (currentImage != null)
        {
            if (page.image != null)
            {
                currentImage.sprite = page.image;
            }
            currentImage.gameObject.SetActive(true);
        }

        if (currentTitleText != null) currentTitleText.text = page.title;
        if (currentContentText != null) currentContentText.text = page.content;

        if (!string.IsNullOrEmpty(page.customEventName))
            OnTutorialCustomEvent?.Invoke(page.customEventName);

        if (!string.IsNullOrEmpty(page.targetButtonName))
            SetupInteractiveTarget(page.targetButtonName, page.showArrow, page.progressType);

        Image blockerImg = blockerPanel.GetComponent<Image>();
        if (blockerImg != null)
        {
            if (page.progressType == TutorialProgressType.RequireAction)
                blockerImg.raycastTarget = false;
            else
                blockerImg.raycastTarget = true;
        }
    }

    private void Update()
    {
        if (IsTutorialActive && tutorialCanvas != null)
        {
            Canvas myCanvas = tutorialCanvas.GetComponent<Canvas>();
            if (myCanvas != null && myCanvas.renderMode == RenderMode.ScreenSpaceCamera && myCanvas.worldCamera == null)
            {
                Camera cam = Camera.main;
                if (cam == null) cam = FindObjectOfType<Camera>();
                if (cam != null) { myCanvas.worldCamera = cam; myCanvas.planeDistance = 50f; }
            }
        }

        if (!IsTutorialActive || currentPages == null) return;
        TutorialPage page = currentPages[currentPageIndex];

        if (page.progressType == TutorialProgressType.Spacebar && Input.GetKeyDown(KeyCode.Space))
        {
            if (Time.time - lastSpaceTime >= spaceCooldown)
            {
                lastSpaceTime = Time.time;
                ForceNextPage();
            }
            else
            {
                Debug.Log("스페이스바 쿨타임 중"); // 입력 간격 주기 1s
            }
        }
    }

    public bool TryAction(string attemptedAction)
    {
        if (!IsTutorialActive || currentPages == null) return true;
        TutorialPage page = currentPages[currentPageIndex];

        if (page.progressType == TutorialProgressType.RequireAction)
        {
            if (attemptedAction == page.requiredActionName) { ForceNextPage(); return true; }
            else { ShowWarningMessage(page.wrongActionMessage); return false; }
        }
        return false;
    }

    private void ShowWarningMessage(string message) // aa
    {
        if (warningText == null || string.IsNullOrEmpty(message)) return;
        if (warningCoroutine != null) StopCoroutine(warningCoroutine);
        warningCoroutine = StartCoroutine(WarningRoutine(message));
    }

    private IEnumerator WarningRoutine(string message)
    {
        warningText.text = message;
        warningText.gameObject.SetActive(true);
        yield return new WaitForSeconds(2f);
        warningText.gameObject.SetActive(false);
    }

    public void ForceNextPage()
    {
        Debug.Log($"🚩 페이지 넘김 요청! | 현재 인덱스: {currentPageIndex} | 프레임: {Time.frameCount}");
        if (currentPageIndex < currentPages.Count - 1) { currentPageIndex++; UpdatePageUI(); }
        else { CloseTutorial(); }
    }

    private void CloseTutorial()
    {
        ResetCurrentTarget();
        tutorialCanvas.SetActive(false);
        blockerPanel.SetActive(false);
        if (warningText != null) warningText.gameObject.SetActive(false);
        if (warningCoroutine != null) StopCoroutine(warningCoroutine);
        IsTutorialActive = false;
        currentPages = null;
    }

    private void SetupInteractiveTarget(string targetNames, bool showArrow, TutorialProgressType progressType)
    {
        string[] names = targetNames.Split(',');
        for (int i = 0; i < names.Length; i++)
        {
            string targetName = names[i].Trim();
            GameObject targetObj = GameObject.Find(targetName);
            if (targetObj == null) continue;

            if (progressType == TutorialProgressType.ClickTargetUI)
            {
                Button btn = targetObj.GetComponent<Button>();
                if (btn != null)
                {
                    btn.onClick.AddListener(OnInteractiveTargetClicked);
                    currentTargetButtons.Add(btn);
                }
            }
            if (i == 0 && showArrow && TutorialArrowUI.Instance != null)
                TutorialArrowUI.Instance.ShowArrow(targetName);
        }
    }

    private void OnInteractiveTargetClicked()
    {
        TutorialManager.Instance.TryAction("SelectEnemy");
        ForceNextPage();
    }

    private void ResetCurrentTarget()
    {
        foreach (var btn in currentTargetButtons)
            if (btn != null) btn.onClick.RemoveListener(OnInteractiveTargetClicked);
        currentTargetButtons.Clear();
        if (TutorialArrowUI.Instance != null) TutorialArrowUI.Instance.HideArrow();
    }

    public TutorialProgressType GetCurrentProgressType()
    {
        if (!IsTutorialActive || currentPages == null) return TutorialProgressType.Spacebar;
        return currentPages[currentPageIndex].progressType;
    }
}
