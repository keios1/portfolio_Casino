using System.Collections;
using UnityEngine;

public class CameraShake : MonoBehaviour
{
    //싱글톤
    public static CameraShake Instance;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    // player.cs에서 takedamage함수에서 사용 (duration 지속 시간, magnitude 흔들림 강도)
    public void ShakeCamera(float duration, float magnitude)
    {
        StartCoroutine(ShakeRoutine(duration, magnitude));
    }

    private IEnumerator ShakeRoutine(float duration, float magnitude)
    {
        // 원래 카메라 위치 저장
        Vector3 originalPos = transform.localPosition;
        float elapsed = 0.0f;

        while (elapsed < duration)
        {
            // 랜덤한 위치로 카메라를 덜덜 떨게 만듭니다.
            float x = Random.Range(-1f, 1f) * magnitude;
            float y = Random.Range(-1f, 1f) * magnitude;

            transform.localPosition = new Vector3(originalPos.x + x, originalPos.y + y, originalPos.z);

            elapsed += Time.deltaTime;
            yield return null; // 다음 프레임까지 대기
        }

        transform.localPosition = originalPos;
    }
}
