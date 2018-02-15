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

        public int PreSaleTotalTokensAmount { get; set; }

        public DateTime CrowdSaleStartDateTimeUtc { get; set; }

        public DateTime CrowdSaleEndDateTimeUtc { get; set; }

        public int CrowdSaleTotalTokensAmount { get; set; }

        public decimal TokenBasePriceUsd { get; set; }

        public int TokenDecimals { get; set; }

        public decimal MinInvestAmountUsd { get; set; }

        public bool KycEnableRequestSending { get; set; }

        public string KycCampaignId { get; set; }

        public string KycLinkTemplate { get; set; }

        public bool CaptchaEnable { get; set; }

        public DateTime UpdatedUtc { get; set; }
    }
}
