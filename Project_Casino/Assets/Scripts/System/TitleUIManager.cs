using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TitleUIManager : MonoBehaviour
{
    [Header("Panels")]
    public GameObject mainPanel;
    public GameObject savePanel;

    [Header("Save Slots")]
    public Button[] slotButtons;          // 슬롯 버튼들
    public TMP_Text[] slotLabels;         // 슬롯에 표시할 텍스트
    public Button[] deleteButtons;        // 각 슬롯 삭제 버튼
    public TMP_Text[] slotHpLabels;      // 슬롯별 HP 표시
    public TMP_Text[] slotCoinLabels;    // 슬롯별 코인 표시

    [Header("Delete Confirm Popup")]
    public GameObject deleteConfirmPopup;
    public TMP_Text deleteConfirmMessage;

    [Header("Sound")]
    public Slider masterSlider;
    public Slider bgmSlider;
    public Slider sfxSlider;

    [Header("Screen")]
    public Toggle fullscreenToggle;
    public TMP_Dropdown resolutionDropdown;

    private int selectedSlot = -1;
    private int pendingDeleteSlot = -1;
    public SoundLibrary soundLibrary;

    private void Start()
    {
        mainPanel.SetActive(true);
        savePanel.SetActive(false);

        if (deleteConfirmPopup != null)
            deleteConfirmPopup.SetActive(false);

        RefreshSlotUI();
    }

    // ======================
    // Start 버튼 → 세이브 목록 열기
    // ======================

    public void OpenSavePanel()
    {
        mainPanel.SetActive(false);
        savePanel.SetActive(true);

        selectedSlot = -1;
        RefreshSlotUI();
    }

    public void BackToMain()
    {
        savePanel.SetActive(false);
        mainPanel.SetActive(true);
        CloseDeleteConfirmPopup();
    }

    // ======================
    // 슬롯 UI 갱신
    // ======================

    public void RefreshSlotUI()
    {

        if (SaveManager.Instance == null)
        {
            Debug.LogWarning("SaveManager.Instance가 없습니다.");
            return;
        }

        if (slotButtons == null || slotButtons.Length == 0)
        {
            Debug.LogWarning("slotButtons가 연결되지 않았습니다.");
            return;
        }

        for (int i = 0; i < slotButtons.Length; i++)
        {
            if (slotButtons[i] == null)
                continue;

            int slotIndex = i;

            bool hasSave = SaveManager.Instance.HasSave(slotIndex);
            SaveData data = SaveManager.Instance.PeekSaveData(slotIndex);

            if (slotLabels != null && i < slotLabels.Length && slotLabels[i] != null)
            {
                if (hasSave && data != null && data.player != null)
                {
                    slotLabels[i].text =
                        $"저장 슬롯 {slotIndex + 1}";
                }
                else
                {
                    slotLabels[i].text = $"저장 슬롯 {slotIndex + 1}\n비어 있음";
                }
            }

            if (slotHpLabels != null && i < slotHpLabels.Length && slotHpLabels[i] != null)
            {
                if (hasSave && data != null && data.player != null)
                {
                    slotHpLabels[i].text = $"HP {data.player.currentHp} / {data.player.maxHp}";
                }
                else
                {
                    slotHpLabels[i].text = "";
                }
            }

            if (slotCoinLabels != null && i < slotCoinLabels.Length && slotCoinLabels[i] != null)
            {
                if (hasSave && data != null && data.player != null)
                {
                    slotCoinLabels[i].text = $"Coin {data.player.coin}";
                }
                else
                {
                    slotCoinLabels[i].text = "";
                }
            }

            if (deleteButtons != null && i < deleteButtons.Length && deleteButtons[i] != null)
            {
                deleteButtons[i].gameObject.SetActive(hasSave);
            }

            // 선택된 슬롯은 색 바꾸기
            ColorBlock colors = slotButtons[i].colors;
            colors.normalColor = (selectedSlot == slotIndex)
                ? new Color(0.85f, 0.85f, 0.45f, 1f)
                : Color.white;
            slotButtons[i].colors = colors;
        }
    }

    // ======================
    // 슬롯 클릭
    // ======================

    public void SelectSlot(int slotIndex)
    {
        if (SaveManager.Instance == null)
        {
            Debug.LogError("SaveManager가 씬에 없습니다.");
            return;
        }

        bool hasSave = SaveManager.Instance.HasSave(slotIndex);

        // 저장 데이터가 있으면 클릭 즉시 로드
        if (hasSave)
        {
            LoadSlot(slotIndex);
        }
        else
        {
            CreateNewGameAndEnter(slotIndex);
        }
    }

    private void CreateNewGameAndEnter(int slotIndex)
    {
        SaveManager.Instance.SetCurrentSlot(slotIndex);
        SaveManager.Instance.CreateNewGame(slotIndex);

        if (PlayerRuntimeData.Instance != null)
            PlayerRuntimeData.Instance.LoadFromSaveData();

        if (PlayerCardCollection.Instance != null)
            PlayerCardCollection.Instance.LoadFromSaveData();

        if (PlayerDeckLoadout.Instance != null)
            PlayerDeckLoadout.Instance.LoadFromSaveData();

        SceneManager.LoadScene("StoryScene"); // 씬이동 변경
    }

    private void LoadSlot(int slotIndex)
    {
        SaveManager.Instance.SetCurrentSlot(slotIndex);
        SaveManager.Instance.LoadGame(slotIndex);

        if (PlayerRuntimeData.Instance != null)
            PlayerRuntimeData.Instance.LoadFromSaveData();

        if (PlayerCardCollection.Instance != null)
            PlayerCardCollection.Instance.LoadFromSaveData();

        if (PlayerDeckLoadout.Instance != null)
            PlayerDeckLoadout.Instance.LoadFromSaveData();

        string sceneName = "NodeScene";

        if (SaveManager.Instance.CurrentSaveData != null &&
            SaveManager.Instance.CurrentSaveData.mapProgress != null)
        {
            MapProgressSaveData mapProgress = SaveManager.Instance.CurrentSaveData.mapProgress;

            if (!string.IsNullOrEmpty(mapProgress.lastSceneName))
            {
                sceneName = mapProgress.lastSceneName;
            }
        }

        SceneManager.LoadScene(sceneName);
    }

    // =========================
    // 삭제 확인 팝업
    // =========================

    public void RequestDeleteSlot(int slotIndex)
    {
        if (SaveManager.Instance == null)
        {
            Debug.LogError("SaveManager가 씬에 없습니다.");
            return;
        }

        if (!SaveManager.Instance.HasSave(slotIndex))
        {
            Debug.Log("삭제할 저장 데이터가 없습니다.");
            return;
        }

        pendingDeleteSlot = slotIndex;

        if (deleteConfirmMessage != null)
        {
            deleteConfirmMessage.text =
                $"삭제하시겠습니까?";
        }

        if (deleteConfirmPopup != null)
            deleteConfirmPopup.SetActive(true);
    }

    public void ConfirmDeleteSlot()
    {
        if (pendingDeleteSlot < 0)
        {
            CloseDeleteConfirmPopup();
            return;
        }

        if (SaveManager.Instance == null)
        {
            Debug.LogError("SaveManager가 씬에 없습니다.");
            CloseDeleteConfirmPopup();
            return;
        }

        SaveManager.Instance.DeleteSlot(pendingDeleteSlot);

        if (selectedSlot == pendingDeleteSlot)
            selectedSlot = -1;

        pendingDeleteSlot = -1;
        CloseDeleteConfirmPopup();
        RefreshSlotUI();
    }

    public void CloseDeleteConfirmPopup()
    {
        pendingDeleteSlot = -1;

        if (deleteConfirmPopup != null)
            deleteConfirmPopup.SetActive(false);
    }

    // ======================
    // 게임종료 버튼
    // ======================

    public void ExitGame()
    {
        Application.Quit();
    }

}
