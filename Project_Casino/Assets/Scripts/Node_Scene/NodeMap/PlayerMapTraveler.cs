using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System;

/// <summary>
/// 노드 씬에 존재하는 플레이어를 이동 시키는 스크립트
/// </summary>
public class PlayerMapTraveler : MonoBehaviour
{
    public float playerY = 0f;
    public float moveSpeed = 5f;

    private void Start()
    {
        SetPositionImmediate();
    }

    /// <summary>
    /// 플레이어 중심점에서 바닥까지 계산
    /// </summary>
    /// <returns></returns>
    private float GetPlayerHalfHeight()
    {
        if (TryGetComponent<Collider>(out var collider))
            return collider.bounds.extents.y;

        return 35f;
    }

    /// <summary>
    /// 노드 꼭대기 y 값 반환
    /// </summary>
    /// <param name="nodeTransform"></param>
    /// <returns></returns>
    private float GetNodeTopY(Transform nodeTransform)
    {
        if(nodeTransform.TryGetComponent<Collider>(out var collider))
            return collider.bounds.max.y;

        return nodeTransform.position.y;
    }

    public void SetPositionImmediate()
    {
        if (MapNodeManager.Instance != null && MapNodeManager.Instance.nodeList.Count > 0)
        {
            int index = MapNodeManager.Instance.GetEnteringNodeIndex();

            if (index < 0 || index >= MapNodeManager.Instance.nodeList.Count)
                index = 0;

            Transform targetNode = MapNodeManager.Instance.nodeList[index].transform;

            // 1. 노드의 맨 윗면 Y 좌표
            float nodeTopY = GetNodeTopY(targetNode);
            // 2. 플레이어 중심에서 발바닥까지의 거리
            float playerHalfHeight = GetPlayerHalfHeight();

            // 최종 위치: 노드 맨 윗면 + 플레이어 몸통 절반 높이 + 미세조정 오프셋
            float finalY = nodeTopY + playerHalfHeight + playerY;

            transform.position = new Vector3(targetNode.position.x, finalY, targetNode.position.z);
        }
    }


    public IEnumerator MoveAndExecute(Vector3 targetPos, Action onArrivedAction)
    {
        float nodeTopY = targetPos.y;

        int index = MapNodeManager.Instance.GetEnteringNodeIndex();
        if (index >= 0 && index < MapNodeManager.Instance.nodeList.Count)
        {
            nodeTopY = GetNodeTopY(MapNodeManager.Instance.nodeList[index].transform);
        }

        // 코루틴 이동 시에도 동일하게 계산 적용
        float playerHalfHeight = GetPlayerHalfHeight();
        float finalY = nodeTopY + playerHalfHeight + playerY;

        Vector3 destination = new Vector3(targetPos.x, finalY, targetPos.z);

        while (Vector3.Distance(transform.position, destination) > 0.01f)
        {
            transform.position = Vector3.MoveTowards(transform.position, destination, moveSpeed * Time.deltaTime);
            yield return null;
        }

        transform.position = destination;
        yield return new WaitForSeconds(0.1f);

        if (onArrivedAction != null)
        {
            onArrivedAction.Invoke();
        }
    }
}
