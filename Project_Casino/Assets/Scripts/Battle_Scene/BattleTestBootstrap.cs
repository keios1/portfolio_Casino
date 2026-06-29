using UnityEngine;
using System.Collections.Generic;

public class BattleTestBootstrap : MonoBehaviour
{
    [Header("테스트 모드 활성화")]
    [SerializeField] private bool useTestSetup = false;

    [Header("테스트용 Collection 프리팹")]
    [SerializeField] private PlayerCardCollection collectionPrefab;

    [Header("테스트용 Deck 프리팹")]
    [SerializeField] private PlayerDeckLoadout deckLoadoutPrefab;

    [Header("테스트 카드 목록")]
    [SerializeField] private List<SkillCardData> testCards = new List<SkillCardData>();

    [Header("각 카드 몇 장 줄지")]
    [SerializeField] private int defaultCount;

    [Header("테스트용 RuntimeData")]
    [SerializeField] private bool createTestRuntimeData = true;
    [SerializeField] private int testMaxHp = 100;
    [SerializeField] private int testCurrentHp = 100;
    [SerializeField] private int testCoin = 100;
    [SerializeField] private int testCoinInSafe = 0;

    private void Awake()
    {
        if (!useTestSetup)
        {
            Debug.Log("[BattleTestBootstrap] 테스트 모드 꺼짐 → 실행 안 함");
            return;
        }

        Debug.Log("[BattleTestBootstrap] 테스트 모드 실행");

        // 싱글톤 확보
        PlayerCardCollection collection = PlayerCardCollection.Instance;
        if (collection == null)
        {
            if (collectionPrefab != null)
                collection = Instantiate(collectionPrefab);
            else
            {
                Debug.LogError("PlayerCardCollection Prefab 연결 안됨!");
                return;
            }
        }

        PlayerDeckLoadout deck = PlayerDeckLoadout.Instance;
        if (deck == null)
        {
            if (deckLoadoutPrefab != null)
                deck = Instantiate(deckLoadoutPrefab);
            else
            {
                Debug.LogError("PlayerDeckLoadout Prefab 연결 안됨!");
                return;
            }
        }

        // 🔹 카드 지급 (세이브 X)
        for (int i = 0; i < testCards.Count; i++)
        {
            SkillCardData card = testCards[i];
            if (card == null) continue;

            int currentOwned = collection.GetOwnedCount(card);
            if (currentOwned < defaultCount)
            {
                int need = defaultCount - currentOwned;
                collection.AddCardRuntimeOnly(card, need);
            }
        }

        // 🔹 덱 구성 (세이브 X)
        deck.ClearDeckRuntimeOnly();

        for (int i = 0; i < testCards.Count; i++)
        {
            SkillCardData card = testCards[i];
            if (card == null) continue;

            int owned = collection.GetOwnedCount(card);
            for (int j = 0; j < owned; j++)
            {
                if (!deck.AddToDeckRuntimeOnly(card))
                    break;
            }
        }
        Debug.Log("[BattleTestBootstrap] 테스트 덱 세팅 완료 (세이브 영향 없음)");

        if (createTestRuntimeData && PlayerRuntimeData.Instance == null)
        {
            GameObject runtimeObj = new GameObject("PlayerRuntimeData_Test");
            PlayerRuntimeData runtimeData = runtimeObj.AddComponent<PlayerRuntimeData>();

            runtimeData.InitTestData(
                testMaxHp,
                testCurrentHp,
                testCoin,
                testCoinInSafe
            );

            Debug.Log("[BattleTestBootstrap] 테스트용 PlayerRuntimeData 생성 완료");
        }

        Debug.Log("[BattleTestBootstrap] 테스트 모드 실행");
    }
}
