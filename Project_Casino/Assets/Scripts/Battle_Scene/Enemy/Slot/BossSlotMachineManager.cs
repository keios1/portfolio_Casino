using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public enum BossSlotSymbol { TRIANGLE, SQUARE, CIRCLE }

public class BossSlotMachineManager : MonoBehaviour
{
    [Header("Boss UI Panel")]
    public GameObject bossSlotPanel;

    [Header("Reel Windows (3x3)")]
    public RectTransform[] reelWindows;

    [Header("Resources (기본 심볼)")]
    public Sprite triangleSprite;
    public Sprite squareSprite;
    public Sprite circleSprite;

    [Header("Resources (특수 패턴 심볼)")]
    public Sprite jackpotSprite; // 잭팟용 이미지
    public Sprite cherrySprite;  // 수리용 이미지 (체리)

    // ==========================================
    //  9개의 개별 칸 하이라이트 제어
    // ==========================================
    [Header("Bingo Cell Highlights (사진의 1~9번 칸 순서)")]
    public GameObject[] cellHighlights = new GameObject[9];
    // 인덱스 0~8에 각각 1번~9번 테두리를 연결합니다.

    [Header("Settings")]
    public float spinDuration = 2.0f;
    public float spinSpeed = 2000f;

    [Header("UI Settings")]
    public float symbolSpacing = 20f;

    [Header("Desperation Phase (발악 패턴용)")]
    public float desperationSpinSpeed = 1500f;
    public Button stopButton;
    private int manualStopPressCount = 0;
    private Sprite[] manualResults = new Sprite[3];

    public Image desperationDesignOverlay;
    public bool IsSpinning { get; private set; } = false;

    private Image[,] reelImages;
    private float symbolHeight;
    private float cellHeight;
    private const int VISIBLE_SLOTS = 3;
    private const int IMAGES_PER_REEL = 4;

    private void Awake()
    {
        CloseUI();

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

    public void ShowUI() { bossSlotPanel.SetActive(true); }

    public void CloseUI()
    {
        bossSlotPanel.SetActive(false);
        HideAllCellHighlights(); // UI 닫을 때 테두리도 끔
    }

    // 기본 랜덤 회전
    public IEnumerator SpinRoutine(System.Action<int> onSpinFinished)
    {
        IsSpinning = true;
        HideAllCellHighlights();

        if (AudioManager.Instance != null &&
        AudioManager.Instance.enemySounds != null &&
        AudioManager.Instance.enemySounds.slotRolling != null)
        {
            AudioManager.Instance.PlaySFX(AudioManager.Instance.enemySounds.slotRolling);
        }

        Coroutine[] routines = new Coroutine[reelWindows.Length];

        Sprite[,] resultGrid = new Sprite[3, 3];
        for (int x = 0; x < 3; x++)
            for (int y = 0; y < 3; y++)
                resultGrid[x, y] = GetRandomSprite();

        for (int i = 0; i < reelWindows.Length; i++)
        {
            Sprite[] reelTargets = new Sprite[3] { resultGrid[i, 0], resultGrid[i, 1], resultGrid[i, 2] };
            routines[i] = StartCoroutine(SpinSingleReel(i, spinDuration + (i * 0.4f), reelTargets));
        }

        foreach (var r in routines) yield return r;
        yield return new WaitForSeconds(0.2f);

        // 멈춘 후 빙고 데미지 계산과 동시에 겹치는 테두리 계산해서 켜기!
        int damage = CalculateBingoDamage(resultGrid);
        PlayBossSlotResultSound(damage);

        yield return new WaitForSeconds(0.8f);

        IsSpinning = false;
        onSpinFinished?.Invoke(damage);
    }

    // 강제 회전 (패턴 C, E)
    public IEnumerator ForceSpinRoutine(Sprite forcedSprite, AudioClip resultSound = null, System.Action onSpinFinished = null)
    {
        IsSpinning = true;
        HideAllCellHighlights();

        if (AudioManager.Instance != null &&
        AudioManager.Instance.enemySounds != null &&
        AudioManager.Instance.enemySounds.slotRolling != null)
        {
            AudioManager.Instance.PlaySFX(AudioManager.Instance.enemySounds.slotRolling);
        }

        Coroutine[] routines = new Coroutine[reelWindows.Length];

        Sprite[,] resultGrid = new Sprite[3, 3];
        for (int x = 0; x < 3; x++)
            for (int y = 0; y < 3; y++)
                resultGrid[x, y] = forcedSprite;

        for (int i = 0; i < reelWindows.Length; i++)
        {
            Sprite[] reelTargets = new Sprite[3] { resultGrid[i, 0], resultGrid[i, 1], resultGrid[i, 2] };
            routines[i] = StartCoroutine(SpinSingleReel(i, spinDuration + (i * 0.4f), reelTargets));
        }

        foreach (var r in routines) yield return r;
        yield return new WaitForSeconds(0.2f);

        if (resultSound != null && AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX(resultSound);
        }
        CalculateBingoDamage(resultGrid); // 9칸 전부 켜짐

        yield return new WaitForSeconds(1.0f);

        IsSpinning = false;
        onSpinFinished?.Invoke();
    }

    private IEnumerator SpinSingleReel(int reelIndex, float duration, Sprite[] targetSprites)
    {
        float elapsed = 0f;
        bool stopping = false;
        int targetSetCount = 0;
        Image targetTopImage = null;

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
                        if (targetSetCount == 2) targetTopImage = img;
                        targetSetCount++;
                    }
                    else img.sprite = GetRandomSprite();
                }
            }

            if (elapsed >= duration && !stopping) stopping = true;

            if (stopping && targetSetCount == 3 && targetTopImage != null)
            {
                if (targetTopImage.rectTransform.anchoredPosition.y <= cellHeight * 2)
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

    public IEnumerator DesperationSpinRoutine(System.Action<bool> onResult)
    {
        IsSpinning = true;
        HideAllCellHighlights();
        manualStopPressCount = 0;

        if (desperationDesignOverlay != null)
        {
            desperationDesignOverlay.gameObject.SetActive(true);
        }

        stopButton.gameObject.SetActive(true);
        stopButton.onClick.RemoveAllListeners();
        stopButton.onClick.AddListener(() => manualStopPressCount++);

        Coroutine[] routines = new Coroutine[reelWindows.Length];
        bool[] stopSignals = new bool[reelWindows.Length];

        for (int i = 0; i < reelWindows.Length; i++)
        {
            int index = i;
            routines[i] = StartCoroutine(ManualSpinSingleReel(i, () => stopSignals[index]));
        }

        for (int i = 0; i < reelWindows.Length; i++)
        {
            yield return new WaitUntil(() => manualStopPressCount > i);
            stopSignals[i] = true;
            yield return routines[i];
        }

        stopButton.gameObject.SetActive(false);
        yield return new WaitForSeconds(0.5f);

        bool isSuccess = (manualResults[0] == manualResults[1] && manualResults[1] == manualResults[2]);
        if (AudioManager.Instance != null &&
        AudioManager.Instance.enemySounds != null)
        {
            AudioClip clip = isSuccess
                ? AudioManager.Instance.enemySounds.slotSuccess
                : AudioManager.Instance.enemySounds.slotFail;

            if (clip != null)
                AudioManager.Instance.PlaySFX(clip);
        }
        // 발악 패턴에서 성공 시 4번, 5번, 6번 칸 켜기
        if (isSuccess)
        {
            if (cellHighlights.Length > 3 && cellHighlights[3] != null) cellHighlights[3].SetActive(true); // 4번
            if (cellHighlights.Length > 4 && cellHighlights[4] != null) cellHighlights[4].SetActive(true); // 5번
            if (cellHighlights.Length > 5 && cellHighlights[5] != null) cellHighlights[5].SetActive(true); // 6번
        }

        yield return new WaitForSeconds(0.8f);

        IsSpinning = false;
        onResult?.Invoke(isSuccess);
    }

    private IEnumerator ManualSpinSingleReel(int reelIndex, System.Func<bool> shouldStop)
    {
        bool stopping = false;
        while (true)
        {
            float moveStep = desperationSpinSpeed * Time.deltaTime;

            for (int j = 0; j < IMAGES_PER_REEL; j++)
                reelImages[reelIndex, j].rectTransform.anchoredPosition += Vector2.down * moveStep;

            for (int j = 0; j < IMAGES_PER_REEL; j++)
            {
                Image img = reelImages[reelIndex, j];
                if (img.rectTransform.anchoredPosition.y <= -cellHeight)
                {
                    img.rectTransform.anchoredPosition += new Vector2(0, IMAGES_PER_REEL * cellHeight);
                    img.sprite = GetRandomSprite();
                }
            }

            if (!stopping && shouldStop()) stopping = true;

            if (stopping)
            {
                bool isCenterAligned = false;
                for (int j = 0; j < IMAGES_PER_REEL; j++)
                {
                    float yPos = reelImages[reelIndex, j].rectTransform.anchoredPosition.y;
                    if (yPos <= cellHeight && yPos > cellHeight - moveStep)
                    {
                        isCenterAligned = true;
                        if (AudioManager.Instance != null &&
                        AudioManager.Instance.enemySounds != null &&
                        AudioManager.Instance.enemySounds.slotStop != null)
                        {
                            AudioManager.Instance.PlaySFX(AudioManager.Instance.enemySounds.slotStop);
                        }
                        break;
                    }
                }

                if (isCenterAligned)
                {
                    Image[] sorted = SnapImagesToGrid(reelIndex);
                    manualResults[reelIndex] = sorted[1].sprite;
                    break;
                }
            }
            yield return null;
        }
    }

    // ==========================================
    // 칸 단위 빙고 판정 로직
    // ==========================================
    private int CalculateBingoDamage(Sprite[,] grid)
    {
        int bingoCount = 0;
        bool[] highlightMap = new bool[9]; // 중복을 방지하기 위한 체크 배열

        // 1. 가로 3줄 (y: 0=맨아래(1,2,3), 1=중간(4,5,6), 2=맨위(7,8,9))
        for (int y = 0; y < 3; y++)
        {
            if (grid[0, y] == grid[1, y] && grid[1, y] == grid[2, y])
            {
                bingoCount++;
                highlightMap[y * 3 + 0] = true;
                highlightMap[y * 3 + 1] = true;
                highlightMap[y * 3 + 2] = true;
            }
        }

        // 2. 세로 3줄 (x: 0=왼쪽(1,4,7), 1=중간(2,5,8), 2=오른쪽(3,6,9))
        for (int x = 0; x < 3; x++)
        {
            if (grid[x, 0] == grid[x, 1] && grid[x, 1] == grid[x, 2])
            {
                bingoCount++;
                highlightMap[0 * 3 + x] = true;
                highlightMap[1 * 3 + x] = true;
                highlightMap[2 * 3 + x] = true;
            }
        }

        // 3. 대각선 1 (좌하 -> 우상 : 1번, 5번, 9번)
        if (grid[0, 0] == grid[1, 1] && grid[1, 1] == grid[2, 2])
        {
            bingoCount++;
            highlightMap[0] = true; // 1번
            highlightMap[4] = true; // 5번
            highlightMap[8] = true; // 9번
        }

        // 4. 대각선 2 (좌상 -> 우하 : 7번, 5번, 3번)
        if (grid[0, 2] == grid[1, 1] && grid[1, 1] == grid[2, 0])
        {
            bingoCount++;
            highlightMap[6] = true; // 7번
            highlightMap[4] = true; // 5번
            highlightMap[2] = true; // 3번
        }

        // 체크된 모든 칸의 스프라이트를 한 번에 켭니다. (겹침 방지)
        for (int i = 0; i < 9; i++)
        {
            if (highlightMap[i] && cellHighlights.Length > i && cellHighlights[i] != null)
            {
                cellHighlights[i].SetActive(true);
            }
        }

        return bingoCount * 10;
    }

    private void HideAllCellHighlights()
    {
        if (cellHighlights == null) return;
        foreach (var cell in cellHighlights)
        {
            if (cell != null) cell.SetActive(false);
        }
    }

    private Image[] SnapImagesToGrid(int reelIndex)
    {
        Image[] sorted = new Image[IMAGES_PER_REEL];
        for (int j = 0; j < IMAGES_PER_REEL; j++) sorted[j] = reelImages[reelIndex, j];

        System.Array.Sort(sorted, (a, b) => a.rectTransform.anchoredPosition.y.CompareTo(b.rectTransform.anchoredPosition.y));

        for (int j = 0; j < IMAGES_PER_REEL; j++)
        {
            sorted[j].rectTransform.anchoredPosition = new Vector2(0, j * cellHeight);
        }
        return sorted;
    }

    private Sprite GetRandomSprite()
    {
        int rand = Random.Range(0, 3);
        return rand switch { 0 => triangleSprite, 1 => squareSprite, _ => circleSprite };
    }

    private void PlayBossSlotResultSound(int damage)
    {
        if (AudioManager.Instance == null) return;
        if (AudioManager.Instance.enemySounds == null) return;

        AudioClip clip = null;

        if (damage > 0)
            clip = AudioManager.Instance.enemySounds.slotSuccess;
        else
            clip = AudioManager.Instance.enemySounds.slotFail;

        if (clip != null)
            AudioManager.Instance.PlaySFX(clip);
    }
}
