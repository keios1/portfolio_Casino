using System;
using System.Collections;
using UnityEngine;
/// <summary>
/// 카드의 실제 효과 로직을 정의하는 추상 클래스.
/// 각 카드별 효과는 ScriptableObject로 구현되며,
/// 코루틴 기반으로 실행되어 연출 및 비동기 처리를 지원합니다.
/// </summary>
public abstract class CardEffect : ScriptableObject
{
    public abstract IEnumerator Apply(
        BattleContext context,
        ICardTarget target,
        int[] consumedDice,
        Action<bool> onFinished
    ); 
}
