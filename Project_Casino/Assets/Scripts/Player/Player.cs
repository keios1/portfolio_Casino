using System;
using UnityEngine;

public class Player : MonoBehaviour, IBattleUnit
{
    [Header("Stats")]
    [SerializeField] private int maxHp;
    [SerializeField] private int currentHp;
    [SerializeField] private int coin = 0;
    [SerializeField] private int coinInSafe;
    [SerializeField] private int currentShield =0;

    [Header("Dice")]
    [SerializeField] private int diceSlotCount = 6;
    [SerializeField] private int[] diceSlots; // 0이면 비어있음, 1~6이면 주사위 값

    public int MaxHp => maxHp;
    public int CurrentHp { get => currentHp; private set => currentHp = value; }
    public int Coin => coin;
    public int CoinInSafe => coinInSafe;
    public int DiceSlotCount => diceSlots != null ? diceSlots.Length : 0;
    public int CurrentShield => currentShield;


    //UI나 다른 시스템이 구독할 수 있는 이벤트들
    public event Action<int, int> OnHpChanged;
    public event Action<int> OnCoinChanged;
    public event Action<int[]> OnDiceChanged;

    public event Action onPlayerAttackedEvent;

    private int bonusDiceNextTurn = 0;
    private float damageMultiplierThisTurn = 1f; // 승부수카드 이번턴 데미지배수

    private void Awake()
    {
        if (diceSlotCount < 1)
            diceSlotCount = 1;

        diceSlots = new int[diceSlotCount];
        currentHp = Mathf.Clamp(currentHp, 0, maxHp);
        if (PlayerRuntimeData.Instance != null)
        {
            PlayerRuntimeData.Instance.LoadToPlayer(this);
        }
        RaiseAll();
    }

    // 게임이 종료되거나 씬이 전환될 때 PlayerRuntimeData에 현재 상태를 저장하는 메서드
    public void ApplyRuntimeData(int newMaxHp, int newCurrentHp, int newCoin, int newcoinInSafe)
    {
        maxHp = Mathf.Max(1, newMaxHp);
        currentHp = Mathf.Clamp(newCurrentHp, 0, maxHp);
        coin = Mathf.Max(0, newCoin);
        coinInSafe = Mathf.Max(0, newcoinInSafe);

        RaiseAll();
    }

    public void SaveToRuntimeData(bool saveToFile = true)
    {
        if (PlayerRuntimeData.Instance == null)
            return;

        PlayerRuntimeData.Instance.SaveFromPlayer(this, saveToFile);
    }

    public int GetBonusDiceValue()
    {
        return bonusDiceNextTurn;
    }


    public void TakeDamage(int amount)
    {
        if (amount <= 0) return;

        PassiveItemUnderhand underhand = null;

        if (PlayerPassiveItemCollection.Instance != null &&
            PlayerPassiveItemCollection.Instance.ownedPassives != null)
        {
            underhand = PlayerPassiveItemCollection.Instance.ownedPassives
                .Find(x => x is PassiveItemUnderhand) as PassiveItemUnderhand;
        }

        if (underhand != null && underhand.IgnoreAttack())
        {
            if (DamageTextManager.Instance != null)
                DamageTextManager.Instance.ShowDamage(0, transform.position + Vector3.up * 1.5f, Color.cyan);

            return;
        }
        if (underhand != null && underhand.IgnoreAttack())
        {
            if (DamageTextManager.Instance != null)
            {
                DamageTextManager.Instance.ShowDamage(0, transform.position + Vector3.up * 1.5f, Color.cyan);
            }
            return;
        }

        int finalDamage = Mathf.RoundToInt(amount * damageMultiplierThisTurn);
        int shieldBefore = currentShield;
        int blockedDamage = Mathf.Min(currentShield, finalDamage);
        currentShield -= blockedDamage;
        int hpDamage = finalDamage - blockedDamage;
        bool shieldWasBroken = shieldBefore > 0 && currentShield <= 0;

        TopBarUIManager.Instance?.UpdateShieldText(currentShield);

        if (shieldWasBroken)
        {
            DamageScreenEffect.Instance?.PlayShieldBreak();
        }

        if (hpDamage <= 0)
        {
            Debug.Log($"[Player] Shield blocked all damage. Remain Shield: {currentShield}");

            if (DamageTextManager.Instance != null)
            {
                DamageTextManager.Instance.ShowDamage(0, transform.position + Vector3.up * 1.5f, Color.cyan);
            }

            //쉴드로 막았어도 '피격'이므로 무조건 알림
            onPlayerAttackedEvent?.Invoke();
            return;
        }

        currentHp = Mathf.Max(0, currentHp - hpDamage);

        // 체력이 깎였을 때도 무조건 알림
        onPlayerAttackedEvent?.Invoke();

        OnHpChanged?.Invoke(currentHp, maxHp);
        SaveToRuntimeData(false);

        if (DamageTextManager.Instance != null)
        {
            DamageTextManager.Instance.ShowDamage(hpDamage, transform.position + Vector3.up * 1.5f, Color.red);
        }

        if (CameraShake.Instance != null)
        {
            CameraShake.Instance.ShakeCamera(0.2f, 0.3f);
        }

        DamageScreenEffect.Instance?.FlashDamage();

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX(AudioManager.Instance.sounds.playerHit);
        }
    }

    public void AddShield(int amount)
    {
        if (amount <= 0) return;

        currentShield += amount;

        DamageScreenEffect.Instance?.ShowShield();
        TopBarUIManager.Instance?.UpdateShieldText(currentShield);

        Debug.Log($"Shield added: {amount}, Current Shield: {currentShield}");

        if (DamageTextManager.Instance != null)
        {
            DamageTextManager.Instance.ShowDamage(
                amount,
                transform.position + Vector3.up * 1.5f,
                Color.cyan);
        }
    }

    public void ClearShield()
    {
        if (currentShield <= 0) return;

        Debug.Log($"Shield cleared: {currentShield}");
        currentShield = 0;

        DamageScreenEffect.Instance?.HideShield();
        TopBarUIManager.Instance?.UpdateShieldText(0);
    }

    public void Heal(int amount, bool saveToFile = true)
    {
        if (amount <= 0) return;

        currentHp = Mathf.Min(maxHp, currentHp + amount);
        OnHpChanged?.Invoke(currentHp, maxHp);

        SaveToRuntimeData(saveToFile);

        DamageScreenEffect.Instance?.FlashHeal();

        if (DamageTextManager.Instance != null)
        {
            Vector3 textPos = transform.position + Vector3.up * 2f;
            DamageTextManager.Instance.ShowHeal(amount, textPos);
        }
    }

    public bool SpendCoin(int amount)
    {
        if (amount <= 0) return true;
        if (coin < amount) return false;

        coin -= amount;
        OnCoinChanged?.Invoke(coin);
        SaveToRuntimeData();
        return true;
    }

    public void AddCoin(int amount)
    {
        if (amount <= 0) return;
        coin += amount;
        OnCoinChanged?.Invoke(coin);
        SaveToRuntimeData();
    }

    public void ClearDice()
    {
        if (diceSlots == null) return;

        for (int i = 0; i < diceSlots.Length; i++)
            diceSlots[i] = 0;

        OnDiceChanged?.Invoke(diceSlots);
    }

    // index 위치에 value값을 설정하려고 시도합니다. 성공하면 true, 실패하면 false를 반환합니다.
    public bool TrySetDice(int index, int value)
    {
        if (diceSlots == null) return false;
        if (index < 0 || index >= diceSlots.Length) return false;
        if (value < 0 || value > 6) return false;

        diceSlots[index] = value;
        OnDiceChanged?.Invoke(diceSlots);
        return true;
    }

    // 현재 주사위 슬롯의 상태를 복사하여 반환합니다. 반환된 배열은 원본과 독립적이므로 수정해도 원본에 영향을 주지 않습니다.
    public int[] GetDiceSnapshot()
    {
        if (diceSlots == null)
            return Array.Empty<int>();

        int[] copy = new int[diceSlots.Length];
        Array.Copy(diceSlots, copy, diceSlots.Length);
        return copy;
    }

    // 주사위 슬롯의 상태를 snapshot 배열로 복원합니다. snapshot이 null이거나 길이가 diceSlots와 다르면 아무 작업도 수행하지 않습니다.
    public void RestoreDiceSnapshot(int[] snapshot)
    {
        if (snapshot == null) return;

        if (diceSlots == null || diceSlots.Length != snapshot.Length)
            diceSlots = new int[snapshot.Length];

        Array.Copy(snapshot, diceSlots, snapshot.Length);
        OnDiceChanged?.Invoke(diceSlots);
    }

    // 첫 번째로 발견되는 유효한 주사위(1~6)를 소비하여 그 값을 consumedValue로 반환합니다. 소비에 성공하면 true, 실패하면 false를 반환합니다.
    public bool TryConsumeOneDice(out int consumedValue)
    {
        consumedValue = 0;
        if (diceSlots == null) return false;

        for (int i = 0; i < diceSlots.Length; i++)
        {
            int v = diceSlots[i];
            if (v >= 1 && v <= GetMaxDiceValue())
            {
                consumedValue = v;
                diceSlots[i] = 0;

                CompactDiceSlots();
                OnDiceChanged?.Invoke(diceSlots);
                return true;
            }
        }

        return false;
    }

    // index 위치에 유효한 주사위(1~6)가 있으면 그것을 소비하여 consumedValue로 반환합니다. 소비에 성공하면 true, 실패하면 false를 반환합니다.
    public bool TryConsumeDiceAt(int index, out int consumedValue)
    {
        consumedValue = 0;
        if (diceSlots == null) return false;
        if (index < 0 || index >= diceSlots.Length) return false;

        int v = diceSlots[index];
        if (v < 1 || v > GetMaxDiceValue()) return false;

        consumedValue = v;
        diceSlots[index] = 0;

        CompactDiceSlots();
        OnDiceChanged?.Invoke(diceSlots);
        return true;
    }

    // value값이 유효한 주사위(1~6)라면 diceSlots에서 첫 번째 빈 슬롯(0인 위치)에 그 값을 추가합니다.
    // 추가에 성공하면 true와 추가된 인덱스를 반환하고, 실패하면 false를 반환합니다.
    public bool TryAddRolledDice(int value, out int addedIndex)
    {
        addedIndex = -1;
        if (diceSlots == null) return false;
        if (value < 1 || value > GetMaxDiceValue()) return false;

        for (int i = 0; i < diceSlots.Length; i++)
        {
            if (diceSlots[i] == 0)
            {
                diceSlots[i] = value;
                addedIndex = i;
                OnDiceChanged?.Invoke(diceSlots);
                return true;
            }
        }

        return false;
    }

    // diceSlots 배열에서 유효한 주사위(1~6)만 앞으로 당기고 나머지는 0으로 채웁니다.
    // 예를 들어 [0, 3, 0, 5, 2]는 [3, 5, 2, 0, 0]이 됩니다.
    private void CompactDiceSlots()
    {
        if (diceSlots == null) return;

        int write = 0;
        for (int read = 0; read < diceSlots.Length; read++)
        {
            int v = diceSlots[read];
            if (v >= 1 && v <= GetMaxDiceValue())
            {
                if (write != read)
                    diceSlots[write] = v;
                write++;
            }
        }

        for (int i = write; i < diceSlots.Length; i++)
            diceSlots[i] = 0;
    }

    // 현재 diceSlots에서 유효한 주사위(1~6)의 개수를 반환합니다.
    public int GetRemainingDiceCount()
    {
        if (diceSlots == null) return 0;

        int cnt = 0;
        for (int i = 0; i < diceSlots.Length; i++)
        {
            int v = diceSlots[i];
            if (v >= 1 && v <= GetMaxDiceValue())
                cnt++;
        }

        return cnt;
    }

    // 다음 턴에 사용할 수 있는 보너스 주사위를 amount만큼 추가합니다.
    // amount가 0 이하이면 아무 작업도 수행하지 않습니다.
    public void AddBonusDiceNextTurn(int amount)
    {
        if (amount <= 0) return;
        bonusDiceNextTurn += amount;
    }

    // 현재 설정된 보너스 주사위 개수를 반환하고, 보너스 주사위를 0으로 초기화합니다.
    public int ConsumeBonusDice()
    {
        int v = bonusDiceNextTurn;
        bonusDiceNextTurn = 0;
        return v;
    }
    // 현재 설정된 보너스 주사위 개수를 반환하지만 초기화하지는 않습니다.
    public int PeekBonusDiceNextTurn()
    {
        return bonusDiceNextTurn;
    }
    // 현재 HP, 최대 HP, 코인, 주사위 슬롯 상태를 이벤트로 모두 알립니다. 주로 초기화 시점에 UI를 최신 상태로 맞추기 위해 사용됩니다.
    private void RaiseAll()
    {
        OnHpChanged?.Invoke(currentHp, maxHp);
        OnCoinChanged?.Invoke(coin);
        OnDiceChanged?.Invoke(diceSlots);
    }

    /// <summary>
    /// 플레이어 최대 HP 변경
    /// </summary>
    /// <param name="amount"></param>
    public void ModifyMaxHP(int amount)
    {
        Debug.Log($"ModifyMaxHP 함수 실행 : {amount}");
        maxHp += amount;
        /* 현재 체력 채워줄거면 주석 해제
        currentHp = Mathf.Clamp(currentHp + amount, 1, maxHp); */

        OnHpChanged?.Invoke(currentHp, maxHp);
        SaveToRuntimeData();
        Debug.Log($"최대 체력 변경: {maxHp}");
    }

    //승부수 이번턴 데미지 배수
    public void SetDamageMultiplier(float value) 
    {
        damageMultiplierThisTurn = value;
    }

    public void ResetDamageModifier()
    {
        damageMultiplierThisTurn = 1f;
    }

    public int GetMaxDiceValue()
    {
        if (PlayerPassiveItemCollection.Instance == null ||
            PlayerPassiveItemCollection.Instance.ownedPassives == null)
        {
            return 6;
        }

        var cheatingDice = PlayerPassiveItemCollection.Instance.ownedPassives
            .Find(x => x is PassiveItemCheatingDice) as PassiveItemCheatingDice;

        if (cheatingDice != null && cheatingDice.currentStock >= 3)
        {
            return 10;
        }

        return 6;
    }
}
