using System;

namespace Lykke.Service.IcoApi.Core.Domain.Campaign
{
    public interface ICampaignSettings
    {
        DateTime PreSaleStartDateTimeUtc { get; set; }
        DateTime PreSaleEndDateTimeUtc { get; set; }
        decimal PreSaleTokenAmount { get; set; }
        decimal PreSaleTokenPriceUsd { get; set; }

        DateTime CrowdSaleStartDateTimeUtc { get; set; }
        DateTime CrowdSaleEndDateTimeUtc { get; set; }
        decimal CrowdSale1stTierTokenPriceUsd { get; set; }
        decimal CrowdSale1stTierTokenAmount { get; set; }
        decimal CrowdSale2ndTierTokenPriceUsd { get; set; }
        decimal CrowdSale2ndTierTokenAmount { get; set; }
        decimal CrowdSale3rdTierTokenPriceUsd { get; set; }
        decimal CrowdSale3rdTierTokenAmount { get; set; }

        int RowndDownTokenDecimals { get; set; }
        decimal MinInvestAmountUsd { get; set; }
        bool EnableFrontEnd { get; set; }

        bool CaptchaEnable { get; set; }
        string CaptchaSecret { get; set; }

        bool KycEnableRequestSending { get; set; }
        string KycCampaignId { get; set; }
        string KycLinkTemplate { get; set; }
        string KycServiceEncriptionKey { get; set; }
        string KycServiceEncriptionIv { get; set; }
    }
}
