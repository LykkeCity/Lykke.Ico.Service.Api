using Common.Log;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Extensions.Caching.Memory;
using Lykke.Service.IcoApi.Core.Services;
using Lykke.Service.IcoApi.Core.Repositories;
using Lykke.Service.IcoApi.Core.Domain.Campaign;
using Lykke.Service.IcoApi.Core.Domain.Investor;

namespace Lykke.Service.IcoApi.Services
{
    public class CampaignService : ICampaignService
    {
        private readonly ILog _log;
        private readonly ICampaignInfoRepository _campaignInfoRepository;
        private readonly ICampaignSettingsRepository _campaignSettingsRepository;
        private readonly IInvestorRefundRepository _investorRefundRepository;
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
        }

        public async Task<string> GetCampaignInfoValue(CampaignInfoType type)
        {
            return await _campaignInfoRepository.GetValueAsync(type);
        }

        public async Task<ICampaignSettings> GetCampaignSettings()
        {
            return await _campaignSettingsRepository.GetAsync();
        }

        public async Task SaveCampaignSettings(ICampaignSettings settings, string username)
        {
            await _campaignSettingsRepository.SaveAsync(settings, username);
        }

        public async Task<IEnumerable<IInvestorRefund>> GetRefunds()
        {
            return await _investorRefundRepository.GetAllAsync();
        }

        public async Task<IEnumerable<ICampaignSettingsHistoryItem>> GetCampaignSettingsHistory()
        {
            return await _campaignSettingsRepository.GetHistoryAsync();
        }
    }
}
