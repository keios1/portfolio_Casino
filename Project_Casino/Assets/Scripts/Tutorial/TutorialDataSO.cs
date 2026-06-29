using System.Collections.Generic;
using UnityEngine;

public enum TutorialProgressType
{
    Spacebar,           // 스페이스바를 누르면 다음으로 넘어감
    ClickTargetUI,      // 지정된 UI 버튼을 클릭해야만 넘어감
    WaitForGameEvent,   // 외부 스크립트에서 ForceNextPage()를 호출할 때까지 대기 (예: 적 사망, 마우스 이동 완료 등)
    RequireAction       // 특정 행동(함수 호출)을 요구함 (예: 상점 열기 시도 검증)
}

[System.Serializable]
public class TutorialPage
{
    public Sprite image;
    public string title;
    [TextArea(3, 10)]
    public string content;

    [Header("진행 방식 설정")]
    public TutorialProgressType progressType;

    [Header("UI 강제 클릭 설정 (ClickTargetUI 전용)")]
    [Tooltip("클릭해야 할 씬 내의 UI 오브젝트 이름")]
    public string targetButtonName;
    public bool showArrow;          // 화살표 UI 표시 여부

    [Header("행동 요구 설정 (RequireAction 전용)")]
    [Tooltip("기다릴 행동의 고유 이름 (예: OpenShop)")]
    public string requiredActionName;
    [Tooltip("다른 행동을 시도했을 때 화면에 띄울 경고 메시지")]
    public string wrongActionMessage;

    [Header("게임 로직 개입 (선택사항)")]
    [Tooltip("이 페이지가 시작될 때 다른 매니저에서 이 이벤트를 감지하여 아이템을 주거나 적 패턴을 실행합니다.")]
    public string customEventName; 
}

[CreateAssetMenu(fileName = "New Tutorial", menuName = "Tutorial/Tutorial Data")]
public class TutorialDataSO : ScriptableObject
{
    public List<TutorialPage> pages;
}
