using Lykke.AzureStorage.Tables;
using Lykke.AzureStorage.Tables.Entity.Annotation;
using Lykke.AzureStorage.Tables.Entity.ValueTypesMerging;
using Lykke.Service.IcoApi.Core.Domain.Campaign;
using System;

namespace Lykke.Services.IcoApi.AzureRepositories
{
    [ValueTypeMergingStrategy(ValueTypeMergingStrategy.UpdateAlways)]
    internal class CampaignSettingsEntity : AzureTableEntity, ICampaignSettings
    {
        public DateTime PreSaleStartDateTimeUtc { get; set; }
        public DateTime PreSaleEndDateTimeUtc { get; set; }
        public decimal PreSaleTokenAmount { get; set; }
        public decimal PreSaleTokenPriceUsd { get; set; }

        public DateTime CrowdSaleStartDateTimeUtc { get; set; }
        public DateTime CrowdSaleEndDateTimeUtc { get; set; }

        public decimal CrowdSale1stTierTokenPriceUsd { get; set; }
        public decimal CrowdSale1stTierTokenAmount { get; set; }

        public decimal CrowdSale2ndTierTokenPriceUsd { get; set; }
        public decimal CrowdSale2ndTierTokenAmount { get; set; }

        public decimal CrowdSale3rdTierTokenPriceUsd { get; set; }
        public decimal CrowdSale3rdTierTokenAmount { get; set; }

        public decimal MinInvestAmountUsd { get; set; }
        public int RowndDownTokenDecimals { get; set; }
        public bool EnableFrontEnd { get; set; }

        public bool CaptchaEnable { get; set; }
        public string CaptchaSecret { get; set; }

        public bool KycEnableRequestSending { get; set; }
        public string KycCampaignId { get; set; }
        public string KycLinkTemplate { get; set; }
        public string KycServiceEncriptionKey { get; set; }
        public string KycServiceEncriptionIv { get; set; }
    }
}
