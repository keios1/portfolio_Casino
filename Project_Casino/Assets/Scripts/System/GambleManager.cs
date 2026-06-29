using System.Collections;
using TMPro;
using UnityEngine;

public enum GambleResult
{
    Win,
    Lose,
    Draw
}

public class GambleManager : MonoBehaviour
{
    public bool isDebugging = true;

    public static GambleManager instance;
    public int betAmount = 0;

    public Deck deck;
    public Hand playerHand;
    public Hand dealerHand;
    public GameObject digitInput;
    public GameObject BackGround;
    public GameObject Result;
    public GameObject winImage;
    public GameObject loseImage;
    public GameObject drawImage;
    public Transform cardStack;
    public TextMeshProUGUI resultText;
    public TextMeshProUGUI gambleAmountText;
    public TextMeshProUGUI playerCoinText;
    public int GambleAmount = 0;

    private bool playerTurn = true;
    private bool gameOver = false;
    private GambleResult result;

    private void Awake()
    {
        instance = this;
    }

    void Start()
    {

    }

    public void PrepareGame()
    {
        digitInput.SetActive(true);
        GambleBegin.instance.digitInputUi.SetActive(true);

        UpdateCoinText();
    }

    public IEnumerator StartGame()
    {
        BackGround.SetActive(true);

        if (gambleAmountText != null)
        {
            gambleAmountText.gameObject.SetActive(true);
            gambleAmountText.text = $"베팅 금액 : {GambleAmount} G";
        }

        UpdateCoinText();

        deck.Prepare();
        playerHand.PrepareHand();
        dealerHand.PrepareHand();

        yield return playerHand.AddCard(deck.Draw());
        yield return dealerHand.AddCard(deck.Draw());
        yield return playerHand.AddCard(deck.Draw());
        yield return dealerHand.AddCard(deck.Draw());
        gameOver = false;
        playerTurn = true;
    }

    public void OnHit()
    {
        IEnumerator onHitCoroutine()
        {
            if (isDebugging) Debug.Log($">> OnHit() 실행\n playerTurn = {playerTurn}, gameOver = {gameOver}");
            if (!playerTurn || gameOver) yield break;

            yield return playerHand.AddCard(deck.Draw());

            if (playerHand.IsBust())
            {
                result = GambleResult.Lose;
                Debug.Log($"플레이어 Bust! 패배예요. P = {playerHand.GetScore()}, D = {dealerHand.GetScore()}");
                gameOver = true;
                yield return EndCoroutine();
            }
        }

        StartCoroutine(onHitCoroutine());
    }

    public void OnStay()
    {
        if (!playerTurn || gameOver) return;

        playerTurn = false;
        DealerTurn();
    }

    void DealerTurn()
    {
        IEnumerator onDealerTurnCoroutine()
        {
            while (dealerHand.GetScore() < 17)
            {
                yield return dealerHand.AddCard(deck.Draw());
            }

            int playerScore = playerHand.GetScore();
            int dealerScore = dealerHand.GetScore();

            if (dealerHand.IsBust())
            {
                result = GambleResult.Win;
                Debug.Log($"딜러 Bust! 승리예요.{dealerScore} {playerScore}");
                StartCoroutine(EndCoroutine());
            }
            else if (dealerScore > playerScore)
            {
                result = GambleResult.Lose;
                Debug.Log($"패배예요.{dealerScore} {playerScore}");
                StartCoroutine(EndCoroutine());
            }
            else if (dealerScore < playerScore)
            {
                result = GambleResult.Win;
                Debug.Log($"승리예요.{dealerScore} {playerScore}");
                StartCoroutine(EndCoroutine());
            }
            else
            {
                result = GambleResult.Draw;
                Debug.Log($"무승부예요.{dealerScore} {playerScore}");
                StartCoroutine(EndCoroutine());
            }

            gameOver = true;
        }

        StartCoroutine(onDealerTurnCoroutine());
    }

    private IEnumerator EndCoroutine()
    {
        yield return new WaitForSeconds(1f);

        winImage.SetActive(false);
        loseImage.SetActive(false);
        drawImage.SetActive(false);

        GameObject resultImage = null;

        switch (result)
        {
            case GambleResult.Win:
                winImage.SetActive(true);
                resultImage = winImage;
                break;

            case GambleResult.Lose:
                loseImage.SetActive(true);
                resultImage = loseImage;
                break;

            case GambleResult.Draw:
                drawImage.SetActive(true);
                resultImage = drawImage;
                break;
        }

        if (resultImage != null)
        {
            resultImage.transform.localPosition = Vector3.zero;
        }

        Result.SetActive(true);

        switch (result)
        {
            case GambleResult.Win:
                int rewardAmount = GambleAmount * 2;
                PlayerRuntimeData.Instance.AddCoin(rewardAmount);
                resultText.text = $"+ {rewardAmount} 코인";
                break;

            case GambleResult.Lose:
                resultText.text = $"- {GambleAmount} 코인";
                break;

            case GambleResult.Draw:
                PlayerRuntimeData.Instance.AddCoin(GambleAmount);
                resultText.text = "0 Coins";
                break;
        }

        UpdateCoinText();

        if (TopBarUIManager.Instance != null)
        {
            TopBarUIManager.Instance.RefreshNow();
        }

        yield return new WaitForSeconds(1.5f);

        Result.SetActive(false);

        winImage.SetActive(false);
        loseImage.SetActive(false);
        drawImage.SetActive(false);

        if (gambleAmountText != null)
        {
            gambleAmountText.gameObject.SetActive(false);
        }

        playerHand.PrepareHand();
        dealerHand.PrepareHand();

        BackGround.SetActive(false);

        PlayerRuntimeData.Instance.SaveToSaveData();

        DealerInteractionUI.Instance.CloseUI();
    }
    private void UpdateCoinText()
    {
        if (playerCoinText != null && PlayerRuntimeData.Instance != null)
        {
            int currentCoin = PlayerRuntimeData.Instance.coin;

            playerCoinText.text = $"남은 금액: {currentCoin} G";
        }
    }
}
