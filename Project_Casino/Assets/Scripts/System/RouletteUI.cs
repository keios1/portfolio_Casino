using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class RouletteUI : MonoBehaviour
{
    public RectTransform rouletteImage;
    public GameObject panel;
    public float spinDuration = 4f;
    public float maxSpeed = 2000f;

    // index: 0    1    2   3
    public int[] results = { 2, -2, -1, 3 };

    private bool isSpinning = false;
    public static RouletteUI Instance;

    void Start()
    {
        panel.SetActive(false);
    }

    void Awake()
    {
        Instance = this;
    }

    public IEnumerator Spin(Action<int> onResult)
    {
        panel.SetActive(true);
        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(panel.GetComponent<RectTransform>());
        yield return null;
        yield return null;
        yield return StartCoroutine(SpinRoutine(onResult));
        panel.SetActive(false);
    }

    IEnumerator SpinRoutine(Action<int> onResult)
    {
        isSpinning = true;
        if (AudioManager.Instance != null &&
        AudioManager.Instance.skillSounds != null)
        {
            AudioManager.Instance.PlaySkillSFX(
                AudioManager.Instance.skillSounds.wheelSpin
            );
        }
        float randomStartAngle = UnityEngine.Random.Range(0f, 360f);
        rouletteImage.rotation = Quaternion.Euler(0, 0, randomStartAngle);

        float targetRotation = 2160f + UnityEngine.Random.Range(0f, 1080f);
        float totalRotated = 0f;
        float elapsed = 0f;

        while (elapsed < spinDuration && totalRotated < targetRotation)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / spinDuration;
            float speed = Mathf.Lerp(maxSpeed, 0, t);
            float delta = speed * Time.deltaTime;
            rouletteImage.Rotate(0, 0, -delta);
            totalRotated += delta;
            yield return null;
        }

        isSpinning = false;

        // 1초 대기 후 결과 추출
        yield return new WaitForSeconds(1f);

        float finalZ = rouletteImage.eulerAngles.z;
        int result = GetResultFromAngle(finalZ);
        onResult?.Invoke(result);
    }

    //int GetResultFromAngle(float angle)
    //{
    //    angle = (angle + 360f) % 360f;
    //    angle = (360f - angle + 90f) % 360f;
    //    float slice = 360f / results.Length; // 4칸 = 90도
    //    int index = Mathf.FloorToInt(angle / slice) % results.Length;
    //    return results[index];
    //}
    int GetResultFromAngle(float angle)
    {
        float normalizedAngle = angle;

        if (normalizedAngle < 0)
        {
            normalizedAngle += 360f;
        }

        Debug.Log($"실제 Z: {normalizedAngle:F1}, 판정용 각도: {normalizedAngle:F1}");

        if (normalizedAngle >= 0 && normalizedAngle < 90) return 3;      // x3
        if (normalizedAngle >= 90 && normalizedAngle < 180) return -1;   // -1
        if (normalizedAngle >= 180 && normalizedAngle < 270) return -2;  // -2
        return 2;
    }
}
