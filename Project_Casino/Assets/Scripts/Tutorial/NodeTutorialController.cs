using UnityEngine;
using System.Collections;

public class NodeTutorialController : MonoBehaviour
{
    [Header("노드 튜토리얼 통합 데이터")]
    public TutorialDataSO nodeTutorialData;

    private bool isWaitingForCameraUp = false;

    private void Start()
    {
        if (TutorialManager.Instance != null)
        {
            TutorialManager.Instance.OnTutorialCustomEvent += HandleTutorialEvent;

            if (PlayerRuntimeData.Instance.hasCompletedBattleTutorial &&
                !PlayerRuntimeData.Instance.hasCompletedNodeTutorial)
            {
                Invoke("StartTutorial", 0.5f);
            }
            else
            {
                Debug.Log("[노드 튜토리얼] 이미 완료했거나, 아직 전투 튜토리얼을 안 깨서 스킵합니다.");
            }
        }

        if (NodeSceneCameraController.Instance != null)
        {
            NodeSceneCameraController.Instance.OnDealerViewArrived.AddListener(OnCameraArrived);
        }
    }

    private void StartTutorial()
    {
        if (nodeTutorialData != null)
        {
            TutorialManager.Instance.ShowTutorial(nodeTutorialData);

            if (NodeSceneCameraController.Instance != null)
            {
                NodeSceneCameraController.Instance.SetLookDownLock(true);
                NodeSceneCameraController.Instance.SetLookUpLock(true);
            }

            PlayerRuntimeData.Instance.hasCompletedNodeTutorial = true;
            PlayerRuntimeData.Instance.SaveToSaveData();
        }
    }

    private void OnDestroy()
    {
        if (TutorialManager.Instance != null)
            TutorialManager.Instance.OnTutorialCustomEvent -= HandleTutorialEvent;

        if (NodeSceneCameraController.Instance != null)
            NodeSceneCameraController.Instance.OnDealerViewArrived.RemoveListener(OnCameraArrived);
    }

    private void HandleTutorialEvent(string eventName)
    {
        if (eventName == "RequireMouseUp")
        {
            isWaitingForCameraUp = true;
            if (NodeSceneCameraController.Instance != null)
                NodeSceneCameraController.Instance.SetLookUpLock(false);
        }
        else if (eventName == "UnlockLookDown")
        {
            if (NodeSceneCameraController.Instance != null)
                NodeSceneCameraController.Instance.SetLookDownLock(false);
        }
        else if (eventName == "LockLookUp")
        {
            if (NodeSceneCameraController.Instance != null)
            {
                NodeSceneCameraController.Instance.SetLookUpLock(true);
                Debug.Log("[노드 튜토리얼] 마우스 올리기 다시 잠금!");
            }
        }
        else if (eventName == "UnlockLookUp")
        {
            if (NodeSceneCameraController.Instance != null)
            {
                NodeSceneCameraController.Instance.SetLookUpLock(false);
                Debug.Log("[노드 튜토리얼] 마우스 올리기 최종 잠금 해제!");
            }
        }
    }

    // 카메라가 위로 도착했을 때 실행되는 함수
    private void OnCameraArrived(bool isUp)
    {
        if (isWaitingForCameraUp && isUp)
        {
            isWaitingForCameraUp = false;
            Debug.Log("[노드 튜토리얼] 카메라 상단 도달 감지! 다음 페이지로 강제 진행합니다.");

            TutorialManager.Instance.TryAction("CameraArrived");
        }

        else if (!isUp)
        {
            if (TutorialManager.Instance != null && TutorialManager.Instance.IsTutorialActive)
            {
                TutorialManager.Instance.TryAction("CameraMovedDown");
            }
        }
    }

    // 헌혈기 오브젝트(또는 버튼)를 클릭했을 때 실행할 함수
    public void OnBloodMachineClicked()
    {
        if (TutorialManager.Instance != null && TutorialManager.Instance.IsTutorialActive)
        {
            if (TutorialManager.Instance.TryAction("ClickBloodMachine"))
            {
                Debug.Log("[노드 튜토리얼] 헌혈기 클릭 성공! 설명을 시작합니다.");
            }
            else
            {
                Debug.Log("[노드 튜토리얼] 아직 헌혈기를 누를 타이밍이 아닙니다.");
            }
        }
        else
        {
            Debug.Log("일반 헌혈기 팝업 열기");
        }
    }
}
