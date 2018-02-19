﻿using System;

namespace Lykke.Service.IcoApi.Core.Domain.Campaign
{
    public interface ICampaignSettings
    {
        DateTime PreSaleStartDateTimeUtc { get; set; }

        DateTime PreSaleEndDateTimeUtc { get; set; }

        int PreSaleTotalTokensAmount { get; set; }

        DateTime CrowdSaleStartDateTimeUtc { get; set; }

        DateTime CrowdSaleEndDateTimeUtc { get; set; }

        int CrowdSaleTotalTokensAmount { get; set; }

        decimal TokenBasePriceUsd { get; set; }

        int TokenDecimals { get; set; }

        decimal MinInvestAmountUsd { get; set; }

        bool KycEnableRequestSending { get; set; }

        string KycCampaignId { get; set; }

        string KycLinkTemplate { get; set; }

        bool CaptchaEnable { get; set; }
    }
}