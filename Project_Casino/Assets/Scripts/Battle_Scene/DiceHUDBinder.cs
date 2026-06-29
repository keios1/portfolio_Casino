using UnityEngine;
/// <summary>
/// Player의 주사위 데이터와 DiceUI를 연결하는 바인더 클래스.
/// 플레이어의 주사위 상태 변화 이벤트를 구독하여 UI에 반영한다.
/// </summary>
public class DiceHUDBinder : MonoBehaviour
{
    [SerializeField] private Player player;
    [SerializeField] private DiceUI diceUI;

    private void OnEnable()
    {
        if (player != null)
            player.OnDiceChanged += HandleDiceChanged;
    }

    private void OnDisable()
    {
        if (player != null)
            player.OnDiceChanged -= HandleDiceChanged;
    }

    private void Start()
    {
        if (player != null && diceUI != null)
            diceUI.SetAll(player.GetDiceSnapshot());
    }

    private void HandleDiceChanged(int[] slots)
    {
        if (diceUI != null)
            diceUI.SetAll(slots);
    }
}
