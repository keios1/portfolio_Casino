using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
/// <summary>
/// 전투 종료 후 결과 화면을 관리하는 UI 클래스.
/// 승리/패배 패널 표시, 보상 표시, 버튼 클릭 처리,
/// 그리고 승리 시 보상 적용 후 다음 씬으로 이동하는 역할을 담당한다.
/// </summary>
public class BattleResultUI : MonoBehaviour
{
    [SerializeField] private Player player;
    [Header("Root")]
    [SerializeField] private GameObject resultUIRoot;

    [Header("Panels")]
    [SerializeField] private GameObject victoryPanel;
    [SerializeField] private GameObject defeatPanel;

    [Header("Victory")]
    //[SerializeField] private TMP_Text victoryTitleText;
    //[SerializeField] private TMP_Text victorySummaryText;
    [SerializeField] private Button victoryNextButton;

    [Header("Defeat")]
    //[SerializeField] private TMP_Text defeatTitleText;
    //[SerializeField] private TMP_Text defeatSummaryText;
    [SerializeField] private Button exitButton;

    [Header("Reward")]
    [SerializeField] private Transform rewardListRoot;
    [SerializeField] private TMP_Text rewardTitleText;
    [SerializeField] private Image coinImage;
    [SerializeField] private TMP_Text coinText;
    [Header("Optional")]
    [SerializeField] private GameObject battleHUD;

    private List<RewardData> currentRewards = new List<RewardData>();
    private bool rewardConfirmed = false;

    private void Awake()
    {
        HideAll();

        victoryNextButton.onClick.AddListener(OnClickVictoryConfirm);
        exitButton.onClick.AddListener(OnClickExit);
    }

    public void ShowVictory(List<RewardData> rewards)
    {
        currentRewards = rewards;

        if(battleHUD != null)
            battleHUD.SetActive(false);

        resultUIRoot.SetActive(true);
        victoryPanel.SetActive(true);
        defeatPanel.SetActive(false);

        //victoryTitleText.text = "Victory!";//change korean later
        //victorySummaryText.text = "You have defeated the enemy!";//change korean later

        int totalCoins = 0;
        for (int i = 0; i < rewards.Count; i++)
        {
            RewardData reward = rewards[i];
            if (reward.rewardType == RewardType.Coin)
            {
                totalCoins += reward.amount;
            }
        }
        if (rewardTitleText != null)
        {
            rewardTitleText.text = "보상";
        }

        if (coinText != null)
        {
            coinText.text = $"{totalCoins} 코인";
        }

        if (coinImage != null)
        {
            coinImage.gameObject.SetActive(totalCoins > 0);
        }
    }

    public void ShowDefeat()
    {
        if(battleHUD != null)
            battleHUD.SetActive(false);

        resultUIRoot.SetActive(true);
        victoryPanel.SetActive(false);
        defeatPanel.SetActive(true);

        //defeatTitleText.text = "Defeat...";//change korean later
        //defeatSummaryText.text = "You have been defeated by the enemy. Better luck next time!";//change korean later
    }

    private void OnClickVictoryConfirm()
    {
        if (rewardConfirmed)
            return;

        rewardConfirmed = true;

        victoryNextButton.interactable = false;

        Debug.Log("1 보상 시작");
        ApplyRewards(currentRewards);

        Debug.Log("2 런타임 저장");
        if (player != null)
            player.SaveToRuntimeData(true);

        Debug.Log("3 카드 내구도 처리");
        if (CardManager.instance != null)
            CardManager.instance.CommitUsedLimitedCards();

        Debug.Log("4 맵 진행 저장");
        SaveBattleClearProgress();

        Debug.Log("5 씬 저장");
        if (SaveManager.Instance != null)
            SaveManager.Instance.SaveSceneState("NodeScene", false);

        Debug.Log("6 씬 이동");
        SceneManager.LoadScene("NodeScene");
    }

    private void OnClickExit()
    {
        int safeCoin = 0;

        if (PlayerRuntimeData.Instance != null)
            safeCoin = PlayerRuntimeData.Instance.coinInSafe;

        if (SaveManager.Instance != null)
            SaveManager.Instance.ResetAfterDefeat(safeCoin);

        SceneManager.LoadScene("TitleScene");
    }

    private void ApplyRewards(List<RewardData> rewards)
    {
        for (int i =0; i<rewards.Count; i++)
        {
            RewardData reward = rewards[i];

            switch (reward.rewardType)
            {
                case RewardType.Coin:
                    player.AddCoin(reward.amount);
                    Debug.Log($"코인 획득: {reward.amount}");
                    break;
                //case RewardType.Item:
                //    Debug.Log($"아이템 획득: {reward.rewardName} x{reward.amount}");
                //    break;

                // Chaos카드 패시브아이템 조건
                case RewardType.PassiveItem:
                    Debug.Log($"패시브 아이템 뽑기권 획득");
                    // TODO: 패시브 아이템 뽑기 UI 호출
                    break;
            }
        }
    }
    private void HideAll()
    {
        resultUIRoot.SetActive(false);
        victoryPanel.SetActive(false);
        defeatPanel.SetActive(false);
    }
    /// <summary>
    /// 배틀 클리어 시 맵 진행 상황을 저장하는 메서드.NodeMapManager가 존재하지 않을때를 위해 SaveManager의 MapProgressSaveData를 직접 수정하는 방식으로 구현되어 있다.
    /// </summary>
    private void SaveBattleClearProgress()
    {
        if (SaveManager.Instance == null || SaveManager.Instance.CurrentSaveData == null)
            return;

        MapProgressSaveData data = SaveManager.Instance.CurrentSaveData.mapProgress;

        if (data == null)
        {
            data = new MapProgressSaveData();
            SaveManager.Instance.CurrentSaveData.mapProgress = data;
        }

        int index = data.currentNodeIndex;

        if (index < 0)
            return;

        if (data.clearStates == null)
            data.clearStates = new List<bool>();

        while (data.clearStates.Count <= index)
            data.clearStates.Add(false);

        data.clearStates[index] = true;

        if (index == data.currentUnlockedIndex)
            data.currentUnlockedIndex++;

        SaveManager.Instance.SaveToFile();
    }
}
