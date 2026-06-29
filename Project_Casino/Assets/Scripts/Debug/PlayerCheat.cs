using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCheat : MonoBehaviour
{
    private void Start()
    {
        // dont destroy this object when loading new scenes
        DontDestroyOnLoad(transform.root.gameObject);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKey(KeyCode.LeftShift) && Input.GetKeyDown(KeyCode.H))
        {
            PlayerRuntimeData.Instance.maxHp += 100000;
            PlayerRuntimeData.Instance.currentHp = PlayerRuntimeData.Instance.maxHp;
            PlayerRuntimeData.Instance.SaveToSaveData();
        }
        if (Input.GetKey(KeyCode.LeftShift) && Input.GetKeyDown(KeyCode.C))
        {
            PlayerRuntimeData.Instance.coin += 100000;
            PlayerRuntimeData.Instance.SaveToSaveData();
        }
        if (Input.GetKey(KeyCode.LeftShift) && Input.GetKeyDown(KeyCode.M))
        {
            MapNodeManager.Instance.currentUnlockedIndex = 999;
        }
        if (Input.GetKey(KeyCode.LeftShift) && Input.GetKeyDown(KeyCode.Alpha6))
        {
            
            // 바로 6번쩨 스테이지로 이동할 수 있도록 세팅
        }
    }
}
