namespace Lykke.Service.IcoApi.Core.Domain.Campaign
{
    public enum CampaignInfoType
    {
        AmountPreSaleInvestedFiat,
        AmountPreSaleInvestedBtc,
        AmountPreSaleInvestedEth,
        AmountPreSaleInvestedUsd,
        AmountPreSaleInvestedToken,
        AmountCrowdSaleInvestedFiat,
        AmountCrowdSaleInvestedBtc,
        AmountCrowdSaleInvestedEth,
        AmountCrowdSaleInvestedUsd,
        AmountCrowdSaleInvestedToken,
        AddressPoolTotalSize,
        AddressPoolCurrentSize,
        LastProcessedBlockBtc,
        LastProcessedBlockEth,
        LastProcessedBlockEthInfura,
        InvestorsRegistered,
        InvestorsConfirmed,
        InvestorsFilledIn,
        InvestorsPaidIn,
        InvestorsKycRequested,
        InvestorsKycPassed,
        LatestTransactions
    }
}
