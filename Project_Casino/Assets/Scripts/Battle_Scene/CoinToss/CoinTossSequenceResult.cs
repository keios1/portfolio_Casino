using System.Collections.Generic;
/// <summary>
/// 코인 토스 시퀀스 결과 데이터를 저장하는 클래스.
/// 기본 주사위 값, 누적 데미지, 성공 횟수, 각 토스 결과를 포함한다.
/// </summary>
[System.Serializable]
public class CoinTossSequenceResult
{
    public int baseDiceValue;
    public int totalDamage;
    public int successCount;
    public List<CoinTossManager.CoinFace> faces = new List<CoinTossManager.CoinFace>();
}
