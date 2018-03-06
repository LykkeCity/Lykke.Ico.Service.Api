using Lykke.Service.IcoApi.Core.Domain.Investor;

namespace Lykke.Service.IcoApi.Core.Domain.Campaign
{
    public class TokenInfo
    {
        public string Name { get; set; }

        public decimal? PriceUsd { get; set; }

        public CampaignPhase? Phase { get; set; }

        public decimal? PhaseTokenAmount { get; set; }

        public decimal? PhaseTokenAmountTotal { get; set; }

        public decimal? PhaseTokenAmountAvailable
        {
            get
            {
                return PhaseTokenAmountTotal - PhaseTokenAmount;
            }
        }

        public decimal? PhaseAmountUsdAvailable
        {
            get
            {
                return PhaseTokenAmountAvailable * PriceUsd;
            }
        }

        public string Error { get; set; }

        public InvestorRefundReason? ErrorReason { get; set; }
    }
}
