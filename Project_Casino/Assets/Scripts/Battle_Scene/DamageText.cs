using UnityEngine;
using TMPro;
/// <summary>
/// 데미지 텍스트를 관리하는 클래스입니다.
/// 데미지나 회복량을 화면에 표시하고, 일정 시간 동안 위로 이동하면서 페이드아웃되는 효과를 구현합니다.
/// </summary>
public class DamageText : MonoBehaviour
{
    [Header("Movement and Fade Settings")]
    public float moveSpeed;
    public float bigMoveMultiplier;
    public float duration;

    [Header("Font Size")]
    public float normalFontSize;
    public float maxFontSize;
    public int maxFontEffectThreshold;

    [Header("Shake")]
    public float shakeAmount;      // 흔들리는 폭
    public float shakeSpeed;

    [Header("Text Settings")]
    private TMP_Text text;
    private float timer;
    private Color startColor;

    private Vector3 basePosition;
    private bool shouldShake = false;
    private bool bigMove = false;
    private bool smallMove = false;
    void Awake()
    {
        text = GetComponent<TMP_Text>();
        startColor = text.color;
        basePosition = transform.localPosition;
    }

    public void SetDamage(int damage, Color color)
    {
        text.text = damage.ToString();
        text.color = color;
        startColor = color;

        if (damage >= maxFontEffectThreshold)
        {
            text.fontSize = maxFontSize;

            shouldShake = true;

            bigMove = true;
            smallMove = false;
        }
        else
        {
            text.fontSize = normalFontSize;

            shouldShake = false;

            bigMove = false;
            smallMove = true;
        }

        timer = 0f;
        basePosition = transform.localPosition;
    }
    public void SetHeal(int value)
    {
        text.text = "+" + value.ToString();
        text.color = Color.green;
        startColor = text.color;

        // 힐은 수치와 상관없이 고정 크기
        text.fontSize = 250f;

        // 힐은 이동/흔들림 없음
        shouldShake = false;
        bigMove = false;
        smallMove = false;

        timer = 0f;
        basePosition = transform.localPosition;
    }
    void Update()
    {
        timer += Time.deltaTime;
        if (bigMove)
        {
            basePosition +=
                Vector3.up
                * moveSpeed
                * bigMoveMultiplier
                * Time.deltaTime;
        }
        else if (smallMove)
        {
            // 작은 수치 : 우상향
            Vector3 direction =
                new Vector3(0.2f, 0.5f, 0f).normalized;

            basePosition +=
                direction * moveSpeed * Time.deltaTime;
        }
        Vector3 finalPos = basePosition;

        if (shouldShake)
        {
            float offsetX =
                Mathf.Sin(timer * shakeSpeed) * (shakeAmount * 3f);

            finalPos += new Vector3(offsetX, 0f, 0f);
        }
        transform.localPosition = finalPos;

        // 페이드아웃
        float t = timer / duration;
        text.color = new Color(startColor.r, startColor.g, startColor.b, 1f - t);

        if (timer >= duration)
        {
            Destroy(gameObject);
        }
    }
}
