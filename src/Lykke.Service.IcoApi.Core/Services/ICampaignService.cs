using System.Threading.Tasks;
using System.Collections.Generic;
using Lykke.Service.IcoApi.Core.Domain.Investor;
using Lykke.Service.IcoApi.Core.Domain.Campaign;

namespace Lykke.Service.IcoApi.Core.Services
{
    public interface ICampaignService
    {
        Task<string> GetCampaignInfoValue(CampaignInfoType type);

        Task<ICampaignSettings> GetCampaignSettings();

        Task SaveCampaignSettings(ICampaignSettings settings, string username);

        Task<IEnumerable<IInvestorRefund>> GetRefunds();

        Task<IEnumerable<ICampaignSettingsHistoryItem>> GetCampaignSettingsHistory();
    }
}
