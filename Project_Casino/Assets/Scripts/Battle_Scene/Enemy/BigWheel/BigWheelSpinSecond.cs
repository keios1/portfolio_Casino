using System.Collections;
using System.Collections.Generic;
using System.Data;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class BigWheelSpinSecond : MonoBehaviour
{
    // NESTED CLASS
    [System.Serializable]
    public class BigWheelState
    {
        [SerializeField] public Sprite image;
        [SerializeField] public int[] slotValue = new int[] { 1, 2, 0, 1, 2, 0 };
    }

    public bool IsSpinning { get; private set; } = false;
    public bool IsChanged { get; private set; } = false;

    [Header("UI Panel")]
    public GameObject spinObject;

    [Header("TXT")]
    public TextMeshProUGUI[] textList;

    public int slot = 6;
    public float spinTime = 1f;
    [SerializeField] public BigWheelState normalState;

    private float angleOffset;
    private float angleDivide;
    private BigWheelState currentWheelState;



    public int[] slotValue = new int[] { 1, 2, 0, 1, 2, 0 };

    private RectTransform spinObjectRect;


    public int GetSpinIndex()
    {
        return (int)(GetZValue(spinObjectRect.rotation.eulerAngles.z + angleOffset) / angleDivide);
    }
    public int GetDamage()
    {
        return currentWheelState.slotValue[GetSpinIndex()];
    }

    // Start is called before the first frame update
    void Start()
    {
        spinObjectRect = spinObject.GetComponent<RectTransform>();
        angleOffset = 360f / ((float)slot * 2);
        angleDivide = 360f / slot;
        currentWheelState = normalState;

        SlotRefresh();

        
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
        spinObject.SetActive(true);
    }

    public void CloseUI()
    {
        spinObject.SetActive(false);
    }

    public void SlotRefresh()
    {
        for (int index = 0; index < textList.Length; index++)
        {
            textList[index].text = currentWheelState.slotValue[index].ToString();
        }
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
