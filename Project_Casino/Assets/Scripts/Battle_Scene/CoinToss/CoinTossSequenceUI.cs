using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 코인 토스 시퀀스 결과를 화면에 표시하는 UI 클래스.
/// 결과 아이콘 생성, 누적 데미지 및 횟수 표시,
/// 진행 상태 텍스트 업데이트를 담당한다.
/// </summary>
public class CoinTossSequenceUI : MonoBehaviour
{
    [SerializeField] private GameObject backObject;

    public Transform resultContainer;
    public GameObject headIconPrefab;
    public GameObject tailIconPrefab;

    public TMP_Text totalText;
    public TMP_Text countText;
    public TMP_Text infoText;

    [Header("Single Toss Result")]
    [SerializeField] private Image singleResultImage;
    [SerializeField] private Sprite headSprite;
    [SerializeField] private Sprite tailSprite;

    public void SetBackVisible(bool visible)
    {
        if (backObject != null)
            backObject.SetActive(visible);
    }

    public void ResetUI(int diceValue)
    {
        SetBackVisible(false);

        if (resultContainer != null)
        {
            for (int i = resultContainer.childCount - 1; i >= 0; --i)
            {
                Destroy(resultContainer.GetChild(i).gameObject);
            }
        }

        if (resultContainer != null)
        {
            for (int i = resultContainer.childCount - 1; i >= 0; --i)
            {
                Destroy(resultContainer.GetChild(i).gameObject);
            }
        }

        if (totalText != null)
        {
            totalText.gameObject.SetActive(true);
            totalText.text = $"총 데미지 : {diceValue}";
        }

        if (countText != null)
        {
            countText.gameObject.SetActive(true);
            countText.text = $"앞면 수 : 0";
        }

        if (infoText != null)
        {
            infoText.gameObject.SetActive(true);
            infoText.text = "클릭 & 드래그로 코인 던지기";
        }
    }

    public void ShowReady()
    {
        if (infoText == null) return;

        infoText.text = "클릭 & 드래그로 코인 던지기";
    }

    public void SetRunning(bool running)
    {
        if (infoText == null) return;

        infoText.text = running
            ? "코인 던지기 중..."
            : "코인 던지기 종료";
    }

    public void AddResult(CoinTossManager.CoinFace face, bool isFirstPhysical)
    {
        SetBackVisible(true);

        GameObject prefab = (face == CoinTossManager.CoinFace.Head) ? headIconPrefab : tailIconPrefab;
        if (prefab == null || resultContainer == null) return;

        GameObject go = Instantiate(prefab, resultContainer);

        AutoTossResultIcon icon = go.GetComponent<AutoTossResultIcon>();
        if (icon != null)
        {
            icon.SetPhysicalMark(isFirstPhysical);
        }
    }

    public void UpdateTotal(int totalDamage, int successCount)
    {
        if (totalText != null)
            totalText.text = $"총 데미지 : {totalDamage}";

        if (countText != null)
            countText.text = $"앞면 수 : {successCount}";
    }
    public void ShowInfoOnlyReady()
    {
        SetBackVisible(false);

        if (resultContainer != null)
        {
            for (int i = resultContainer.childCount - 1; i >= 0; --i)
            {
                Destroy(resultContainer.GetChild(i).gameObject);
            }
        }

        if (resultContainer != null)
        {
            for (int i = resultContainer.childCount - 1; i >= 0; --i)
            {
                Destroy(resultContainer.GetChild(i).gameObject);
            }
        }

        if (totalText != null)
            totalText.gameObject.SetActive(false);

        if (countText != null)
            countText.gameObject.SetActive(false);

        if (infoText != null)
        {
            infoText.gameObject.SetActive(true);
            infoText.text = "클릭 & 드래그로 코인 던지기";
        }
    }

    public void HideSingleResult()
    {
        if (singleResultImage != null)
            singleResultImage.gameObject.SetActive(false);
    }

    public void ShowSingleResult(CoinTossManager.CoinFace face)
    {
        SetBackVisible(true);

        if (singleResultImage == null)
            return;

        singleResultImage.sprite =
            face == CoinTossManager.CoinFace.Head
            ? headSprite
            : tailSprite;

        singleResultImage.gameObject.SetActive(true);
    }
}
