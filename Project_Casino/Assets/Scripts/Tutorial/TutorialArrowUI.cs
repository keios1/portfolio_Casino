using UnityEngine;

/// <summary>
/// 튜토리얼 손가락(화살표)의 위치를 관리합니다.
/// 둥둥 떠다니는 애니메이션을 제거하고 지정된 앵커에 즉시 표시합니다.
/// </summary>
public class TutorialArrowUI : MonoBehaviour
{
    public static TutorialArrowUI Instance;

    [Header("손가락(화살표) 이미지 객체")]
    [SerializeField] private GameObject arrowObject;

    private void Awake()
    {
        Instance = this;
        if (arrowObject != null) arrowObject.SetActive(false);
    }

    public void ShowArrow(string targetID)
    {
        if (string.IsNullOrEmpty(targetID))
        {
            HideArrow();
            return;
        }

        if (TutorialTarget.ActiveTargets.TryGetValue(targetID, out TutorialTarget target))
        {
            arrowObject.SetActive(true);

            Transform anchor = target.arrowAnchor != null ? target.arrowAnchor : target.transform;
            arrowObject.transform.position = anchor.position;

            Debug.Log($"[손가락 UI] <{targetID}> 위치에 손가락 표시!");
        }
        else
        {
            Debug.LogWarning($"[손가락 UI] <{targetID}> 타겟을 찾을 수 없어 손가락을 숨깁니다.");
            HideArrow();
        }
    }

    public void HideArrow()
    {
        if (arrowObject != null) arrowObject.SetActive(false);
    }
}
