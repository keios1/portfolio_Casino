using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public enum NodeType
{
    NormalBattle,
    EliteBattle,
    BossBattle,
    Gacha,
    MiniGame,
    Roward
}

/// <summary>
/// 맵노드들의 위치를 리스트로 가지고 있는 매니저
/// </summary>
public class MapNodeManager : MonoBehaviour
{
    public static MapNodeManager Instance { get; private set; }

    [Header("노드 리스트")]
    public List<EnteringStage> nodeList = new List<EnteringStage>();
    public List<bool> clearStates = new List<bool>();

    public int currentUnlockedIndex = 0; // 현재 도전 가능한 스테이지 인덱스
    public int selectedNodeIndex;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    /// <summary>
    /// 노드 씬에 들어왔을 때 노드 리스트를 다 지우고 이름순으로 정렬해서 삽입
    /// </summary>
    /// <param name="scene"></param>
    /// <param name="mode"></param>
    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "NodeScene")
        {
            nodeList.Clear();

            EnteringStage[] nodes = FindObjectsOfType<EnteringStage>();
            System.Array.Sort(nodes, (a, b) => string.Compare(a.name, b.name));

            for (int i = 0; i < nodes.Length; i++)
            {
                nodeList.Add(nodes[i]);

                if (clearStates.Count <= i)
                    clearStates.Add(false);

                nodes[i].isClear = clearStates[i];
                nodes[i].RefreshNodeVisual();
            }
            LoadMapProgress();

            if (PlayerRuntimeData.Instance != null && PlayerRuntimeData.Instance.justFinishedTutorialBattle)
            {
                PlayerRuntimeData.Instance.justFinishedTutorialBattle = false; 

                if (clearStates.Count > 0 && !clearStates[0])
                {
                    clearStates[0] = true;
                    nodeList[0].isClear = true;

                    if (currentUnlockedIndex == 0) currentUnlockedIndex = 1;

                    SaveMapProgress();

                    nodeList[0].RefreshNodeVisual();
                    if (nodeList.Count > 1) nodeList[1].RefreshNodeVisual();
                }
            }
        }
    }

    /// <summary>
    /// 클릭한 노드에 진입 가능한지 확인
    /// </summary>
    /// <param name="node"></param>
    /// <returns></returns>
    public bool CanEnteringStage(EnteringStage node)
    {
        if (TutorialManager.Instance != null && TutorialManager.Instance.IsTutorialActive)
        {
            Debug.Log("튜토리얼 진행 중이라 노드에 진입할 수 없습니다.");
            return false; 
        }

        if (DealerInteractionUI.Instance != null && DealerInteractionUI.Instance.HasAnyPopupOpen())
        {
            return false;
        }

        if (node.isClear)
        {
            return false;
        }

        int index = nodeList.IndexOf(node);
        Debug.Log($"[노드 진입 체크] 클릭한 노드: {node.gameObject.name} (인덱스: {index}) | 현재 해금된 인덱스 기준: {currentUnlockedIndex}");
        if (index == -1)
            return false;

        return index <= currentUnlockedIndex;
    }

    public void SelectStage(EnteringStage node)
    {
        selectedNodeIndex = nodeList.IndexOf(node);
        Debug.Log($"{selectedNodeIndex}번 노드 선택됨. 이동 준비 완료.");
    }

    /// <summary>
    /// 스테이지 클리어시 호출
    /// </summary>
    /// <param name="index"></param>
    public void ClearStage(int index)
    {
        if (index >= 0 && index < clearStates.Count)
        {
            clearStates[index] = true;
        }

        if (index >= 0 && index < nodeList.Count)
        {
            nodeList[index].isClear = true;
        }

        if (index == currentUnlockedIndex)
        {
            currentUnlockedIndex++;
        }

        Debug.Log($"{index}번 노드 클리어");
    }

    /// <summary>
    /// 현재 진입중인 노드 인덱스 반환
    /// </summary>
    /// <returns></returns>
    public int GetEnteringNodeIndex()
    {
        return selectedNodeIndex;
    }

    /// <summary>
    /// 맵의 진행 상황을 저장하는 함수
    /// </summary>
    public void SaveMapProgress()
    {
        if (SaveManager.Instance == null || SaveManager.Instance.CurrentSaveData == null)
            return;

        MapProgressSaveData data = SaveManager.Instance.CurrentSaveData.mapProgress;

        data.currentNodeIndex = selectedNodeIndex;
        data.currentUnlockedIndex = currentUnlockedIndex;

        data.clearStates.Clear();
        for (int i = 0; i < clearStates.Count; i++)
        {
            data.clearStates.Add(clearStates[i]);
        }

        SaveManager.Instance.SaveToFile();
    }

    /// <summary>
    /// 맵의 진행 상황을 불러오는 함수
    /// </summary>
    public void LoadMapProgress()
    {
        if (SaveManager.Instance == null || SaveManager.Instance.CurrentSaveData == null)
            return;

        MapProgressSaveData data = SaveManager.Instance.CurrentSaveData.mapProgress;

        selectedNodeIndex = Mathf.Max(0, data.currentNodeIndex);
        currentUnlockedIndex = Mathf.Max(0, data.currentUnlockedIndex);

        clearStates.Clear();

        for (int i = 0; i < nodeList.Count; i++)
        {
            bool isClear = false;

            if (data.clearStates != null && i < data.clearStates.Count)
                isClear = data.clearStates[i];

            clearStates.Add(isClear);
            nodeList[i].isClear = isClear;

            nodeList[i].RefreshNodeVisual();
        }
    }
}
