using UnityEngine;
using Cinemachine;
using UnityEngine.Events;
using System.Threading;
using System.Collections;

public class NodeSceneCameraController : MonoBehaviour
{
    public static NodeSceneCameraController Instance;

    [Header("Settings")]
    public Animator camAnimator;
    public CinemachineBrain camBrain;
    public CinemachineVirtualCamera dealerCamera;
    public float uiActivateTime = 0.8f;
    [Range(0.5f, 0.95f)] public float yThreshold = 0.9f;
    [Tooltip("0이면 가로 전체, 0.5에 가까울수록 좁아짐")]
    [Range(0f, 0.5f)] public float xThreshold = 0.2f;

    [Header("Events")]
    public UnityEvent<bool> OnDealerViewArrived;

    [Header("Input Delay")]
    [Tooltip("마우스 영역 진입 후 몇 초 뒤 움직일지")]
    public float activationDelay = 0.3f;
    private float mouseEnterTimer = 0;
    private float mouseExitTimer = 0;

    [Header("Dealer Hand Setting")]
    public Transform handTransform;
    public float handZDefault = 0f;
    public float handZDealer = 2f;
    public float handMoveSpeed = 3f;

    [Header("UI Elements")]
    public GameObject upArrowUI;

    private Coroutine handMoveCoroutine;

    private bool isAtDealerView = false;
    private bool isInteractionLocked = false;

    private bool isLookDownLocked = false;
    private bool isLookUpLocked = false; 

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        if (camBrain != null)
        {
            camBrain.m_CameraActivatedEvent.AddListener(OnCameraActivated);
        }

        if (upArrowUI != null) upArrowUI.SetActive(true);
    }

    void OnDestroy()
    {
        if (camBrain != null)
        {
            camBrain.m_CameraActivatedEvent.RemoveListener(OnCameraActivated);
        }
    }

    void Update()
    {
        if(!isInteractionLocked)
            HandleMouseInput();
    }

    void HandleMouseInput()
    {
        float mouseNormalizedX = Input.mousePosition.x / Screen.width;
        float mouseNormalizedY = Input.mousePosition.y / Screen.height;

        bool isMouseInsideHorizontal = (mouseNormalizedX > xThreshold && mouseNormalizedX < (1f - xThreshold));

        if (!isLookUpLocked && mouseNormalizedY > yThreshold && isMouseInsideHorizontal && !isAtDealerView)
        {
            mouseEnterTimer += Time.deltaTime;

            if (mouseEnterTimer >= activationDelay)
            {
                camAnimator.SetBool("IsLookingUp", true);

                // 딜러 손 이동
                MoveHandTo(handZDealer);
            }
        }
        else
        {
            mouseEnterTimer = 0;
        }

        // 2. 아래를 볼 때 (복귀 로직)
        if (!isLookDownLocked && isMouseInsideHorizontal && mouseNormalizedY < xThreshold && isAtDealerView)
        {
            mouseExitTimer += Time.deltaTime; // 하단 영역에 있으면 타이머 증가

            if (mouseExitTimer >= activationDelay)
            {
                camAnimator.SetBool("IsLookingUp", false);
                isAtDealerView = false;
                OnDealerViewArrived.Invoke(false);
                SetMouseInteraction(false);
                // 딜러 손 이동
                MoveHandTo(handZDefault);
                mouseExitTimer = 0; // 실행 후 리셋

                if (upArrowUI != null) upArrowUI.SetActive(true);
            }
        }
        else
        {
            mouseExitTimer = 0; // 하단 영역을 벗어나면 타이머 리셋
        }
    }

    void MoveHandTo(float handZ)
    {
        if (handTransform == null) return;

        if (handMoveCoroutine != null)
            StopCoroutine(handMoveCoroutine);

        handMoveCoroutine = StartCoroutine(AnimateHandZ(handZ));

    }

    IEnumerator AnimateHandZ(float handZ)
    {
        Vector3 pos = handTransform.localPosition;

        while(Mathf.Abs(handTransform.localPosition.z - handZ) > 0.01f)
        {
            pos.z = Mathf.Lerp(handTransform.localPosition.z, handZ, Time.deltaTime * handMoveSpeed);
            handTransform.position = pos;
            yield return null;
        }
        pos.z = handZ;
        handTransform.localPosition = pos;

        handMoveCoroutine = null;
    }

    void OnCameraActivated(ICinemachineCamera newCamera, ICinemachineCamera previousCamera)
    {
        if (newCamera.Name == dealerCamera.name)
        {
            StopAllCoroutines();
            StartCoroutine(WaitForArrival());
        }
    }

    System.Collections.IEnumerator WaitForArrival()
    {
        while (camBrain.IsBlending)
        {
            yield return null;
        }

        yield return new WaitForSeconds(uiActivateTime);

        isAtDealerView = true;
        OnDealerViewArrived.Invoke(true);        
        SetMouseInteraction(true);

        if (upArrowUI != null) upArrowUI.SetActive(false);
    }

    void SetMouseInteraction(bool isUIOpen)
    {
        if (isUIOpen)
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
        else
        {
            if (isInteractionLocked)
                return;

            Cursor.lockState = CursorLockMode.Confined;
        }
    }

    public void SetInteractionLock(bool lockState)
    {
        isInteractionLocked = lockState;
    }

    //튜토용 외부에서 내리기 잠금을 켜고 끄는 함수
    public void SetLookDownLock(bool lockState)
    {
        isLookDownLocked = lockState;
    }
    public void SetLookUpLock(bool lockState)
    {
        isLookUpLocked = lockState;
    }
}
