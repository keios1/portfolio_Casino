using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class DiceSpawnPreviewUI : MonoBehaviour
{
    [Header("Slot Preview Images")]
    [SerializeField] private Image[] previewImages = new Image[6];

    [Header("1번 애니메이션")]
    [SerializeField] private Sprite[] startFrames;
    [SerializeField] private float startFrameTime = 0.1f;

    [Header("2번 반복 애니메이션")]
    [SerializeField] private Sprite[] loopFrames;
    [SerializeField] private float loopFrameTime = 0.1f;

    [Header("3번 생성 애니메이션")]
    [SerializeField] private Sprite[] endFrames;
    [SerializeField] private float endFrameTime = 0.06f;

    private Coroutine[] runningCoroutines;

    private void Awake()
    {
        runningCoroutines = new Coroutine[previewImages.Length];
        HideAll();
    }

    public void HideAll()
    {
        for (int i = 0; i < previewImages.Length; i++)
            Hide(i);
    }

    public void Hide(int index)
    {
        if (!IsValid(index)) return;

        if (runningCoroutines[index] != null)
        {
            StopCoroutine(runningCoroutines[index]);
            runningCoroutines[index] = null;
        }

        previewImages[index].enabled = false;
        previewImages[index].sprite = null;
    }

    public void StartPreview(int index)
    {
        if (!IsValid(index)) return;

        Hide(index);
        runningCoroutines[index] = StartCoroutine(StartThenLoopRoutine(index));
    }

    public IEnumerator PlayStartThenLoop(int index)
    {
        StartPreview(index);
        yield break;
    }

    public IEnumerator PlayEndAndHide(int index)
    {
        if (!IsValid(index)) yield break;

        Hide(index);

        Image img = previewImages[index];
        img.enabled = true;

        yield return PlayFrames(img, endFrames, endFrameTime);

        Hide(index);
    }

    private IEnumerator StartThenLoopRoutine(int index)
    {
        Image img = previewImages[index];
        img.enabled = true;

        yield return PlayFrames(img, startFrames, startFrameTime);

        while (true)
            yield return PlayFrames(img, loopFrames, loopFrameTime);
    }

    private IEnumerator PlayFrames(Image img, Sprite[] frames, float frameTime)
    {
        if (img == null || frames == null || frames.Length == 0)
            yield break;

        for (int i = 0; i < frames.Length; i++)
        {
            img.sprite = frames[i];
            yield return new WaitForSeconds(frameTime);
        }
    }

    private bool IsValid(int index)
    {
        return previewImages != null
            && index >= 0
            && index < previewImages.Length
            && previewImages[index] != null;
    }
}
