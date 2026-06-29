using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CartoonTransitionManager : MonoBehaviour
{
    public static CartoonTransitionManager Instance;

    [Header("UI 연결")]
    public GameObject transitionPanel;
    public RectTransform maskTransform;
    public Animator maskAnimator;

    [Header("세팅")]
    [Tooltip("원이 완전히 가려지는 데 걸리는 시간")]
    public float animationDuration = 0.5f;
    [Tooltip("뒤편에서 카메라가 이동을 완료할 때까지 깜깜하게 대기하는 시간")]
    public float cameraDelay = 0.4f;

    private GameObject canvasObject;
    private RectTransform canvasRectTransform;

    // 현재 씬 전환 연출이 진행 중인지 체크하는 안전 플래그
    private bool isHandlingSceneOut = false;

    private void Awake()
    {
        if (Instance == null) { Instance = this; DontDestroyOnLoad(gameObject); }
        else { Destroy(gameObject); return; }
        if (transitionPanel != null) transitionPanel.SetActive(false);
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // 캐싱이 정상 완료되었다면 이제 여기서 절대 return되지 않습니다.
        if (this == null || maskTransform == null || transitionPanel == null || canvasObject == null) return;

        if (isHandlingSceneOut)
        {
            isHandlingSceneOut = false; // 플래그 리셋

            // 1. 강제로 캔버스 온오프 전체 활성화!
            SetTransitionActive(true);

            // 2. 좌표 및 앵커 정중앙 고정
            if (canvasRectTransform != null) canvasRectTransform.anchoredPosition = Vector2.zero;
            maskTransform.anchoredPosition = Vector2.zero;

            // 3. 리얼타임 애니메이션 강제 재생
            if (maskAnimator != null)
            {
                maskAnimator.enabled = true;
                maskAnimator.updateMode = AnimatorUpdateMode.UnscaledTime;
                maskAnimator.Play("FadeOut", 0, 0f);
            }

            // 4. StopAllCoroutines에 영향받지 않게 안전하게 이름 기반으로 제어
            StopCoroutine(nameof(DisablePanelDelayed));
            StartCoroutine(nameof(DisablePanelDelayed));
        }
    }

    private IEnumerator DisablePanelDelayed()
    {
        yield return new WaitForSecondsRealtime(animationDuration);
        if (transitionPanel != null) transitionPanel.SetActive(false);
    }

    public void PlayTransitionAtMouseWithoutAction()
    {
        StopAllCoroutines();
        StartCoroutine(AnimateUiFadeCoroutine(Input.mousePosition, null));
    }

    public void PlayTransitionAtPosition(Vector2 screenPos, Action onCompleteAction)
    {
        StopAllCoroutines();
        StartCoroutine(AnimateUiFadeCoroutine(screenPos, onCompleteAction));
    }

    public IEnumerator AnimateUiFadeCoroutine(Vector2 targetScreenPos, Action onCompleteAction)
    {
        if (maskAnimator != null)
        {
            maskAnimator.enabled = true;
            maskAnimator.updateMode = AnimatorUpdateMode.UnscaledTime;
        }
        if (transitionPanel != null) transitionPanel.SetActive(true);

        maskTransform.position = targetScreenPos;
        maskAnimator.SetTrigger("PlayTransition");

        yield return new WaitForSecondsRealtime(animationDuration);

        if (onCompleteAction != null)
        {
            onCompleteAction.Invoke();
        }

        yield return new WaitForSecondsRealtime(0.05f);

        maskTransform.anchoredPosition = Vector2.zero;

        yield return new WaitForSecondsRealtime(cameraDelay);

        SetTransitionActive(false);
    }

    private void SetTransitionActive(bool isActive)
    {
        if (transitionPanel != null) transitionPanel.SetActive(isActive);
        if (canvasObject != null) canvasObject.SetActive(isActive);
        if (!isActive && maskAnimator != null) maskAnimator.enabled = false;
    }

}
