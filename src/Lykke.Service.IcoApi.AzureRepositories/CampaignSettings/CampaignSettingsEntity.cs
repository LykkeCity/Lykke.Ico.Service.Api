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
        public decimal PreSaleSmarcAmount { get; set; }
        public decimal PreSaleLogiAmount { get; set; }
        public decimal PreSaleSmarcPriceUsd { get; set; }
        public decimal PreSaleLogiPriceUsd { get; set; }

        public DateTime CrowdSaleStartDateTimeUtc { get; set; }
        public DateTime CrowdSaleEndDateTimeUtc { get; set; }

        public decimal CrowdSale1stTierSmarcPriceUsd { get; set; }
        public decimal CrowdSale1stTierSmarcAmount { get; set; }
        public decimal CrowdSale1stTierLogiPriceUsd { get; set; }
        public decimal CrowdSale1stTierLogiAmount { get; set; }

        public decimal CrowdSale2ndTierSmarcPriceUsd { get; set; }
        public decimal CrowdSale2ndTierSmarcAmount { get; set; }
        public decimal CrowdSale2ndTierLogiPriceUsd { get; set; }
        public decimal CrowdSale2ndTierLogiAmount { get; set; }

        public decimal CrowdSale3rdTierSmarcPriceUsd { get; set; }
        public decimal CrowdSale3rdTierSmarcAmount { get; set; }
        public decimal CrowdSale3rdTierLogiPriceUsd { get; set; }
        public decimal CrowdSale3rdTierLogiAmount { get; set; }

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
