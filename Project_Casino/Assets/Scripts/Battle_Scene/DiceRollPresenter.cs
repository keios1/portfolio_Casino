using System.Collections;
using UnityEngine;
using UnityEngine.UI;
/// <summary>
/// 주사위 굴림 연출을 담당하는 클래스.
/// 중앙 표시 및 슬롯 이동 애니메이션을 통해 주사위 결과를 시각적으로 표현하고,
/// 최종 결과를 DiceUI에 반영한다.
/// </summary>
public class DiceRollPresenter : MonoBehaviour
{
    [Header("Center Popup")]
    [SerializeField] private GameObject centerRoot; // 중앙 표시 오브젝트
    [SerializeField] private Image centerImage;

    [Header("Sprites: 0=empty, 1~6=face")]
    [SerializeField] private Sprite[] diceSprites = new Sprite[7];

    [Header("Targets")]
    [SerializeField] private DiceUI diceUI;                      // 왼쪽하단 슬롯 갱신
    [SerializeField] private RectTransform[] slotTargets = new RectTransform[6]; // DiceSlot_0~5의 RectTransform

    [Header("Timing")]
    //[SerializeField] private float showSeconds = 0.5f;
    [SerializeField] private float moveSeconds = 0.25f;

    private Coroutine co;
    private Vector3 centerStartPos;

    private void Awake()
    {
        if (centerImage != null)
            centerStartPos = centerImage.rectTransform.position;

        if (centerRoot != null)
            centerRoot.SetActive(false);
    }

    public void Play(int diceValue, int slotIndex)
    {
        if (co != null) StopCoroutine(co);
        co = StartCoroutine(CoPlay(diceValue, slotIndex));
    }

    private IEnumerator CoPlay(int diceValue, int slotIndex)
    {
        int v = Mathf.Clamp(diceValue, 1, 6);

        // 1) 중앙 ON + 스프라이트 적용
        //if (centerRoot != null) centerRoot.SetActive(true);
        //if (centerImage != null && diceSprites != null && diceSprites.Length >= 7)
        //    centerImage.sprite = diceSprites[v];

        // 중앙에서 잠깐 보여주기
        //yield return new WaitForSeconds(showSeconds);

        // 2) 슬롯으로 이동(선택)
        if (moveSeconds > 0f && centerImage != null && slotTargets != null &&
            slotIndex >= 0 && slotIndex < slotTargets.Length && slotTargets[slotIndex] != null)
        {
            RectTransform from = centerImage.rectTransform;
            Vector3 start = from.position;
            Vector3 end = slotTargets[slotIndex].position;

            float t = 0f;
            while (t < 1f)
            {
                t += Time.deltaTime / moveSeconds;
                from.position = Vector3.Lerp(start, end, t);
                yield return null;
            }

            // 원위치 복귀(다음 연출 위해)
            from.position = centerStartPos;
        }

        // 3) 슬롯 갱신
        if (diceUI != null)
            diceUI.SetSlot(slotIndex, v);

        // 4) 중앙 OFF
        if (centerRoot != null) centerRoot.SetActive(false);

        co = null;
    }
}
