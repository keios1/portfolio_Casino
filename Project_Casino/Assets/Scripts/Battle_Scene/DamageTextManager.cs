using UnityEngine;
/// <summary>
/// 데미지 및 회복 수치를 화면에 표시하는 매니저 클래스.
/// 월드 좌표를 스크린 좌표로 변환하여 UI 캔버스에 텍스트를 생성하고,
/// 데미지 및 힐 연출을 담당한다.
/// </summary>
public class DamageTextManager : MonoBehaviour
{
    public static DamageTextManager Instance { get; private set; }

    [SerializeField] private GameObject damageTextPrefab;
    [SerializeField] private Canvas canvas;

    private Camera mainCam;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        mainCam = Camera.main;
    }

    public void ShowDamage(int damage, Vector3 worldPosition, Color color)
    {
        Vector3 screenPos = Camera.main.WorldToScreenPoint(worldPosition);

        GameObject obj = Instantiate(damageTextPrefab, canvas.transform);
        obj.transform.position = screenPos;

        obj.GetComponent<DamageText>().SetDamage(damage, color);
    }

    public void ShowHeal(int value, Vector3 worldPosition)
    {
        Vector3 screenPos = Camera.main.WorldToScreenPoint(worldPosition);

        GameObject obj = Instantiate(damageTextPrefab, canvas.transform);
        obj.transform.position = screenPos;

        obj.GetComponent<DamageText>().SetHeal(value);
    }
}
