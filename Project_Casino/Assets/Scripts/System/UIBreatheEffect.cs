using UnityEngine;
using UnityEngine.UI;

public class UIBreatheEffect : MonoBehaviour
{
    [Header("숨쉬기 설정")]
    [Tooltip("빛이 깜빡이는 속도")]
    public float speed = 3f;

    [Tooltip("최소 투명도 (0~1)")]
    public float minAlpha = 0.2f;

    [Tooltip("최대 투명도 (0~1)")]
    public float maxAlpha = 1.0f;

    private Image img;

    private void Awake()
    {
        img = GetComponent<Image>();
    }

    private void Update()
    {
        if (img == null) return;

        // 시간에 따라 수학 공식(Sin 그래프)을 이용해 알파값이 부드럽게 왔다 갔다 하게 만듭니다.
        float alpha = Mathf.Lerp(minAlpha, maxAlpha, (Mathf.Sin(Time.time * speed) + 1f) / 2f);

        Color c = img.color;
        c.a = alpha;
        img.color = c;
    }
}
