using Common.Log;
using System.Threading.Tasks;
using Lykke.Ico.Core.Repositories.CampaignInfo;
using Lykke.Ico.Core.Repositories.CampaignSettings;
using Lykke.Service.IcoApi.Core.Services;
using Microsoft.Extensions.Caching.Memory;
using Lykke.Ico.Core.Repositories.InvestorRefund;
using System.Collections.Generic;

namespace Lykke.Service.IcoApi.Services
{
    public class CampaignService : ICampaignService
    {
        private readonly ILog _log;
        private readonly ICampaignInfoRepository _campaignInfoRepository;
        private readonly ICampaignSettingsRepository _campaignSettingsRepository;
        private readonly IInvestorRefundRepository _investorRefundRepository;
        private readonly IMemoryCache _cache;
        private const string _cachKey = "CampaignSettings";

        public CampaignService(ILog log,
            ICampaignInfoRepository campaignInfoRepository,
            ICampaignSettingsRepository campaignSettingsRepository,
            IInvestorRefundRepository investorRefundRepository,
            IMemoryCache cache)
        {
            _log = log;
            _campaignInfoRepository = campaignInfoRepository;
            _campaignSettingsRepository = campaignSettingsRepository;
            _investorRefundRepository = investorRefundRepository;
            _cache = cache;
        }

        public async Task<string> GetCampaignInfoValue(CampaignInfoType type)
        {
            return await _campaignInfoRepository.GetValueAsync(type);
        }

        public async Task<ICampaignSettings> GetCampaignSettings()
        {
            if (!_cache.TryGetValue(_cachKey, out ICampaignSettings campaignSettings))
            {
                campaignSettings = await _campaignSettingsRepository.GetAsync();

                _cache.Set(_cachKey, campaignSettings);
            }

            return campaignSettings;
        }

        public async Task SaveCampaignSettings(ICampaignSettings settings)
        {
            await _campaignSettingsRepository.SaveAsync(settings);

            _cache.Remove(_cachKey);
        }

        public async Task<IEnumerable<IInvestorRefund>> GetRefunds()
        {
            return await _investorRefundRepository.GetAllAsync();
        }
    }
}
