using System.Threading.Tasks;
using Lykke.Ico.Core.Repositories.CampaignInfo;
using Lykke.Ico.Core.Repositories.CampaignSettings;

namespace Lykke.Service.IcoApi.Core.Services
{
    public interface ICampaignService
    {
        Task<string> GetCampaignInfoValue(CampaignInfoType type);
        Task<ICampaignSettings> GetCampaignSettings();
    }
}
