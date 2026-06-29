using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TurnAnnounceUI : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private TMP_Text turnText;
    [SerializeField] private Image backImage;
    [SerializeField] private Animator animator;

    [Header("Animation")]
    [SerializeField] private string stateName = "Turn";
    [SerializeField] private float textDelay = 0.6f;
    [SerializeField] private float visibleTime;

    private Coroutine routine;

    private void Awake()
    {
        HideImmediate();
    }

    public void Play(string message)
    {
        if (routine != null)
            StopCoroutine(routine);

        routine = StartCoroutine(PlayRoutine(message));
    }

    private IEnumerator PlayRoutine(string message)
    {
        SetImageAlpha(1f);
        SetTextAlpha(0f);

        if (turnText != null)
            turnText.text = message;

        if (animator != null)
            animator.Play(stateName, 0, 0f);

        // 텍스트 등장 딜레이
        yield return new WaitForSeconds(textDelay);

        SetTextAlpha(1f);

        // 유지 시간
        yield return new WaitForSeconds(visibleTime);

        HideImmediate();
        routine = null;
    }

    private void HideImmediate()
    {
        SetImageAlpha(0f);
        SetTextAlpha(0f);
    }

    private void SetImageAlpha(float alpha)
    {
        if (backImage == null) return;

        Color c = backImage.color;
        c.a = alpha;
        backImage.color = c;
    }

    private void SetTextAlpha(float alpha)
    {
        if (turnText == null) return;

        Color c = turnText.color;
        c.a = alpha;
        turnText.color = c;
    }
}
