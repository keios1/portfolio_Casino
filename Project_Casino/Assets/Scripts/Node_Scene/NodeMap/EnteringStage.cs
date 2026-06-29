using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 노드마다 적용된 씬 전환 스크립트
/// </summary>
public class EnteringStage : MonoBehaviour
{
    [Header("노드 외형")]
    [SerializeField] private GameObject normalVisual;
    [SerializeField] private GameObject clearedVisual;

    public GameObject NormalVisual => normalVisual;
    public GameObject ClearedVisual => clearedVisual;

    public bool isClear = false;    // 클리어 여부

    private void Start()
    {
        RefreshNodeVisual();
    }

    private void OnMouseDown()
    {
        if (MapNodeManager.Instance.CanEnteringStage(this))
        {
            // 3. 플레이어 이동 및 만화 트랜지션 연출 실행
            PlayerMapTraveler traveler = FindObjectOfType<PlayerMapTraveler>();
            if (traveler != null)
            {
                StartCoroutine(traveler.MoveAndExecute(transform.position, () =>
                {
                    StartTransitionAndLoadScene();
                }));
            }
            else
            {
                // 월드에 플레이어 이동 컴포넌트가 없다면 연출 없이 즉시 진입 (방어 코드)
                StartTransitionAndLoadScene();
            }
        }
        else
        {
            // 진입 불가의 구체적인 이유(UI 오픈 상태, 이미 클리어함 등)는 매니저가 로그를 띄워줍니다.
            Debug.Log($"[{gameObject.name}] 매니저 판단 하에 진입이 거부되었습니다.");
        }
    }

    // 플레이어가 노드에 발을 디디는 순간 실행될 트랜지션 공장
    private void StartTransitionAndLoadScene()
    {
        if (CartoonTransitionManager.Instance != null)
        {
            // 플레이어가 서 있는 노드의 화면 좌표를 구합니다.
            Vector3 screenPos = Camera.main.WorldToScreenPoint(transform.position);

            // 암전이 완료되는 칼같은 타이밍에 실제 유니티 씬을 바꿉니다.
            CartoonTransitionManager.Instance.PlayTransitionAtPosition(screenPos, () =>
            {
                PerformStageEntry();
            });
        }
        else
        {
            // 매니저가 없으면 딜레이 없이 바로 로드
            PerformStageEntry();
        }
    }

    private void PerformStageEntry()
    {
        MapNodeManager.Instance.SelectStage(this);
        MapNodeManager.Instance.SaveMapProgress();

        if (SaveManager.Instance != null)
            SaveManager.Instance.SaveSceneState(gameObject.name, true);

        SceneManager.LoadScene(gameObject.name);
    }

    public void SetStageCleared()
    {
        isClear = true;
        RefreshNodeVisual();
    }

    public void RefreshNodeVisual()
    {
        if(normalVisual != null && clearedVisual != null)
        {
            normalVisual.SetActive(!isClear);
            clearedVisual.SetActive(isClear);
        }
    }
}
