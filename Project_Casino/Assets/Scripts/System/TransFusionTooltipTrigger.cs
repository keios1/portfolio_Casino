using UnityEngine;

public class TransfusionTooltipTrigger : ExplainTooltipTrigger
{
    [SerializeField] private TransfusionManager transfusionManager;
    [SerializeField] private bool isCoinToBlood;

    protected override string GetExplainText()
    {
        if (transfusionManager == null)
            return "교환 정보를 불러올 수 없습니다.";

        if (isCoinToBlood)
        {
            return $"코인을 소모해 체력을 회복합니다.\n" +
                   $"현재 체력 : {transfusionManager.CurrentHp}\n" +
                   $"교환 후 체력 : {transfusionManager.PreviewHpAfterTransfusion}";
        }

        return $"체력을 소모해 코인을 획득합니다.\n" +
               $"현재 코인 : {transfusionManager.CurrentCoin}\n" +
               $"교환 후 코인 : {transfusionManager.PreviewCoinAfterDonation}";
    }
}
