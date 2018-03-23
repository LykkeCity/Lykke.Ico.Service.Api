using Lykke.Service.IcoApi.Core.Domain.Campaign;

namespace Lykke.Service.IcoApi.Models
{
    public class CampaignResponse
    {
        public bool CampaignActive { get; set; }        
        public bool CaptchaEnabled { get; set; }

        public CampaignPhase? SmarcPhase { get; set; }
        public decimal? SmarcPhaseAmount { get; set; }
        public decimal? SmarcPhaseAmountAvailable { get; set; }
        public decimal? SmarcPhasePriceUsd { get; set; }

        public CampaignPhase? LogiPhase { get; set; }
        public decimal? LogiPhaseAmount { get; set; }
        public decimal? LogiPhaseAmountAvailable { get; set; }
        public decimal? LogiPhasePriceUsd { get; set; }
    }
}
