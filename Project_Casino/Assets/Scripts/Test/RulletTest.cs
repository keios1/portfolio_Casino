using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RulletTest : MonoBehaviour
{
    public CardEffect testEffect;

    public void TestEffect()
    {
        StartCoroutine(RunTest());
    }

    private IEnumerator RunTest()
    {
        int[] dice = new int[] { 3 };

        BattleContext context = null;
        ICardTarget target = null;

        yield return testEffect.Apply(
            context,
            target,
            dice,
            (result) => { }
         );

        Debug.Log("이벤트 실행완료");

    }


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
