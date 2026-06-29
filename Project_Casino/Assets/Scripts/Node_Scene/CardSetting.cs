using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class CardSetting : MonoBehaviour
{
    public RectTransform[] cards;

    [Header("Layout Settings")]
    public float radius = 1000f;
    public float angleSpacing = 8f;
    public float yOffset = -50f;      // 카드의 전체적인 높낮이 조절용
    public float verticalWeight = 1.0f; // 곡률의 깊이 조절 (0에 가까울수록 평평함)

    [Header("Visual Settings")]
    public float baseScale = 0.8f;    // 기본 크기
    public float focusScale = 1.2f;   // 중앙에 왔을 때 크기
    public float visibleRange = 2.5f; // 보이는 범위 (값이 클수록 카드가 많이 보임)

    [Header("Animation")]
    public float targetIndex = 0;
    public float moveSpeed = 5f;
    private float currentLerpIndex = 0f;

    private int hoveredIndex = -1;

    public void SetHover(int index) => hoveredIndex = index;

    void Update()
    {
        currentLerpIndex = Mathf.Lerp(currentLerpIndex, targetIndex, Time.deltaTime * moveSpeed);

        for (int i = 0; i < cards.Length; i++)
        {
            float relativeIndex = i - currentLerpIndex;
            float distance = Mathf.Abs(relativeIndex);

            float angle = relativeIndex * angleSpacing;
            float rad = angle * Mathf.Deg2Rad;

            float x = Mathf.Sin(rad) * radius;
            float y = (Mathf.Cos(rad) * radius - radius) * verticalWeight + yOffset;

            cards[i].anchoredPosition = new Vector2(x, y);
            cards[i].localRotation = Quaternion.Euler(0, 0, -angle);

            float scale = Mathf.Lerp(focusScale, baseScale, distance / visibleRange);

            if (i == hoveredIndex) scale *= 1.15f;
            cards[i].localScale = Vector3.one * Mathf.Max(scale, baseScale);

            if (cards[i].TryGetComponent(out CanvasGroup cg))
            {
                cg.alpha = Mathf.Clamp01(visibleRange - distance);
            }

            cards[i].SetSiblingIndex((int)(100 - distance * 10));
        }
    }
}
