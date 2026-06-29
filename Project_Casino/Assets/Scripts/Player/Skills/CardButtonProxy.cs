using UnityEngine;

/// <summary>
/// UI 상의 카드 버튼 클릭 이벤트를 받아 CardManager로 카드 사용 요청을 전달하는 중계(Proxy) 역할을 하는 클래스입니다.
/// </summary>
public class CardButtonProxy : MonoBehaviour
{
    [SerializeField] private CardManager executor;
    [SerializeField] private SkillCardData card;

    private void Awake()
    {
        if (executor == null)
            executor = FindObjectOfType<CardManager>();
    }

    public void Setup(CardManager newExecutor, SkillCardData newCard)
    {
        executor = newExecutor;
        card = newCard;

        CardUI ui = GetComponent<CardUI>();
        if (ui != null)
            ui.Setup(executor, card);
    }

    public void OnClickUseCard()
    {
        if (executor == null || card == null)
            return;

        CardUI ui = GetComponent<CardUI>();
        CardUseRequestResult result = executor.TryUseCard(card, ui);

        switch (result)
        {
            case CardUseRequestResult.Failed:
                //if (ui != null)
                //    ui.CancelPendingUse();
                break;

            case CardUseRequestResult.Pending:
                // 대기 상태 유지
                break;

            case CardUseRequestResult.Succeeded:
                // 실제 확정은 manager가 TriggerUsedSuccessfully 호출
                break;
        }
    }
}
