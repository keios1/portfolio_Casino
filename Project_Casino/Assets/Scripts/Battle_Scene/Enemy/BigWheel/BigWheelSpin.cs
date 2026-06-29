using System.Collections;
using System.Collections.Generic;
using System.Data;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class BigWheelSpin : MonoBehaviour
{
    // NESTED CLASS
    [System.Serializable]
    public class BigWheelState
    {
        [SerializeField] public int[] slotValue = new int[] { 10, 10, 10, 20, 5, 5 };
    }

    public bool IsSpinning { get; private set; } = false;
    public bool IsChanged { get; private set; } = false;

    [Header("UI Panel")]
    public GameObject spinObject;

    //public Transform singlePosition;
    //public Transform doublePosition;

    [Header("TXT")]
    public TextMeshProUGUI[] textList;

    public int slot = 6;
    public float spinTime = 1f;
    [SerializeField] public BigWheelState normalState;
    [SerializeField] public BigWheelState changedState;

    private float angleOffset;
    private float angleDivide;
    private BigWheelState currentWheelState;
    


    public int[] slotValue = new int[] { 10, 10, 10, 20, 5, 5 };
    
    private RectTransform spinObjectRect;
    

    public int GetSpinIndex()
    {
        return (int)(GetZValue(spinObjectRect.rotation.eulerAngles.z + angleOffset) / angleDivide);
    }
    public int GetDamage()
    {
        return currentWheelState.slotValue[GetSpinIndex()];
    }

    private void Awake()
    {
        if (currentWheelState == null)
        {
            Debug.Log($"큰 스핀 변경됨");
            currentWheelState = normalState;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        spinObjectRect = spinObject.GetComponent<RectTransform>();
        angleOffset = 360f / ((float)slot * 2);
        angleDivide = 360f / slot;

         

        //gameObject.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            StartCoroutine(SpinRandomCoroutine());
        }
        if (Input.GetKeyDown(KeyCode.T))
        {
            StartCoroutine(SpinTargetCoroutine(3));
        }
    }

    public float GetZValue(float z)
    {
        while (z > 360f)
        {
            z -= 360f;
        }
        return z;
    }

    // spinTime초 동안 무작위한 각도로 이동 
    public IEnumerator SpinRandomCoroutine()
    {
        float beginTime = Time.time;
        float EndTime = Time.time + spinTime;
        float beginAngle = spinObjectRect.eulerAngles.z;
        float endAngle = spinObjectRect.eulerAngles.z + Random.Range(360f * 2, 360f * 5);

        IsSpinning = true;
        PlayWheelRollingSound();
        while (Time.time < EndTime)
        {
            Vector3 _angleValue = spinObjectRect.eulerAngles;
            _angleValue.z = Mathf.Lerp(beginAngle, endAngle, (Time.time - beginTime) / spinTime);
            spinObjectRect.eulerAngles = _angleValue;
            yield return null;
        }
        PlayWheelStopSound();
        IsSpinning = false;

        Vector3 _angleValueEnd = spinObjectRect.eulerAngles;
        _angleValueEnd.z = GetZValue(_angleValueEnd.z);
        spinObjectRect.eulerAngles = _angleValueEnd;

        Debug.Log($">> 인덱스 = {GetSpinIndex()}");
    }

    public IEnumerator SpinTargetCoroutine(int index)
    {
        float beginTime = Time.time;
        float EndTime = Time.time + spinTime;
        float beginAngle = spinObjectRect.eulerAngles.z;
        //float endAngle = angleDivide * index + angleOffset + Random.Range(2, 5) * 360f;
        float endAngle = angleDivide * index + Random.Range(2, 5) * 360f;

        Debug.Log($">> SpinTargetCoroutine({index}) / beginAngle{beginAngle} / endAngle{endAngle}");

        IsSpinning = true;
        PlayWheelRollingSound();
        while (Time.time < EndTime)
        {
            Vector3 _angleValue = spinObjectRect.eulerAngles;
            _angleValue.z = Mathf.Lerp(beginAngle, endAngle, (Time.time - beginTime) / spinTime);
            spinObjectRect.eulerAngles = _angleValue;
            yield return null;
        }
        Vector3 _endAngleValue = spinObjectRect.eulerAngles;
        _endAngleValue.z = endAngle;
        spinObjectRect.eulerAngles = _endAngleValue;
        PlayWheelStopSound();
        IsSpinning = false;


        Vector3 _angleValueEnd = spinObjectRect.eulerAngles;
        _angleValueEnd.z = GetZValue(_angleValueEnd.z);
        spinObjectRect.eulerAngles = _angleValueEnd;
        Debug.Log($">> 인덱스 = {GetSpinIndex()}");
    }

    public void ShowUI()
    {
        gameObject.SetActive(true);
    }

    public void CloseUI()
    {
        gameObject.SetActive(false);
    }

    public void Change()
    {
        //currentWheelState.image = changedState.image;
        //currentWheelState.slotValue = changedState.slotValue;

        currentWheelState = changedState;


        textList[0].text = currentWheelState.slotValue[0].ToString();
        textList[1].text = currentWheelState.slotValue[1].ToString();
        textList[2].text = currentWheelState.slotValue[2].ToString();
        textList[3].text = currentWheelState.slotValue[3].ToString();
        textList[4].text = currentWheelState.slotValue[4].ToString();
        textList[5].text = currentWheelState.slotValue[5].ToString();

        IsChanged = true;
    }

    // 빅휠 2번째 에너미
    public void ChangeSecondPatternAC()
    {
        //transform.position = singlePosition.transform.position;

        currentWheelState = new BigWheelState()
        {
            //image = currentWheelState.image,
            slotValue = new int[] { 10, 10, 10, 20, 5, 5 }
        };
        SlotRefresh();
    }
    public void ChangeSecondPatternB()
    {
        //transform.position = singlePosition.transform.position;

        currentWheelState = new BigWheelState()
        {
            //image = currentWheelState.image,
            slotValue = new int[] { 5, 10, 15, 5, 10, 15 }
        };
        SlotRefresh();
    }
    public void ChangeSecondPatternE()
    {
        //transform.position = doublePosition.transform.position;

        currentWheelState = new BigWheelState()
        {
            //image = currentWheelState.image,
            slotValue = new int[] { 10, 15, 20, 10, 15, 20 }
        };
        SlotRefresh();
    }
    public void SlotRefresh()
    {
        for (int index = 0; index < textList.Length; index++)
        {
            textList[index].text = currentWheelState.slotValue[index].ToString();
        }
    }

    public void ChangeThirdPatternAB()
    {
        //transform.position = singlePosition.transform.position;

        currentWheelState = new BigWheelState()
        {
            slotValue = new int[] { 15, 10, 10, 10, 5, 5 }
        };
        SlotRefresh();
    }

    public void ChangeThirdPatternC()
    {
        //transform.position = singlePosition.transform.position;

        currentWheelState = new BigWheelState()
        {
            slotValue = new int[] { 15, 10, 5, 5, 0, 0 }
        };
        SlotRefresh();
    }
    private void PlayWheelRollingSound()
    {
        if (AudioManager.Instance == null) return;
        if (AudioManager.Instance.enemySounds == null) return;

        AudioManager.Instance.PlaySFX(
            AudioManager.Instance.enemySounds.wheelRolling
        );
    }
    private void PlayWheelStopSound()
    {
        if (AudioManager.Instance == null) return;
        if (AudioManager.Instance.enemySounds == null) return;

        AudioManager.Instance.PlaySFX(
            AudioManager.Instance.enemySounds.wheelStop
        );
    }
    private void OnEnable()
    {
        Debug.Log($">> {gameObject.name}에서 Spin OnEnable");
    }
    private void OnDisable()
    {
        Debug.Log($">> {gameObject.name}에서 Spin OnDisable");
    }
}
