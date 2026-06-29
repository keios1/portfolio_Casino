using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// 패링 결과 종류
public enum ParryResult { None, Nullify, Reflect, Absorb, GameWinner }

public class ParryManager : MonoBehaviour
{
    public static ParryManager Instance;

    [Header("시스템 연결")]
    public CardManager cardManager;
    public CardHandUI cardHandUI;

    [Header("UI 연결")]
    public GameObject parryCardPanel; // 3초 대기할 카드 선택 패널
    public Transform cardSpawnRoot;   // 카드가 생성될 부모
    public GameObject cardPrefab;     // CardHandUI에서 쓰는 진짜 카드 프리팹

    private bool isCardSelected = false;
    private ParryResult finalResult = ParryResult.None;
    private SkillCardData selectedCardData = null;

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    public IEnumerator StartParrySequence(int incomingDamage, Action<ParryResult> onResult)
    {
        isCardSelected = false;
        finalResult = ParryResult.None;
        selectedCardData = null;

        if (parryCardPanel != null) parryCardPanel.SetActive(true);

        // 1. 진짜 손패에서 패링 카드를 찾습니다.
        List<SkillCardData> parryCards = cardHandUI.GetParryCards();

        if (parryCards == null || parryCards.Count == 0)
        {
            Debug.Log("[패링] 손패에 패링 카드가 없습니다. 3초 대기...");
            yield return new WaitForSeconds(3.0f);

            if (parryCardPanel != null) parryCardPanel.SetActive(false);
            onResult?.Invoke(ParryResult.None);
            yield break;
        }

        // 2. 카드가 있다면 화면(cardSpawnRoot)에 소환
        List<GameObject> spawnedCards = new List<GameObject>();
        foreach (SkillCardData cardData in parryCards)
        {
            GameObject go = Instantiate(cardPrefab, cardSpawnRoot);

            go.transform.localScale = new Vector3(1.5f, 1.5f, 1.5f);

            spawnedCards.Add(go);

            // 기존 카드 UI 셋업 
            CardUI ui = go.GetComponent<CardUI>();
            if (ui != null && cardManager != null)
                ui.Setup(cardManager, cardData);

            // 일반 턴에 쓰는 기능(드래그 등)을 강제로 끕니다.
            if (go.GetComponent<CardButtonProxy>() != null) Destroy(go.GetComponent<CardButtonProxy>());
            if (go.GetComponent<CardView>() != null) Destroy(go.GetComponent<CardView>());

            Button btn = go.GetComponent<Button>();
            if (btn == null) btn = go.AddComponent<Button>();

            btn.onClick.AddListener(() => OnParryCardClicked(cardData));
        }

        // 3. 타이머 및 입력 대기
        float timeLimit = 10.0f;
        float timer = 0f;

        Debug.Log("[패링] 카드를 마우스로 클릭(제한 시간 10초)");

        // 3초가 지나거나, 카드를 클릭할 때까지만 반복
        while (!isCardSelected && timer < timeLimit)
        {
            timer += Time.deltaTime;
            yield return null;
        }

        if (!isCardSelected)
        {
            Debug.Log("[패링 실패] 시간 초과 카드를 고르지 못해 그대로 맞습니다.");
            finalResult = ParryResult.None;
        }

        // 4. 소환했던 카드들 지우고 창 닫기
        foreach (GameObject go in spawnedCards) Destroy(go);
        spawnedCards.Clear();

        if (parryCardPanel != null) parryCardPanel.SetActive(false);

        // 5. 카드를 썼다면 손패에서 버리기
        if (isCardSelected && selectedCardData != null)
        {
            cardHandUI.ConsumeParryCard(selectedCardData);
        }

        onResult?.Invoke(finalResult);
    }

    // 마우스로 카드를 클릭했을 때 실행되는 함수
    private void OnParryCardClicked(SkillCardData cardData)
    {
        isCardSelected = true;
        selectedCardData = cardData;

        // 고른 카드의 ID에 따라 무효/반사/흡수 지정
        if (cardData.id == 32050)
        {
            Debug.Log("-> [무효] 카드 클릭됨!");
            PlayParryNullify();
            finalResult = ParryResult.Nullify;
        }
        else if (cardData.id == 42050)
        {
            Debug.Log("-> [반사] 카드 클릭됨!");
            PlayParryReflect();
            finalResult = ParryResult.Reflect;
        }
        else if (cardData.id == 42056) // 흡수 카드 추가
        {
            Debug.Log("-> [흡수] 카드 클릭됨!");
            PlayParryAbsorb();
            finalResult = ParryResult.Absorb;
        }
        else if (cardData.id == 22050) // 승부수 카드 추가
        {
            Debug.Log("-> [승부수] 카드 클릭됨!");
            finalResult = ParryResult.GameWinner;
        }
    }

    private void PlayParryReflect()
    {
        if (AudioManager.Instance == null) return;
        if (AudioManager.Instance.skillSounds == null) return;

        AudioManager.Instance.PlaySkillSFX(
            AudioManager.Instance.skillSounds.Reflect
        );
    }
    private void PlayParryNullify()
    {
        if (AudioManager.Instance == null) return;
        if (AudioManager.Instance.skillSounds == null) return;

        AudioManager.Instance.PlaySkillSFX(
            AudioManager.Instance.skillSounds.Nullify
        );
    }
    private void PlayParryAbsorb()
    {
        if (AudioManager.Instance == null) return;
        if (AudioManager.Instance.skillSounds == null) return;

        AudioManager.Instance.PlaySkillSFX(
            AudioManager.Instance.skillSounds.Absorb
        );
    }
}
