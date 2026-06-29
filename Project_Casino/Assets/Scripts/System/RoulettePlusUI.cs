using System;
using System.Collections;
using UnityEngine;

public class RoulettePlusUI : MonoBehaviour
{
    public static RoulettePlusUI Instance;
    public RectTransform rouletteImage;

    [Header("Settings")]
    public float spinDuration = 6f;
    public float maxSpeed = 2000f;

    private bool isSpinning = false;

    private void Awake()
    {
        Instance = this;
        gameObject.SetActive(false);
    }

    public IEnumerator Spin(Action<int> onResult)
    {
        if (isSpinning) yield break;
        gameObject.SetActive(true);
        yield return StartCoroutine(SpinRoutine(onResult));
    }

    private IEnumerator SpinRoutine(Action<int> onResult)
    {
        isSpinning = true;
        if (AudioManager.Instance != null &&
        AudioManager.Instance.skillSounds != null)
        {
            AudioManager.Instance.PlaySkillSFX(
                AudioManager.Instance.skillSounds.wheelSpin
            );
        }
        rouletteImage.rotation = Quaternion.Euler(0, 0, 0);

        yield return null;
        yield return null;

        float targetRotation = 2160f + UnityEngine.Random.Range(0f, 1080f);
        float totalRotated = 0f;
        float elapsed = 0f;

        while (elapsed < spinDuration && totalRotated < targetRotation)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / spinDuration;
            float currentSpeed = Mathf.Lerp(maxSpeed, 0f, t);
            float delta = currentSpeed * Time.deltaTime;
            rouletteImage.Rotate(0, 0, -delta);
            totalRotated += delta;
            yield return null;
        }

        isSpinning = false;

        yield return new WaitForSeconds(1f);

        float finalZ = rouletteImage.localEulerAngles.z;
        int result = GetResultFromAngle(finalZ);
        onResult?.Invoke(result);

        yield return new WaitForSeconds(0.5f);
        gameObject.SetActive(false);
    }

    private int GetResultFromAngle(float angle)
    {
        if (angle < 0) angle += 360f;
        angle = angle % 360f;

        if (angle >= 0 && angle < 72f) return 4;  // x4
        if (angle >= 72f && angle < 144f) return 3;  // x3
        if (angle >= 144f && angle < 216f) return -2; // x-2
        if (angle >= 216f && angle < 288f) return 3;  // x3
        return -1;                                     // x-1 (288~360)
    }
}
