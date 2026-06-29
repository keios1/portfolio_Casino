using TMPro;
using UnityEngine;
/// <summary>
/// 코인 토스 결과 아이콘의 상태를 제어하는 UI 컴포넌트.
/// 물리 토스 여부 표시 및 결과 아이콘의 시각적 상태를 설정한다.
/// </summary>
public class AutoTossResultIcon : MonoBehaviour
{
    public GameObject physicalMarkObject;
    public TMP_Text label; // H/T 고정 표시라면 사실 안 써도 됨

    public void SetPhysicalMark(bool isPhysical)
    {
        if (physicalMarkObject != null)
            physicalMarkObject.SetActive(isPhysical);
    }
}
