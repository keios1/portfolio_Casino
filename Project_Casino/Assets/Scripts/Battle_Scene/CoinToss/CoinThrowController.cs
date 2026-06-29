using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(Rigidbody))]
public class CoinThrowController : MonoBehaviour
{
    [Header("Input")]
    public Camera inputCamera;
    public bool lockMoveToWorldZ = true;
    public bool throwForwardOnly = true;

    [Header("Drag")]
    public float minDragPixelsToThrow;
    public float maxDragPixels;
    public float dragPreviewDistance;

    [Header("Toss Motion")]
    public float minTravelDistance;
    public float maxTravelDistance;
    public float minArcHeight;
    public float maxArcHeight;
    public float minDuration;
    public float maxDuration;

    [Header("Spin")]
    public int minHalfSpins;
    public int maxHalfSpins;
    public float settleRotateTime;

    [Header("Landing")]
    public Transform forwardRef;
    public Transform floorRef;
    public float floorOffset = 0.02f;

    public bool IsThrown { get; private set; }
    public bool IsSettled { get; private set; }
    public bool IsDragging { get; private set; }

    public CoinTossManager.CoinFace CurrentFace { get; private set; } = CoinTossManager.CoinFace.Head;

    private CoinTossManager.CoinFace pendingFace = CoinTossManager.CoinFace.Head;

    private Rigidbody rb;
    private Collider col;

    private Plane dragPlane;
    private Vector3 grabOffsetWS;
    private Vector3 dragStartWorldPos;
    private Vector2 dragStartScreen;

    private Vector3 resetPosition;
    private Quaternion resetRotation;

    private Coroutine tossRoutine;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        col = GetComponent<Collider>();

        if (inputCamera == null)
            inputCamera = Camera.main;

        rb.isKinematic = true;
        rb.useGravity = false;
    }

    private Camera GetCam()
    {
        return inputCamera != null ? inputCamera : Camera.main;
    }

    private Vector3 GetForwardDir()
    {
        Vector3 dir = forwardRef != null ? forwardRef.forward : Vector3.forward;
        dir.y = 0f;

        if (dir.sqrMagnitude < 0.0001f)
            dir = Vector3.forward;

        return dir.normalized;
    }

    private float GetFloorY()
    {
        if (floorRef != null)
            return floorRef.position.y + floorOffset;

        return resetPosition.y;
    }

    public void ResetCoin(Vector3 pos, Quaternion rot)
    {
        if (tossRoutine != null)
        {
            StopCoroutine(tossRoutine);
            tossRoutine = null;
        }

        rb.isKinematic = true;
        rb.useGravity = false;

        transform.position = pos;
        transform.rotation = rot;

        resetPosition = pos;
        resetRotation = rot;

        IsThrown = false;
        IsSettled = false;
        IsDragging = false;

        CurrentFace = CoinTossManager.CoinFace.Head;
        pendingFace = CoinTossManager.CoinFace.Head;
    }

    public void SetPendingFace(CoinTossManager.CoinFace face)
    {
        pendingFace = face;
        CurrentFace = face;
    }

    private void Update()
    {
        if (IsThrown || tossRoutine != null)
            return;

        if (Input.GetMouseButtonDown(0))
            TryBeginDrag();

        if (IsDragging && Input.GetMouseButton(0))
            UpdateDragPreview();

        if (IsDragging && Input.GetMouseButtonUp(0))
            EndDragAndThrow();
    }

    private void TryBeginDrag()
    {
        Camera cam = GetCam();
        if (cam == null)
            return;

        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        if (!Physics.Raycast(ray, out RaycastHit hit, 200f))
            return;

        if (hit.transform != transform && hit.rigidbody != rb)
            return;

        IsDragging = true;
        IsSettled = false;
        dragStartScreen = Input.mousePosition;
        dragStartWorldPos = transform.position;
        grabOffsetWS = transform.position - hit.point;

        dragPlane = new Plane(Vector3.up, dragStartWorldPos);
    }

    private void UpdateDragPreview()
    {
        Camera cam = GetCam();
        if (cam == null)
            return;

        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        if (!dragPlane.Raycast(ray, out float enter))
            return;

        Vector3 point = ray.GetPoint(enter);
        Vector3 target = point + grabOffsetWS;

        if (lockMoveToWorldZ)
        {
            float zDelta = target.z - dragStartWorldPos.z;
            zDelta = Mathf.Clamp(zDelta, -dragPreviewDistance, dragPreviewDistance);

            target.x = dragStartWorldPos.x;
            target.y = dragStartWorldPos.y;
            target.z = dragStartWorldPos.z + zDelta;
        }

        transform.position = target;
    }

    private void EndDragAndThrow()
    {
        IsDragging = false;

        Vector2 dragEndScreen = Input.mousePosition;
        Vector2 drag = dragEndScreen - dragStartScreen;

        float dragForward = drag.y;
        if (throwForwardOnly)
            dragForward = Mathf.Max(0f, dragForward);

        float throwPixels = Mathf.Abs(dragForward);

        if (throwPixels < minDragPixelsToThrow)
        {
            transform.position = resetPosition;
            transform.rotation = resetRotation;
            IsThrown = false;
            IsSettled = false;
            return;
        }

        float normalized = Mathf.InverseLerp(
            minDragPixelsToThrow,
            maxDragPixels,
            Mathf.Min(throwPixels, maxDragPixels)
        );
        if (CoinTossManager.Instance != null)
        {
            CoinTossManager.Instance.SetSequenceRunningText(true);
        }
        tossRoutine = StartCoroutine(CoAnimateToss(normalized));
    }

    private IEnumerator CoAnimateToss(float normalizedPower)
    {
        IsThrown = true;
        IsSettled = false;

        if (AudioManager.Instance != null &&
        AudioManager.Instance.skillSounds != null &&
        AudioManager.Instance.skillSounds.coinThrow != null)
        {
            AudioManager.Instance.PlaySFX(
                AudioManager.Instance.skillSounds.coinThrow
            );
        }

        Vector3 startPos = transform.position;
        Quaternion startRot = transform.rotation;

        Vector3 forward = GetForwardDir();

        float distance = Mathf.Lerp(minTravelDistance, maxTravelDistance, normalizedPower);
        float arcHeight = Mathf.Lerp(minArcHeight, maxArcHeight, normalizedPower);
        float duration = Mathf.Lerp(maxDuration, minDuration, normalizedPower);

        Vector3 endPos = startPos + forward * distance;
        endPos.y = GetFloorY();

        int halfSpins = Mathf.RoundToInt(Mathf.Lerp(minHalfSpins, maxHalfSpins, normalizedPower));

        float randomYaw = Random.Range(0f, 360f);

        Quaternion yawRot = Quaternion.AngleAxis(randomYaw, Vector3.up);
        Quaternion flatHeadRot = resetRotation * yawRot;
        Quaternion flatTailRot = flatHeadRot * Quaternion.AngleAxis(180f, Vector3.right);

        Quaternion finalRot = (pendingFace == CoinTossManager.CoinFace.Head) ? flatHeadRot : flatTailRot;

        Vector3 spinAxis = transform.right;
        float totalSpinAngle = 180f * halfSpins;

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);

            Vector3 pos = Vector3.Lerp(startPos, endPos, t);
            pos.y += Mathf.Sin(t * Mathf.PI) * arcHeight;
            transform.position = pos;

            Quaternion spinRot = startRot * Quaternion.AngleAxis(totalSpinAngle * t, spinAxis);
            transform.rotation = spinRot;

            yield return null;
        }

        transform.position = endPos;

        if (AudioManager.Instance != null &&
        AudioManager.Instance.skillSounds != null &&
        AudioManager.Instance.skillSounds.coinLand != null)
        {
            AudioManager.Instance.PlaySFX(
                AudioManager.Instance.skillSounds.coinLand
            );
        }

        Quaternion beforeSettleRot = transform.rotation;
        float settleElapsed = 0f;

        while (settleElapsed < settleRotateTime)
        {
            settleElapsed += Time.deltaTime;
            float t = Mathf.Clamp01(settleElapsed / settleRotateTime);
            transform.rotation = Quaternion.Slerp(beforeSettleRot, finalRot, t);
            yield return null;
        }

        transform.rotation = finalRot;
        CurrentFace = pendingFace;

        IsSettled = true;
        tossRoutine = null;
    }
}
