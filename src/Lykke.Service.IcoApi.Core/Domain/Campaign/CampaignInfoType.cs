namespace Lykke.Service.IcoApi.Core.Domain.Campaign
{
    public enum CampaignInfoType
    {
        AmountInvestedFiat,
        AmountInvestedBtc,
        AmountInvestedEth,
        AmountInvestedUsd,
        AmountInvestedToken,
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
