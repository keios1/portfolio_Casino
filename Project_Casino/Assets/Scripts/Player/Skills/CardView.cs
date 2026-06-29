using TMPro;
using UnityEngine;
using UnityEngine.UI;
/// <summary>
/// 카드의 기본적인 표시 및 클릭 이벤트를 담당하는 뷰 클래스.
/// 카드 데이터와 버튼을 바인딩하여 이름 표시 및 클릭 시 콜백을 전달한다.
/// 최소한의 UI 표현 역할만 수행하는 경량 View 컴포넌트이다.
/// </summary>
public class CardView : MonoBehaviour
{
    [SerializeField] private Button button;
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private AudioClip clickClip;

    private SkillCardData data;
    private System.Action<CardView> onClick;

    public SkillCardData Data => data;

    private void Reset()
    {
        AutoBind();
    }

    private void Awake()
    {
        AutoBind();
    }

    private void AutoBind()
    {
        if (button == null)
            button = GetComponent<Button>();

        if (titleText == null)
            titleText = GetComponentInChildren<TMP_Text>(true);
    }

    public void Bind(SkillCardData newData, System.Action<CardView> clickCallback)
    {
        data = newData;
        onClick = clickCallback;

        if (titleText != null)
            titleText.text = (data != null) ? data.cardName : "NULL";

        if (button != null)
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() =>
            {
                if (AudioManager.Instance != null && clickClip != null)
                    AudioManager.Instance.PlayUI(clickClip);

                onClick?.Invoke(this);
            });
        }
    }
}
