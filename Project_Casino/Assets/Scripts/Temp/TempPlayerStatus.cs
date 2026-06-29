using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TempPlayerStatus : MonoBehaviour
{
    public static TempPlayerStatus instance;

    public int health = 100;
    public int coinInPurse = 20; // 소지금
    public int coinInSafe = 0; // 금고에 보관된 돈

    private void Awake()
    {
        instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            Debug.Log($"체력 : {health} / 돈 : {coinInPurse} / 금고 : {coinInSafe}");
        }
    }
}
