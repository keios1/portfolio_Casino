using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public enum SlotResultType { OOO, XXX, STAR, CHERRY }

public class SlotMachineManager : MonoBehaviour
{
    [Header("UI Panel")]
    public GameObject slotUIPanel;

    [Header("UI References (3개의 세로 릴)")]
    public RectTransform[] reelWindows;

    [Header("Resources")]
    public Sprite oSprite;
    public Sprite xSprite;
    public Sprite starSprite;
    public Sprite cherrySprite;

    [Header("Bingo Highlight Effect (각 칸별 개별 애니메이션)")]
    public GameObject highlightLeft;   // 4번 칸 (왼쪽 릴)
    public GameObject highlightCenter; // 5번 칸 (가운데 릴)
    public GameObject highlightRight;  // 6번 칸 (오른쪽 릴)

    [Header("Settings")]
    public float spinDuration = 2.0f;
    public float spinSpeed = 2000f;

    [Header("UI Settings")]
    public float symbolSpacing = 20f;

    public bool IsSpinning { get; private set; } = false;

    private Image[,] reelImages;
    private float symbolHeight;
    private float cellHeight;
    private const int VISIBLE_SLOTS = 3;
    private const int IMAGES_PER_REEL = 4;

    private void Start()
    {
        CloseUI();
    }

    public void ShowUI() { slotUIPanel.SetActive(true); }

    public void CloseUI()
    {
        slotUIPanel.SetActive(false);
        HideBingoHighlights();
    }

    private void Awake()
    {
        reelImages = new Image[reelWindows.Length, IMAGES_PER_REEL];

        for (int i = 0; i < reelWindows.Length; i++)
        {
            RectTransform window = reelWindows[i];

            float totalSpacing = symbolSpacing * (VISIBLE_SLOTS - 1);
            symbolHeight = (window.rect.height - totalSpacing) / VISIBLE_SLOTS;
            cellHeight = symbolHeight + symbolSpacing;

            for (int j = 0; j < IMAGES_PER_REEL; j++)
            {
                Image img = window.GetChild(j).GetComponent<Image>();
                reelImages[i, j] = img;

                RectTransform rt = img.rectTransform;
                rt.anchorMin = new Vector2(0.5f, 0f);
                rt.anchorMax = new Vector2(0.5f, 0f);
                rt.pivot = new Vector2(0.5f, 0f);
                rt.sizeDelta = new Vector2(window.rect.width, symbolHeight);

                rt.anchoredPosition = new Vector2(0, j * cellHeight);
                img.sprite = GetRandomSprite();
            }
        }
    }

    public void StartSpinning(SlotResultType finalResult)
    {
        if (IsSpinning) return;
        HideBingoHighlights(); // 스핀 시작할 때 혹시 켜져있을 테두리 초기화
        StartCoroutine(SpinRoutine(finalResult));
    }
    private void PlayResultSound(SlotResultType result)
    {
        if (AudioManager.Instance == null) return;
        if (AudioManager.Instance.enemySounds == null) return;

        AudioClip clip = null;

        switch (result)
        {
            case SlotResultType.OOO:
                clip = AudioManager.Instance.enemySounds.slotSuccess;
                break;

            case SlotResultType.XXX:
                clip = AudioManager.Instance.enemySounds.slotFail;
                break;

            case SlotResultType.STAR:
                clip = AudioManager.Instance.enemySounds.slotSuccess;
                break;

            case SlotResultType.CHERRY:
                clip = AudioManager.Instance.enemySounds.slotHeal;
                break;
        }

        if (clip != null)
        {
            AudioManager.Instance.PlaySFX(clip);
        }
    }
    private IEnumerator SpinRoutine(SlotResultType finalResult)
    {
        IsSpinning = true;

        if (AudioManager.Instance != null &&
        AudioManager.Instance.enemySounds != null &&
        AudioManager.Instance.enemySounds.slotRolling != null)
        {
            AudioManager.Instance.PlaySFX(AudioManager.Instance.enemySounds.slotRolling);
        }

        Sprite targetCenterSprite = finalResult switch
        {
            SlotResultType.OOO => oSprite,
            SlotResultType.XXX => xSprite,
            SlotResultType.STAR => starSprite,
            SlotResultType.CHERRY => cherrySprite,
            _ => xSprite
        };

        Coroutine[] routines = new Coroutine[reelWindows.Length];

        for (int i = 0; i < reelWindows.Length; i++)
        {
            float stopTime = spinDuration + (i * 0.4f);

            Sprite[] targetSprites = new Sprite[3] {
                GetRandomSprite(),
                targetCenterSprite,
                GetRandomSprite()
            };

            routines[i] = StartCoroutine(SpinSingleReel(i, stopTime, targetSprites));
        }

        foreach (var r in routines) yield return r;

        // ==========================================
        // 릴이 모두 멈춘 후 빙고 판정
        // ==========================================

        yield return new WaitForSeconds(0.2f); // 멈춘 후 아주 잠깐 뜸 들이기
        PlayResultSound(finalResult);

        // XXX(실패)가 아닐 때만 빙고 테두리 켜기!
        if (finalResult != SlotResultType.XXX)
        {
            ShowBingoHighlights();

            // (선택사항) 여기서 띠링! 하는 빙고 효과음을 넣으면 완벽합니다.
            // AudioManager.Instance.PlaySFX(AudioManager.Instance.sounds.bingoSuccess);
        }

        yield return new WaitForSeconds(0.5f); // 빙고 확인시켜 줄 시간
        IsSpinning = false; // 이때 턴이 넘어가면서 적이 공격을 시작합니다.
    }

    // ... (이하 SpinSingleReel, SnapImagesToGrid, GetRandomSprite 함수는 기존과 동일하게 유지)
    private IEnumerator SpinSingleReel(int reelIndex, float duration, Sprite[] targetSprites)
    {
        float elapsed = 0f;
        bool stopping = false;
        int targetSetCount = 0;
        Image targetCenterImage = null;

        while (true)
        {
            elapsed += Time.deltaTime;
            float moveStep = spinSpeed * Time.deltaTime;

            for (int j = 0; j < IMAGES_PER_REEL; j++)
                reelImages[reelIndex, j].rectTransform.anchoredPosition += Vector2.down * moveStep;

            for (int j = 0; j < IMAGES_PER_REEL; j++)
            {
                Image img = reelImages[reelIndex, j];
                if (img.rectTransform.anchoredPosition.y <= -cellHeight)
                {
                    img.rectTransform.anchoredPosition += new Vector2(0, IMAGES_PER_REEL * cellHeight);

                    if (stopping && targetSetCount < 3)
                    {
                        img.sprite = targetSprites[targetSetCount];
                        if (targetSetCount == 1) targetCenterImage = img;
                        targetSetCount++;
                    }
                    else
                    {
                        img.sprite = GetRandomSprite();
                    }
                }
            }

            if (elapsed >= duration && !stopping) stopping = true;

            if (stopping && targetSetCount == 3 && targetCenterImage != null)
            {
                if (targetCenterImage.rectTransform.anchoredPosition.y <= cellHeight)
                {
                    SnapImagesToGrid(reelIndex);
                    if (AudioManager.Instance != null &&
                    AudioManager.Instance.enemySounds != null &&
                    AudioManager.Instance.enemySounds.slotStop != null)
                    {
                        AudioManager.Instance.PlaySFX(AudioManager.Instance.enemySounds.slotStop);
                    }
                    break;
                }
            }
            yield return null;
        }
    }

    private void SnapImagesToGrid(int reelIndex)
    {
        Image[] sorted = new Image[IMAGES_PER_REEL];
        for (int j = 0; j < IMAGES_PER_REEL; j++) sorted[j] = reelImages[reelIndex, j];
        System.Array.Sort(sorted, (a, b) => a.rectTransform.anchoredPosition.y.CompareTo(b.rectTransform.anchoredPosition.y));
        for (int j = 0; j < IMAGES_PER_REEL; j++)
        {
            sorted[j].rectTransform.anchoredPosition = new Vector2(0, j * cellHeight);
        }
    }

    private Sprite GetRandomSprite()
    {
        int rand = Random.Range(0, 4);
        return rand switch { 0 => oSprite, 1 => xSprite, 2 => starSprite, _ => cherrySprite };
    }

    private void ShowBingoHighlights()
    {
        if (highlightLeft != null) highlightLeft.SetActive(true);
        if (highlightCenter != null) highlightCenter.SetActive(true);
        if (highlightRight != null) highlightRight.SetActive(true);
    }

    private void HideBingoHighlights()
    {
        if (highlightLeft != null) highlightLeft.SetActive(false);
        if (highlightCenter != null) highlightCenter.SetActive(false);
        if (highlightRight != null) highlightRight.SetActive(false);
    }
}
