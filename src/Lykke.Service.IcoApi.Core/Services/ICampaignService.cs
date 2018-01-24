using System.Threading.Tasks;
using Lykke.Ico.Core.Repositories.CampaignInfo;
using Lykke.Ico.Core.Repositories.CampaignSettings;
using System.Collections.Generic;
using Lykke.Ico.Core.Repositories.InvestorRefund;

namespace Lykke.Service.IcoApi.Core.Services
{
    public interface ICampaignService
    {
        Task<string> GetCampaignInfoValue(CampaignInfoType type);

        Task<ICampaignSettings> GetCampaignSettings();

        Task SaveCampaignSettings(ICampaignSettings settings);

        Task<IEnumerable<IInvestorRefund>> GetRefunds();
    }
}
