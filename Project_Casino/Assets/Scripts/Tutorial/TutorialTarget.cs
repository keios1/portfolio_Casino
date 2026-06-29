using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 튜토리얼 화살표가 가리킬 대상 오브젝트에 부착하는 스크립트입니다.
/// </summary>
public class TutorialTarget : MonoBehaviour
{
    // 전역에서 타겟들을 쉽게 찾기 위한 딕셔너리
    public static Dictionary<string, TutorialTarget> ActiveTargets = new Dictionary<string, TutorialTarget>();

    [Header("타겟 고유 이름 (TutorialDataSO의 Target Button Name과 동일하게 작성)")]
    public string targetID;

    [Header("화살표가 위치할 앵커 (빈 오브젝트 연결, 비워두면 자기 자신 위치)")]
    public Transform arrowAnchor;

    private void Awake()
    {
        if (!string.IsNullOrEmpty(targetID))
        {
            ActiveTargets[targetID] = this;
        }
    }

    private void OnDestroy()
    {
        if (!string.IsNullOrEmpty(targetID) && ActiveTargets.ContainsKey(targetID))
        {
            ActiveTargets.Remove(targetID);
        }
    }
}
