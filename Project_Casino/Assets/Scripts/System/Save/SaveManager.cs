using System.IO;
using UnityEngine;

/// <summary>
/// 세이브 슬롯 기반으로 게임 데이터를 파일에 저장하고 불러오는 매니저.
/// JSON 직렬화를 사용하여 SaveData를 저장하며,
/// 슬롯 선택, 새 게임 생성, 기존 데이터 로드, 삭제 기능을 제공한다.
/// </summary>
public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance { get; private set; }

    private int currentSlot = -1;
    private SaveData currentSaveData;

    public int CurrentSlot => currentSlot;
    public SaveData CurrentSaveData => currentSaveData;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void SetCurrentSlot(int slot)
    {
        currentSlot = slot;
    }

    public bool HasSave(int slot)
    {
        return File.Exists(GetSlotPath(slot));
    }

    public void CreateNewGame(int slot)
    {
        currentSlot = slot;
        currentSaveData = new SaveData();

        SaveToFile();

        if (PlayerRuntimeData.Instance != null)
            PlayerRuntimeData.Instance.LoadFromSaveData();

        if (PlayerCardCollection.Instance != null)
            PlayerCardCollection.Instance.LoadFromSaveData();

        if (PlayerDeckLoadout.Instance != null)
            PlayerDeckLoadout.Instance.ResetToDefaultDeck();

        SaveToFile();
    }

    public void LoadGame(int slot)
    {
        currentSlot = slot;
        string path = GetSlotPath(slot);

        if (!File.Exists(path))
        {
            CreateNewGame(slot);
            return;
        }

        string json = File.ReadAllText(path);
        currentSaveData = JsonUtility.FromJson<SaveData>(json);

        if (currentSaveData == null)
        {
            currentSaveData = new SaveData();
            SaveToFile();
        }
    }
    public void ResetAfterDefeat(int safeCoin)
    {
        if (currentSaveData == null)
            currentSaveData = new SaveData();

        currentSaveData.player.currentHp = currentSaveData.player.maxHp;
        currentSaveData.player.coin = 100 + safeCoin;
        currentSaveData.player.coinInSafe = 0;

        currentSaveData.mapProgress = new MapProgressSaveData();
        currentSaveData.cardCollection.ownedCards.Clear();
        currentSaveData.deck.deckCardIds.Clear();

        SaveToFile();

        if (PlayerRuntimeData.Instance != null)
            PlayerRuntimeData.Instance.LoadFromSaveData();

        if (PlayerCardCollection.Instance != null)
            PlayerCardCollection.Instance.LoadFromSaveData();

        if (PlayerDeckLoadout.Instance != null)
            PlayerDeckLoadout.Instance.ResetToDefaultDeck();

        if (SkillCardShopManager.Instance != null)
            SkillCardShopManager.Instance.ResetShopForNewRun();
    }
    /// <summary>
    /// 현재 슬롯을 바꾸지 않고 특정 슬롯의 세이브 데이터를 미리 읽어온다.
    /// UI 표시용.
    /// </summary>
    public SaveData PeekSaveData(int slot)
    {
        string path = GetSlotPath(slot);

        if (!File.Exists(path))
            return null;

        string json = File.ReadAllText(path);
        if (string.IsNullOrEmpty(json))
            return null;

        return JsonUtility.FromJson<SaveData>(json);
    }

    public void SaveToFile()
    {
        if (currentSlot < 0)
        {
            Debug.LogWarning("현재 선택된 세이브 슬롯이 없습니다.");
            return;
        }

        if (currentSaveData == null)
            currentSaveData = new SaveData();

        string json = JsonUtility.ToJson(currentSaveData, true);
        File.WriteAllText(GetSlotPath(currentSlot), json);
        Debug.Log($"저장 경로: {GetSlotPath(currentSlot)}");
    }

    public void DeleteSlot(int slot)
    {
        string path = GetSlotPath(slot);

        if (File.Exists(path))
        {
            File.Delete(path);
            Debug.Log($"세이브 슬롯 {slot} 삭제 완료");
        }

        if (currentSlot == slot)
        {
            currentSlot = -1;
            currentSaveData = null;
        }
    }

    public string GetSlotPath(int slot)
    {
        return Path.Combine(Application.persistentDataPath, $"save_slot_{slot}.json");
    }

    /// <summary>
    /// 맵의 진행 상황을 저장한다. 전투 중인지 여부도 함께 저장하여, 씬 로드 후 적절한 상태로 게임을 시작할 수 있도록 한다.
    /// </summary>
    /// <param name="sceneName"></param>
    /// <param name="isInBattle"></param>
    public void SaveSceneState(string sceneName, bool isInBattle)
    {
        if (currentSaveData == null)
            currentSaveData = new SaveData();

        if (currentSaveData.mapProgress == null)
            currentSaveData.mapProgress = new MapProgressSaveData();

        currentSaveData.mapProgress.lastSceneName = sceneName;
        currentSaveData.mapProgress.isInBattle = isInBattle;

        SaveToFile();
    }

    /// <summary>
    /// 전투 시작 시의 상태를 저장합니다. 씬 이름과 전투 시작 여부를 기록하며, 전투 시드도 설정합니다.
    /// </summary>
    /// <param name="sceneName">현재 씬 이름</param>
    public void SaveBattleStartState(string sceneName)
    {
        if (currentSaveData == null)
            currentSaveData = new SaveData();

        if (currentSaveData.mapProgress == null)
            currentSaveData.mapProgress = new MapProgressSaveData();

        currentSaveData.mapProgress.lastSceneName = sceneName;
        currentSaveData.mapProgress.isInBattle = true;

        if (currentSaveData.mapProgress.currentBattleSeed == 0)
            currentSaveData.mapProgress.currentBattleSeed = UnityEngine.Random.Range(1, int.MaxValue);

        SaveToFile();
    }
    public void SaveBattleEndState(string sceneName)
    {
        if (currentSaveData == null)
            currentSaveData = new SaveData();

        if (currentSaveData.mapProgress == null)
            currentSaveData.mapProgress = new MapProgressSaveData();

        currentSaveData.mapProgress.lastSceneName = sceneName;
        currentSaveData.mapProgress.isInBattle = false;
        currentSaveData.mapProgress.currentBattleSeed = 0;

        SaveToFile();
    }
}
