using Lykke.Service.IcoApi.Core.Domain.Campaign;

namespace Lykke.Service.IcoApi.Models
{
    public class CampaignResponse
    {
        public bool CampaignActive { get; set; }        
        public bool CaptchaEnabled { get; set; }

        public CampaignTier? SmarcPhase { get; set; }
        public decimal? SmarcPhaseAmount { get; set; }
        public decimal? SmarcPhaseAmountAvailable { get; set; }
        public decimal? SmarcPhasePriceUsd { get; set; }

        public decimal? SmarcPresaleAmount { get; set; }
        public decimal? SmarcPresaleAmountAvailable { get; set; }
        public decimal? SmarcPresalePriceUsd { get; set; }

        public CampaignTier? SmarcCrowdsaleTier { get; set; }
        public decimal? SmarcCrowdsaleAmount { get; set; }
        public decimal? SmarcCrowdsaleAmountAvailable { get; set; }
        public decimal? SmarcCrowdsalePriceUsd { get; set; }

        public CampaignTier? LogiPhase { get; set; }
        public decimal? LogiPhaseAmount { get; set; }
        public decimal? LogiPhaseAmountAvailable { get; set; }
        public decimal? LogiPhasePriceUsd { get; set; }

        public decimal? LogiPresaleAmount { get; set; }
        public decimal? LogiPresaleAmountAvailable { get; set; }
        public decimal? LogiPresalePriceUsd { get; set; }

        public CampaignTier? LogiCrowdsaleTier { get; set; }
        public decimal? LogiCrowdsaleAmount { get; set; }
        public decimal? LogiCrowdsaleAmountAvailable { get; set; }
        public decimal? LogiCrowdsalePriceUsd { get; set; }
    }
}
