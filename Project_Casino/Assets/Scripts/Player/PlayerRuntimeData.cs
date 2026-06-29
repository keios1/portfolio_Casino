using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 플레이어의 런타임 상태 데이터를 저장하고 유지하는 싱글톤 클래스.
/// HP, 코인 등의 상태를 씬 간 유지하며,
/// Player와 데이터를 상호 동기화하는 역할을 수행한다.
/// </summary>
public class PlayerRuntimeData : MonoBehaviour
{
    public static PlayerRuntimeData Instance { get; private set; }

    [Header("Player Status")]
    public int currentHp;
    public int maxHp;
    public int coin;
    public int coinInSafe;

    //튜토리얼 여부 상태 저장 
    [Header("Tutorial States")]
    public bool hasCompletedBattleTutorial = false;
    public bool hasCompletedNodeTutorial = false;

    public bool justFinishedTutorialBattle = false;

    //아래는 이전 데이터
    public bool hasShownShopTutorial = false;
    public bool hasShownSafeTutorial = false;
    public bool hasShownBloodCenterTutorial = false;
    public bool hasShownBettingTutorial = false;

    public event Action<int, int> OnHpChanged;
    public event Action<int> OnCoinChanged;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        LoadFromSaveData();
    }

    public void InitTestData(int testMaxHp, int testCurrentHp, int testCoin, int testCoinInSafe)
    {
        maxHp = Mathf.Max(1, testMaxHp);
        currentHp = Mathf.Clamp(testCurrentHp, 0, maxHp);
        coin = Mathf.Max(0, testCoin);
        coinInSafe = Mathf.Max(0, testCoinInSafe);

        OnHpChanged?.Invoke(currentHp, maxHp);
        OnCoinChanged?.Invoke(coin);
    }

    public void LoadFromSaveData()
    {
        if (SaveManager.Instance == null || SaveManager.Instance.CurrentSaveData == null)
            return;

        PlayerSaveData data = SaveManager.Instance.CurrentSaveData.player;
        TutorialSaveData tutData = SaveManager.Instance.CurrentSaveData.tutorial; 

        maxHp = Mathf.Max(1, data.maxHp);
        currentHp = Mathf.Clamp(data.currentHp, 0, maxHp);
        coin = Mathf.Max(0, data.coin);
        coinInSafe = Mathf.Max(0, data.coinInSafe);

        // 저장되어 있던 튜토리얼 시청 기록을 런타임 변수에 덮어씌움
        hasCompletedBattleTutorial = tutData.hasCompletedBattleTutorial;
        hasCompletedNodeTutorial = tutData.hasCompletedNodeTutorial;

        hasShownShopTutorial = tutData.hasShownShopTutorial;// 이전 튜토리얼
        hasShownSafeTutorial = tutData.hasShownSafeTutorial;
        hasShownBloodCenterTutorial = tutData.hasShownBloodCenterTutorial;
        hasShownBettingTutorial = tutData.hasShownBettingTutorial;

        OnHpChanged?.Invoke(currentHp, maxHp);
        OnCoinChanged?.Invoke(coin);
    }

    public void SaveToSaveData()
    {
        if (SaveManager.Instance == null || SaveManager.Instance.CurrentSaveData == null)
            return;

        PlayerSaveData data = SaveManager.Instance.CurrentSaveData.player;
        TutorialSaveData tutData = SaveManager.Instance.CurrentSaveData.tutorial; 

        data.maxHp = maxHp;
        data.currentHp = currentHp;
        data.coin = coin;
        data.coinInSafe = coinInSafe;

        // 현재 런타임의 튜토리얼 시청 기록을 세이브 데이터에 반영
        tutData.hasCompletedBattleTutorial = hasCompletedBattleTutorial;
        tutData.hasCompletedNodeTutorial = hasCompletedNodeTutorial;

        tutData.hasShownShopTutorial = hasShownShopTutorial; // 이전 튜토리얼
        tutData.hasShownSafeTutorial = hasShownSafeTutorial;
        tutData.hasShownBloodCenterTutorial = hasShownBloodCenterTutorial;
        tutData.hasShownBettingTutorial = hasShownBettingTutorial;

        SaveManager.Instance.SaveToFile();
    }

    //플레이어의 현재 상태를 런타임 데이터에 저장한다.
    public void SaveFromPlayer(Player player, bool saveToFile = true)
    {
        if (player == null) return;

        maxHp = player.MaxHp;
        currentHp = player.CurrentHp;
        coin = player.Coin;
        coinInSafe = player.CoinInSafe;

        OnHpChanged?.Invoke(currentHp, maxHp);
        OnCoinChanged?.Invoke(coin);

        if (saveToFile)
            SaveToSaveData();
    }

    //런타임 데이터의 상태를 플레이어에 적용한다.
    public void LoadToPlayer(Player player)
    {
        if (player == null) return;
        player.ApplyRuntimeData(maxHp, currentHp, coin, coinInSafe);
    }

    //코인을 소비하는 메서드. 충분한 코인이 있는지 확인 후 소비한다.
    public bool SpendCoin(int amount)
    {
        if (amount <= 0) return true;
        if (coin < amount) return false;

        coin -= amount;

        OnCoinChanged?.Invoke(coin);
        SaveToSaveData();

        return true;
    }

    //코인을 추가하는 메서드. 음수 입력은 무시한다.
    public void AddCoin(int amount)
    {
        if (amount <= 0) return;

        coin += amount;

        OnCoinChanged?.Invoke(coin);
        SaveToSaveData();
    }

    public void UpdateMaxHpFromPassive(int newMaxHp)
    {
        maxHp += newMaxHp;

        if (currentHp > maxHp)
            currentHp = maxHp;

        OnHpChanged?.Invoke(currentHp, maxHp);
        SaveToSaveData();
    }

    // HP를 소비하는 메서드. HP가 0 이하가 되면 실패한다.
    public bool SpendHp(int amount)
    {
        if (amount <= 0) return true;
        if (currentHp - amount <= 0) return false;

        currentHp -= amount;

        OnHpChanged?.Invoke(currentHp, maxHp);
        SaveToSaveData();

        return true;
    }

    // HP를 회복하는 메서드. maxHp를 넘지 않는다.
    public void AddHp(int amount)
    {
        if (amount <= 0) return;

        currentHp = Mathf.Min(maxHp, currentHp + amount);

        OnHpChanged?.Invoke(currentHp, maxHp);
        SaveToSaveData();
    }

    // 금고에 코인을 입금하는 메서드
    public bool DepositCoinToSafe(int amount)
    {
        if (amount <= 0) return false;
        if (coin < amount) return false;

        coin -= amount;
        coinInSafe += amount;

        OnCoinChanged?.Invoke(coin);
        SaveToSaveData();

        return true;
    }
}
