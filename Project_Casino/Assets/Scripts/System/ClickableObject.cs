using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClickableObject : MonoBehaviour
{
    [Header("이 오브젝트를 클릭했을 때 열릴 UI")]
    public GameObject targetUI;

    // 유니티가 마우스 클릭을 감지하는 내장 함수입니다 (Collider 필요)
    private void OnMouseDown()
    {
        // 카툰 트랜지션을 발동하면서 암전 시 UI를 켜줍니다.
       //  CartoonTransitionManager.Instance.PlayTransitionAtMouseWithoutAction();
    }
}
