using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// ESC 일시정지 메뉴와 설정창 전체를 관리하는 컨트롤러.
/// 일시정지 메뉴: 계속하기, 설정, 사전, 전투 포기, 저장 후 종료
/// 설정창: 일반, 그래픽, 소리, 입력 탭
/// </summary>
public class SettingPanelController : MonoBehaviour
{
    [Header("Pause Root")]
    [SerializeField] private GameObject pauseRoot;
    [SerializeField] private GameObject pauseMenuPanel;

    [Header("Pause Sub Panels")]
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private GameObject dictionaryPanel;
    [SerializeField] private GameObject giveUpConfirmPanel;

    [Header("Settings Tabs")]
    [SerializeField] private GameObject generalPanel;
    [SerializeField] private GameObject graphicPanel;
    [SerializeField] private GameObject soundPanel;
    [SerializeField] private GameObject inputPanel;

    [Header("Sound")]
    [SerializeField] private Slider masterSlider;
    [SerializeField] private Slider bgmSlider;
    [SerializeField] private Slider sfxSlider;

    [Header("Sound Text")]
    [SerializeField] private TMP_Text masterText;
    [SerializeField] private TMP_Text bgmText;
    [SerializeField] private TMP_Text sfxText;

    [Header("Graphic")]
    [SerializeField] private Toggle fullscreenToggle;
    [SerializeField] private TMP_Dropdown resolutionDropdown;

    [Header("Scene")]
    [SerializeField] private string titleSceneName = "TitleScene";

    [Header("Pause Option")]
    [SerializeField] private bool useTimeScalePause = true;
    [SerializeField] private bool isTitleSceneMode = false;
    private bool isInitializing;
    private bool isPaused;
    private enum PauseView
    {
        Closed,
        PauseMenu,
        Settings,
        Dictionary,
        GiveUpConfirm
    }

    private PauseView currentView = PauseView.Closed;
    public void Back()
    {
        if (isTitleSceneMode)
        {
            ClosePause();
            return;
        }

        switch (currentView)
        {
            case PauseView.Settings:
            case PauseView.Dictionary:
            case PauseView.GiveUpConfirm:
                OpenPauseMenu();
                break;

            case PauseView.PauseMenu:
                ClosePause();
                break;

            case PauseView.Closed:
                OpenPauseMenu();
                break;
        }
    }
    private void Awake()
    {
        InitUIEvents();
    }

    private void Start()
    {
        LoadCurrentSettings();
        ClosePause();
    }

    private void InitUIEvents()
    {
        if (masterSlider != null)
            masterSlider.onValueChanged.AddListener(SetMasterVolume);

        if (bgmSlider != null)
            bgmSlider.onValueChanged.AddListener(SetBGMVolume);

        if (sfxSlider != null)
            sfxSlider.onValueChanged.AddListener(SetSFXVolume);

        if (fullscreenToggle != null)
            fullscreenToggle.onValueChanged.AddListener(SetFullscreen);

        if (resolutionDropdown != null)
            resolutionDropdown.onValueChanged.AddListener(SetResolution);
    }
    // =========================
    // Pause Menu
    // =========================

    private void ShowPauseMenuOnly()
    {
        if (pauseMenuPanel != null)
            pauseMenuPanel.SetActive(true);

        if (settingsPanel != null)
            settingsPanel.SetActive(false);

        if (dictionaryPanel != null)
            dictionaryPanel.SetActive(false);

        if (giveUpConfirmPanel != null)
            giveUpConfirmPanel.SetActive(false);
    }

    public void OpenPauseMenu()
    {

        if (TutorialManager.Instance != null && TutorialManager.Instance.IsTutorialActive)
        {
            Debug.Log("튜토리얼 중이라 설정창을 열 수 없습니다.");
            return;
        }

        currentView = PauseView.PauseMenu;
        isPaused = true;

        if (pauseRoot != null)
            pauseRoot.SetActive(true);

        if (useTimeScalePause)
            Time.timeScale = 0f;

        ShowPauseMenuOnly();
    }

    public void ClosePause()
    {
        currentView = PauseView.Closed;
        isPaused = false;

        if (pauseMenuPanel != null)
            pauseMenuPanel.SetActive(false);

        if (settingsPanel != null)
            settingsPanel.SetActive(false);

        if (dictionaryPanel != null)
            dictionaryPanel.SetActive(false);

        if (giveUpConfirmPanel != null)
            giveUpConfirmPanel.SetActive(false);

        if (pauseRoot != null)
            pauseRoot.SetActive(false);

        if (useTimeScalePause)
            Time.timeScale = 1f;
    }

    public void OpenSettings()
    {
        currentView = PauseView.Settings;
        isPaused = true;

        if (pauseRoot != null)
            pauseRoot.SetActive(true);

        if (pauseMenuPanel != null)
            pauseMenuPanel.SetActive(false);

        if (settingsPanel != null)
            settingsPanel.SetActive(true);

        if (dictionaryPanel != null)
            dictionaryPanel.SetActive(false);

        if (giveUpConfirmPanel != null)
            giveUpConfirmPanel.SetActive(false);

        LoadCurrentSettings();
        ShowGeneralTab();
    }

    public void OpenDictionary()
    {
        currentView = PauseView.Dictionary;

        if (pauseMenuPanel != null)
            pauseMenuPanel.SetActive(false);

        if (settingsPanel != null)
            settingsPanel.SetActive(false);

        if (dictionaryPanel != null)
            dictionaryPanel.SetActive(true);

        if (giveUpConfirmPanel != null)
            giveUpConfirmPanel.SetActive(false);
    }

    public void OpenGiveUpConfirm()
    {
        currentView = PauseView.GiveUpConfirm;

        if (pauseMenuPanel != null)
            pauseMenuPanel.SetActive(false);

        if (settingsPanel != null)
            settingsPanel.SetActive(false);

        if (dictionaryPanel != null)
            dictionaryPanel.SetActive(false);

        if (giveUpConfirmPanel != null)
            giveUpConfirmPanel.SetActive(true);
    }

    public void BackToPauseMenu()
    {
        ShowPauseMenuOnly();
    }

    // =========================
    // Settings Tabs
    // =========================

    private void ShowOnlySettingTab(GameObject targetPanel)
    {
        if (generalPanel != null)
            generalPanel.SetActive(generalPanel == targetPanel);

        if (graphicPanel != null)
            graphicPanel.SetActive(graphicPanel == targetPanel);

        if (soundPanel != null)
            soundPanel.SetActive(soundPanel == targetPanel);

        if (inputPanel != null)
            inputPanel.SetActive(inputPanel == targetPanel);
    }

    public void ShowGeneralTab()
    {
        ShowOnlySettingTab(generalPanel);
    }

    public void ShowGraphicTab()
    {
        ShowOnlySettingTab(graphicPanel);
    }

    public void ShowSoundTab()
    {
        ShowOnlySettingTab(soundPanel);
    }

    public void ShowInputTab()
    {
        ShowOnlySettingTab(inputPanel);
    }

    // =========================
    // Save / Give Up
    // =========================

    public void SaveAndGoTitle()
    {
        if (SaveManager.Instance != null)
            SaveManager.Instance.SaveToFile();

        Time.timeScale = 1f;
        SceneManager.LoadScene(titleSceneName);
    }

    public void ConfirmGiveUp()
    {
        Time.timeScale = 1f;

        int safeCoin = 0;

        if (SaveManager.Instance != null && SaveManager.Instance.CurrentSaveData != null)
            safeCoin = SaveManager.Instance.CurrentSaveData.player.coinInSafe;

        if (SaveManager.Instance != null)
            SaveManager.Instance.ResetAfterDefeat(safeCoin);

        SceneManager.LoadScene(titleSceneName);
    }

    public void CancelGiveUp()
    {
        BackToPauseMenu();
    }

    // =========================
    // Load Settings
    // =========================

    private void LoadCurrentSettings()
    {
        isInitializing = true;

        float master = PlayerPrefs.GetFloat("MASTER_VOLUME", 0.5f);
        float bgm = PlayerPrefs.GetFloat("BGM_VOLUME", 0.5f);
        float sfx = PlayerPrefs.GetFloat("SFX_VOLUME", 0.5f);

        if (masterSlider != null)
            masterSlider.value = master;

        if (bgmSlider != null)
            bgmSlider.value = bgm;

        if (sfxSlider != null)
            sfxSlider.value = sfx;

        UpdateVolumeText(masterText, master);
        UpdateVolumeText(bgmText, bgm);
        UpdateVolumeText(sfxText, sfx);

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SetMasterVolume(master);
            AudioManager.Instance.SetBGMVolume(bgm);
            AudioManager.Instance.SetSFXVolume(sfx);
        }

        bool fullscreen = PlayerPrefs.GetInt("FULLSCREEN", 1) == 1;

        if (fullscreenToggle != null)
            fullscreenToggle.isOn = fullscreen;

        if (resolutionDropdown != null && resolutionDropdown.options.Count > 0)
        {
            int savedIndex = PlayerPrefs.GetInt("RESOLUTION_INDEX", 2);
            savedIndex = Mathf.Clamp(savedIndex, 0, resolutionDropdown.options.Count - 1);
            resolutionDropdown.value = savedIndex;
        }

        isInitializing = false;
    }

    // =========================
    // Sound
    // =========================

    private void SetMasterVolume(float value)
    {
        if (isInitializing) return;

        PlayerPrefs.SetFloat("MASTER_VOLUME", value);
        PlayerPrefs.Save();

        UpdateVolumeText(masterText, value);

        if (AudioManager.Instance != null)
            AudioManager.Instance.SetMasterVolume(value);
    }

    private void SetBGMVolume(float value)
    {
        if (isInitializing) return;

        PlayerPrefs.SetFloat("BGM_VOLUME", value);
        PlayerPrefs.Save();

        UpdateVolumeText(bgmText, value);

        if (AudioManager.Instance != null)
            AudioManager.Instance.SetBGMVolume(value);
    }

    private void SetSFXVolume(float value)
    {
        if (isInitializing) return;

        PlayerPrefs.SetFloat("SFX_VOLUME", value);
        PlayerPrefs.Save();

        UpdateVolumeText(sfxText, value);

        if (AudioManager.Instance != null)
            AudioManager.Instance.SetSFXVolume(value);
    }

    private void UpdateVolumeText(TMP_Text text, float value)
    {
        if (text == null) return;

        text.text = Mathf.RoundToInt(value * 100f) + "%";
    }

    // =========================
    // Graphic
    // =========================

    private void SetFullscreen(bool isFullscreen)
    {
        if (isInitializing) return;

        PlayerPrefs.SetInt("FULLSCREEN", isFullscreen ? 1 : 0);
        PlayerPrefs.Save();

        if (isFullscreen)
        {
            Screen.SetResolution(1920, 1080, FullScreenMode.ExclusiveFullScreen);
        }
        else
        {
            Screen.SetResolution(1280, 720, FullScreenMode.Windowed);
        }
    }

    private void SetResolution(int index)
    {
        if (isInitializing) return;

        PlayerPrefs.SetInt("RESOLUTION_INDEX", index);
        PlayerPrefs.Save();

        FullScreenMode mode = Screen.fullScreenMode;

        switch (index)
        {
            case 0:
                Screen.SetResolution(1280, 720, mode);
                break;

            case 1:
                Screen.SetResolution(1600, 900, mode);
                break;

            case 2:
                Screen.SetResolution(1920, 1080, mode);
                break;

            default:
                Debug.LogWarning($"지원하지 않는 해상도 인덱스입니다: {index}");
                break;
        }
    }

    // =========================
    // Input
    // =========================
    private void OnEnable()
    {
        if (GameInputManager.Instance != null)
            GameInputManager.Instance.OnPausePressed += HandlePauseShortcut;
    }

    private void OnDisable()
    {
        if (GameInputManager.Instance != null)
            GameInputManager.Instance.OnPausePressed -= HandlePauseShortcut;
    }

    private void HandlePauseShortcut()
    {
        if (TutorialManager.Instance != null && TutorialManager.Instance.IsTutorialActive)
        {
            Debug.Log("튜토리얼 중이라 설정창을 열 수 없습니다.");
            return;
        }

        if (currentView != PauseView.Closed)
        {
            Back();
            return;
        }

        if (DealerInteractionUI.Instance != null &&
            DealerInteractionUI.Instance.TryHandleBackShortcut())
        {
            return;
        }

        OpenPauseMenu();
    }
}
