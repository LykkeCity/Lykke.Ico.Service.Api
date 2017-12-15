using Common.Log;
using System.Threading.Tasks;
using Lykke.Ico.Core.Repositories.CampaignInfo;
using Lykke.Ico.Core.Repositories.CampaignSettings;
using Lykke.Service.IcoApi.Core.Services;

namespace Lykke.Service.IcoApi.Services
{
    public class CampaignService : ICampaignService
    {
        private readonly ILog _log;
        private readonly ICampaignInfoRepository _campaignInfoRepository;
        private readonly ICampaignSettingsRepository _campaignSettingsRepository;

        public CampaignService(ILog log,
            ICampaignInfoRepository campaignInfoRepository,
            ICampaignSettingsRepository campaignSettingsRepository)
        {
            _log = log;
            _campaignInfoRepository = campaignInfoRepository;
            _campaignSettingsRepository = campaignSettingsRepository;
        }

        public async Task<string> GetCampaignInfoValue(CampaignInfoType type)
        {
            return await _campaignInfoRepository.GetValueAsync(type);
        }

        public async Task<ICampaignSettings> GetCampaignSettings()
        {
            return await _campaignSettingsRepository.GetAsync();
        }
    }
}
