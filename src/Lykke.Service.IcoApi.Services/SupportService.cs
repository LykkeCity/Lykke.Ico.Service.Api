using Common;
using Common.Log;
using Lykke.Ico.Core.Repositories.CampaignSettings;
using Lykke.Ico.Core.Repositories.Investor;
using Lykke.Service.IcoApi.Core.Services;
using System.Threading.Tasks;

namespace Lykke.Service.IcoApi.Services
{
    public class SupportService : ISupportService
    {
        private readonly ILog _log;
        private readonly IInvestorRepository _investorRepository;
        private readonly ICampaignSettingsRepository _campaignSettingsRepository;

        public SupportService(
            ILog log,
            IInvestorRepository investorRepository,
            ICampaignSettingsRepository campaignSettingsRepository)
        {
            _log = log;
            _investorRepository = investorRepository;
            _campaignSettingsRepository = campaignSettingsRepository;
        }

        public async Task UpdateMinInvestAmount(decimal minInvestAmountUsd)
        {
            var settings = await _campaignSettingsRepository.GetAsync();

            settings.MinInvestAmountUsd = minInvestAmountUsd;

            await _campaignSettingsRepository.SaveAsync(settings);
        }

        public async Task UpdateInvestorAsync(string email, string tokenAddress, string refundEthAddress, string refundBtcAddress)
        {
            email = email.ToLowCase();

            await _investorRepository.SaveAddressesAsync(email, tokenAddress, refundEthAddress, refundBtcAddress);
        }
    }
}
