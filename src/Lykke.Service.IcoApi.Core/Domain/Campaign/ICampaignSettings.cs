using System;

namespace Lykke.Service.IcoApi.Core.Domain.Campaign
{
    public interface ICampaignSettings
    {
        DateTime PreSaleStartDateTimeUtc { get; set; }
        DateTime? PreSaleEndDateTimeUtc { get; set; }
        decimal PreSaleSmarcAmount { get; set; }
        decimal PreSaleLogiAmount { get; set; }
        decimal PreSaleSmarcPriceUsd { get; set; }
        decimal PreSaleLogiPriceUsd { get; set; }

        DateTime CrowdSaleStartDateTimeUtc { get; set; }
        DateTime? CrowdSaleEndDateTimeUtc { get; set; }
        decimal CrowdSale1stTierSmarcPriceUsd { get; set; }
        decimal CrowdSale1stTierSmarcAmount { get; set; }
        decimal CrowdSale1stTierLogiPriceUsd { get; set; }
        decimal CrowdSale1stTierLogiAmount { get; set; }
        decimal CrowdSale2ndTierSmarcPriceUsd { get; set; }
        decimal CrowdSale2ndTierSmarcAmount { get; set; }
        decimal CrowdSale2ndTierLogiPriceUsd { get; set; }
        decimal CrowdSale2ndTierLogiAmount { get; set; }
        decimal CrowdSale3rdTierSmarcPriceUsd { get; set; }
        decimal CrowdSale3rdTierSmarcAmount { get; set; }
        decimal CrowdSale3rdTierLogiPriceUsd { get; set; }
        decimal CrowdSale3rdTierLogiAmount { get; set; }

        int RowndDownTokenDecimals { get; set; }
        decimal MinInvestAmountUsd { get; set; }
        bool EnableFrontEnd { get; set; }
        decimal? MinEthExchangeRate { get; set; }
        decimal? MinBtcExchangeRate { get; set; }

        bool CaptchaEnable { get; set; }
        string CaptchaSecret { get; set; }

        bool KycEnableRequestSending { get; set; }
        string KycCampaignId { get; set; }
        string KycLinkTemplate { get; set; }
        string KycServiceEncriptionKey { get; set; }
        string KycServiceEncriptionIv { get; set; }
    }
}
