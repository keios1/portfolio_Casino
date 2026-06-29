using UnityEngine;
using UnityEngine.UI;

public class DiceUI : MonoBehaviour
{
    public static DiceUI Instance;

    [Header("Background Slots (size 6)")]
    [SerializeField] private Image[] slotBackgrounds = new Image[6];

    // 쉐이더 마테리얼이 들어간 빛나는 테두리 프레임 이미지
    [Header("Glow Outline Frames (size 6)")]
    [SerializeField] private Image[] glowOutlines = new Image[6];

    [Header("Face Images (size 6) - overlay images")]
    [SerializeField] private Image[] diceFaces = new Image[6];

    [Header("Slot Buttons (size 6)")]
    [SerializeField] private Button[] slotButtons = new Button[6];

    [Header("Sprites: 0=empty, 1~6=face")]
    [SerializeField] private Sprite[] diceSprites = new Sprite[7];

    [Header("박스 배경 이미지")]
    [SerializeField] private Sprite emptyBoxSprite;  // 노란색 박스
    [SerializeField] private Sprite filledBoxSprite; // 빨간색 박스

    [Header("Optional")]
    [SerializeField] private CardManager cardManager;

    public bool DEBUG_dicebutton = true;

    private void Awake()
    {
        Instance = this;

        if (cardManager == null)
            cardManager = FindObjectOfType<CardManager>();

        BindButtons();
        ClearAllFaces();
        InitializeMaterials(); // 아웃라인 프레임의 쉐이더 초기화
    }

    private void InitializeMaterials()
    {
        for (int i = 0; i < glowOutlines.Length; i++)
        {
            if (glowOutlines[i] != null && glowOutlines[i].material != null)
            {
                // 각 프레임이 개별적으로 쉐이더 깜빡임을 쓸 수 있도록 마테리얼 복제
                glowOutlines[i].material = new Material(glowOutlines[i].material);

                // 처음엔 쉐이더의 스위치를 끄고(_Hover = 0), 이미지 자체도 안 보이게 꺼둡니다.
                glowOutlines[i].material.SetFloat("_Hover", 0f);
                glowOutlines[i].enabled = false;
            }
        }
    }

    private void BindButtons()
    {
        if (slotButtons == null) return;

        for (int i = 0; i < slotButtons.Length; i++)
        {
            int index = i;
            if (slotButtons[index] == null) continue;

            slotButtons[index].onClick.RemoveAllListeners();
            slotButtons[index].onClick.AddListener(() =>
            {
                if (DEBUG_dicebutton) Debug.Log($"Dice Slot {index} clicked");

                if (cardManager == null) cardManager = FindObjectOfType<CardManager>();
                if (cardManager == null) return;

                cardManager.OnClickDiceSlot(index);
            });
        }
    }

    private void ClearAllFaces()
    {
        if (diceFaces == null) return;

        for (int i = 0; i < diceFaces.Length; i++)
        {
            if (diceFaces[i] == null) continue;
            diceFaces[i].sprite = null;
            diceFaces[i].enabled = false;

            if (slotBackgrounds[i] != null && emptyBoxSprite != null)
                slotBackgrounds[i].sprite = emptyBoxSprite;
        }
    }

    public void SetSlot(int index, int value)
    {
        if (diceFaces == null || index < 0 || index >= diceFaces.Length || diceFaces[index] == null)
            return;

        int v = Mathf.Clamp(value, 0, 10);

        if (v == 0)
        {
            diceFaces[index].sprite = null;
            diceFaces[index].enabled = false;

            if (slotBackgrounds[index] != null && emptyBoxSprite != null)
                slotBackgrounds[index].sprite = emptyBoxSprite;

            // 추가: 빈 칸이면 빛나는 테두리도 반드시 끔
            SetSlotBlink(index, false);

            return;
        }

        if (diceSprites != null && diceSprites.Length > v)
        {
            Sprite sp = diceSprites[v];
            diceFaces[index].sprite = sp;
            diceFaces[index].enabled = (sp != null);

            if (slotBackgrounds[index] != null && filledBoxSprite != null)
                slotBackgrounds[index].sprite = filledBoxSprite;
        }
    }

    public void SetAll(int[] diceSlots)
    {
        if (diceFaces == null) return;

        for (int i = 0; i < diceFaces.Length; i++)
        {
            int v = (diceSlots != null && i < diceSlots.Length) ? diceSlots[i] : 0;
            SetSlot(i, v);
        }
    }

    // ==========================================
    // 아웃라인 프레임 켜기/끄기 + 쉐이더 작동 제어
    // ==========================================

    public void SetSlotBlink(int index, bool isBlinking)
    {
        if (index >= 0 && index < glowOutlines.Length && glowOutlines[index] != null)
        {
            glowOutlines[index].enabled = isBlinking;
            glowOutlines[index].material.SetFloat("_Hover", isBlinking ? 1f : 0f);
        }
    }

    public void StopAllBlinks()
    {
        for (int i = 0; i < glowOutlines.Length; i++)
        {
            SetSlotBlink(i, false);
        }
    }

    // ==========================================
    // 키보드 단축키
    private void OnEnable()
    {
        if (GameInputManager.Instance != null)
            GameInputManager.Instance.OnDiceSlotPressed += HandleDiceSlotShortcut;
    }

    private void OnDisable()
    {
        if (GameInputManager.Instance != null)
            GameInputManager.Instance.OnDiceSlotPressed -= HandleDiceSlotShortcut;
    }

    private void HandleDiceSlotShortcut(int index)
    {
        if (cardManager == null)
            cardManager = FindObjectOfType<CardManager>();

        if (cardManager == null)
            return;

        cardManager.OnClickDiceSlot(index);
    }
}
