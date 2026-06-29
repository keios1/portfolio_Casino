using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public class BigWheelBullet : MonoBehaviour
{
    public Transform beginPosition;
    public Transform endPosition;
    public float moveTime;
    public void Begin(System.Action endAction)
    {
        StartCoroutine(BulletMove(endAction));
    }

    IEnumerator BulletMove(System.Action endAction)
    {
        Debug.Log($">> {gameObject.name} 에서 BigWheelBullet.BulletMove()");

        float beginTime = Time.time;
        float EndTime = Time.time + moveTime;
        Vector3 beginWorldPosition = beginPosition.position;
        Vector3 endWorldPosition = endPosition.position;
        while (Time.time < EndTime)
        {
            transform.position = Vector3.Lerp(beginWorldPosition, endWorldPosition, (Time.time - beginTime) / moveTime);
            yield return null;
        }
        transform.position = endWorldPosition;
        endAction();
        Destroy(gameObject);
    }
}
