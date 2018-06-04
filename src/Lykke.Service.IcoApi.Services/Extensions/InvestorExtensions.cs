using Lykke.Service.IcoApi.Core.Domain.Campaign;
using Lykke.Service.IcoApi.Core.Domain.Investor;
using System;

namespace Lykke.Service.IcoApi.Services.Extensions
{
    public static class InvestorExtensions
    {
        public static CampaignPhase GetCampaignPhase(this IInvestor self)
        {
            return self.Phase == nameof(CampaignPhase.CrowdSale) ? CampaignPhase.CrowdSale : CampaignPhase.PreSale;
        }

        public static string GetCampaignPhaseString(this IInvestor self)
        {
            return Enum.GetName(typeof(CampaignPhase), self.GetCampaignPhase());
        }
    }
}
