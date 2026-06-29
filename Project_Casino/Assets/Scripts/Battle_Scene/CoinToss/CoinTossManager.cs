using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class CoinTossManager : MonoBehaviour
{
    public static CoinTossManager Instance { get; private set; }

    public enum CoinFace
    {
        Head,
        Tail
    }
    [Header("Canvas")]
    [SerializeField] private Canvas mainCanvas;

    [Header("Refs")]
    public GameObject fieldRoot;
    public CoinThrowController coin;
    public Transform coinSpawn;
    public Rigidbody coinRb;

    [Header("Chance")]
    [Range(0f, 1f)]
    public float successRate = 0.5f;

    [Header("Sequence")]
    public float autoTossInterval = 0.5f;

    [Header("Sequence Timing")]
    public float firstResultHoldTime;
    public float finalResultHoldTime;

    [Header("UI")]
    public CoinTossSequenceUI sequenceUI;

    private Action<CoinFace> singleResultCallback;
    private Action<CoinTossSequenceResult> sequenceResultCallback;

    private bool runningPhysicalToss;
    private bool sequenceMode;
    private Coroutine sequenceRoutine;

    private int sequenceDiceValue;
    private int sequenceTotalDamage;
    private int sequenceSuccessCount;
    private readonly List<CoinFace> sequenceFaces = new List<CoinFace>();

    private CoinFace pendingFirstFace = CoinFace.Head;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        if (coinRb == null && coin != null)
            coinRb = coin.GetComponent<Rigidbody>();

        if (fieldRoot != null)
            fieldRoot.SetActive(false);

        if (sequenceUI != null)
            sequenceUI.gameObject.SetActive(false);
    }
    private void SetMainCanvasVisible(bool visible)
    {
        if (mainCanvas == null)
            return;

        mainCanvas.enabled = visible;
    }

    public void Open(Action<CoinFace> callback)
    {
        if (!ValidateRefs())
        {
            callback?.Invoke(CoinFace.Tail);
            return;
        }

        PrepareForNewToss();
        SetCoinTossBlock(true);

        singleResultCallback = callback;
        sequenceResultCallback = null;
        sequenceMode = false;

        if (fieldRoot != null)
            fieldRoot.SetActive(true);

        SetMainCanvasVisible(false);

        if (sequenceUI != null)
        {
            sequenceUI.gameObject.SetActive(true);
            sequenceUI.HideSingleResult();
            sequenceUI.ShowInfoOnlyReady();
        }

        ResetCoinToSpawn();

        pendingFirstFace = RollFace();
        coin.SetPendingFace(pendingFirstFace);

        runningPhysicalToss = true;
    }

    public void OpenSequence(int diceValue, Action<CoinTossSequenceResult> callback)
    {
        if (!ValidateRefs())
        {
            CoinTossSequenceResult fail = new CoinTossSequenceResult();
            fail.baseDiceValue = diceValue;
            fail.totalDamage = 0;
            fail.successCount = 0;
            fail.faces.Add(CoinFace.Tail);

            callback?.Invoke(fail);
            return;
        }

        PrepareForNewToss();
        SetCoinTossBlock(true);

        sequenceMode = true;
        sequenceResultCallback = callback;
        singleResultCallback = null;

        sequenceDiceValue = diceValue;
        sequenceTotalDamage = 0;
        sequenceSuccessCount = 0;
        sequenceFaces.Clear();

        if (fieldRoot != null)
            fieldRoot.SetActive(true);

        SetMainCanvasVisible(false);

        if (sequenceUI != null)
        {
            sequenceUI.gameObject.SetActive(true);
            sequenceUI.ResetUI(diceValue);
            sequenceUI.ShowReady();
            //sequenceUI.SetRunning(true);
        }

        ResetCoinToSpawn();

        pendingFirstFace = RollFace();
        coin.SetPendingFace(pendingFirstFace);

        runningPhysicalToss = true;
    }

    private bool ValidateRefs()
    {
        if (coin == null || coinSpawn == null)
        {
            Debug.LogError("[CoinTossManager] Missing refs. coin / coinSpawn 확인 필요");
            return false;
        }

        return true;
    }

    private void Update()
    {
        if (!runningPhysicalToss)
            return;

        if (coin == null)
            return;

        if (!coin.IsThrown)
            return;

        if (!coin.IsSettled)
            return;

        runningPhysicalToss = false;

        CoinFace firstFace = coin.CurrentFace;

        if (sequenceRoutine != null)
        {
            StopCoroutine(sequenceRoutine);
            sequenceRoutine = null;
        }

        if (!sequenceMode)
        {
            sequenceRoutine = StartCoroutine(CoResolveSingle(firstFace));
            return;
        }

        sequenceRoutine = StartCoroutine(CoResolveSequence(firstFace));
    }

    private IEnumerator CoResolveSingle(CoinFace firstFace)
    {
        yield return new WaitForSeconds(firstResultHoldTime);

        if (sequenceUI != null)
            sequenceUI.SetRunning(false);

        if (sequenceUI != null)
            sequenceUI.ShowSingleResult(firstFace);

        yield return new WaitForSeconds(finalResultHoldTime);

        singleResultCallback?.Invoke(firstFace);

        sequenceRoutine = null;
        CloseAndResetField();
    }

    private IEnumerator CoResolveSequence(CoinFace firstFace)
    {
        ApplyFaceToSequence(firstFace, true);

        yield return new WaitForSeconds(firstResultHoldTime);

        while (firstFace == CoinFace.Head)
        {
            CoinFace autoFace = RollFace();
            firstFace = autoFace;

            ApplyFaceToSequence(autoFace, false);

            yield return new WaitForSeconds(autoTossInterval);

            if (autoFace == CoinFace.Tail)
                break;
        }

        if (sequenceUI != null)
            sequenceUI.SetRunning(false);

        yield return new WaitForSeconds(finalResultHoldTime);

        CoinTossSequenceResult result = new CoinTossSequenceResult();
        result.baseDiceValue = sequenceDiceValue;
        result.totalDamage = sequenceTotalDamage;
        result.successCount = sequenceSuccessCount;
        result.faces = new List<CoinFace>(sequenceFaces);

        sequenceResultCallback?.Invoke(result);

        sequenceRoutine = null;
        CloseAndResetField();
    }

    private void ApplyFaceToSequence(CoinFace face, bool isFirstPhysical)
    {
        sequenceFaces.Add(face);

        if (face == CoinFace.Head)
        {
            sequenceSuccessCount++;
            sequenceTotalDamage += sequenceDiceValue;
        }

        if (sequenceUI != null)
        {
            sequenceUI.AddResult(face, isFirstPhysical);
            sequenceUI.UpdateTotal(sequenceTotalDamage, sequenceSuccessCount);
        }
    }

    private CoinFace RollFace()
    {
        return UnityEngine.Random.value < successRate ? CoinFace.Head : CoinFace.Tail;
    }

    private void PrepareForNewToss()
    {
        if (sequenceRoutine != null)
        {
            StopCoroutine(sequenceRoutine);
            sequenceRoutine = null;
        }

        runningPhysicalToss = false;

        singleResultCallback = null;
        sequenceResultCallback = null;
        sequenceMode = false;

        sequenceDiceValue = 0;
        sequenceTotalDamage = 0;
        sequenceSuccessCount = 0;
        sequenceFaces.Clear();

        pendingFirstFace = CoinFace.Head;

        SetCoinTossBlock(false);

        if (sequenceUI != null)
            sequenceUI.gameObject.SetActive(false);
    }

    private void ResetCoinToSpawn()
    {
        if (coin == null || coinSpawn == null)
            return;

        coin.ResetCoin(coinSpawn.position, coinSpawn.rotation);

        if (coinRb != null)
            coinRb.Sleep();
    }

    private void CloseAndResetField()
    {
        runningPhysicalToss = false;

        SetCoinTossBlock(false);

        ResetCoinToSpawn();

        if (fieldRoot != null)
            fieldRoot.SetActive(false);

        if (sequenceUI != null)
        {
            sequenceUI.HideSingleResult();

            sequenceUI.gameObject.SetActive(false);
        }

        SetMainCanvasVisible(true);
    }
    private void SetCoinTossBlock(bool isBlocked)
    {
        if (ButtonBlockerManager.Instance == null)
            return;

        if (ButtonBlockerManager.Instance.coinTossBlock == null)
            return;

        ButtonBlockerManager.Instance.SetBlock(
            ButtonBlockerManager.Instance.coinTossBlock,
            isBlocked
        );
    }
    public void SetSequenceRunningText(bool running)
    {
        if (sequenceUI == null) return;

        sequenceUI.SetRunning(running);
    }
}
