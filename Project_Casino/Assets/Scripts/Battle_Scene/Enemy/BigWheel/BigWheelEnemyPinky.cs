using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BigWheelEnemyPinky : EnemyBase
{
    public override IEnumerator ExecuteEnemyTurn()
    {
        yield return null;
    }

    public void ApplyDamageToPlayerPublic(int damage)
    {
        ApplyDamageToPlayer(damage);
    }

    public void ForceChangeSpritePublic(EAnimationRequestArgument animArg)
    {


        //ForceChangeSprite(newSprite);
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
