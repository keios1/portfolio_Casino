using System;
using System.Collections;
using UnityEngine;

public class GameWinnerUI : MonoBehaviour
{
    public static GameWinnerUI Instance;

    [Header("Card1")]
    public GameObject card1Success;
    public GameObject card1Fail;
    public GameObject card1Back;

    [Header("Card2")]
    public GameObject card2Success;
    public GameObject card2Fail;
    public GameObject card2Back;

    private bool card1IsSuccess;
    private bool isSelecting;

    private Action<bool> resultCallback;

    private void Awake()
    {
        Instance = this;
        gameObject.SetActive(false);
    }

    public void Open(Action<bool> callback)
    {
        resultCallback = callback;

        // 카드1이 성공 카드인지 랜덤 결정
        card1IsSuccess = UnityEngine.Random.value < 0.5f;

        // 모든 앞면 숨김
        card1Success.SetActive(false);
        card1Fail.SetActive(false);

        card2Success.SetActive(false);
        card2Fail.SetActive(false);

        // 뒷면 표시
        card1Back.SetActive(true);
        card2Back.SetActive(true);

        isSelecting = true;

        gameObject.SetActive(true);
    }

    public void SelectCard(int cardIndex)
    {
        if (!isSelecting)
            return;

        isSelecting = false;

        StartCoroutine(RevealCard(cardIndex));
    }

    private IEnumerator RevealCard(int selectedCard)
    {
        bool success =
            selectedCard == 1
            ? card1IsSuccess
            : !card1IsSuccess;

        if (selectedCard == 1)
        {
            yield return StartCoroutine(
                FlipCard(
                    card1Back,
                    success ? card1Success : card1Fail
                )
            );
        }
        else
        {
            yield return StartCoroutine(
                FlipCard(
                    card2Back,
                    success ? card2Success : card2Fail
                )
            );
        }

        yield return new WaitForSeconds(1f);

        resultCallback?.Invoke(success);

        gameObject.SetActive(false);
    }

    private IEnumerator FlipCard(GameObject back, GameObject front)
    {
        RectTransform backRect = back.GetComponent<RectTransform>();
        Vector3 originalScale = backRect.localScale; // 원래 크기 저장
        float duration = 0.15f;
        float time = 0f;

        while (time < duration)
        {
            time += Time.deltaTime;
            float scaleX = Mathf.Lerp(1f, 0f, time / duration);
            backRect.localScale = new Vector3(originalScale.x * scaleX, originalScale.y, originalScale.z);
            yield return null;
        }

        back.SetActive(false);
        front.SetActive(true);

        RectTransform frontRect = front.GetComponent<RectTransform>();
        frontRect.localScale = new Vector3(0f, originalScale.y, originalScale.z);
        time = 0f;

        while (time < duration)
        {
            time += Time.deltaTime;
            float scaleX = Mathf.Lerp(0f, 1f, time / duration);
            frontRect.localScale = new Vector3(originalScale.x * scaleX, originalScale.y, originalScale.z);
            yield return null;
        }

        frontRect.localScale = originalScale; // (1,1,1) 대신 원래 크기로
    }
}
