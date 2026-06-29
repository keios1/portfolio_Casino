using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyContainer : MonoBehaviour
{
    public static EnemyContainer instance;

    public List<EnemyBase> enemies;

    public int GetEnemyCount()
    {
        if (enemies == null) return 0;

        int result = 0;
        foreach (EnemyBase enemy in enemies)
        {
            if (enemy != null && enemy.IsAlive())
                result++;
        }

        return result;
    }

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Debug.Log($">> EnemyContainer.Awake() => 이미 EnemyContainer 인스턴스({this.gameObject.name})가 존재합니다. 중복된 EnemyContainer는 제거됩니다.");
            Destroy(this.gameObject);
        }
        else
        {
            instance = this;
        }

    }
}
