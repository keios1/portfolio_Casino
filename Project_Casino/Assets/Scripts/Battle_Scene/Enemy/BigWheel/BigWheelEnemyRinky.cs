using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BigWheelEnemyRinky : EnemyBase
{

    public void ApplyDamageToPlayerPublic(int damage)
    {
        ApplyDamageToPlayer(damage);
    }

    public override IEnumerator ExecuteEnemyTurn()
    {
        yield return null;
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
