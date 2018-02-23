using Lykke.Ico.Core;

namespace Lykke.Service.IcoApi.Models
{
    public class CampaignResponse
    {
        public decimal InvestedUsd { get; set; }

        public decimal? TokenPriceUsd { get; set; }

        public TokenPricePhase? Phase { get; set; }

        public bool CampaignActive { get; set; }

        public bool CaptchaEnabled { get; set; }
    }
}
