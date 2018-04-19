using Common;
using Lykke.Service.IcoApi.Core.Repositories;
using Lykke.Service.IcoApi.Core.Services;
using Lykke.Service.IcoApi.Services.Helpers;
using System.Threading.Tasks;

namespace Lykke.Service.IcoApi.Services
{
    public class KycService : IKycService
    {
        private readonly ICampaignSettingsRepository _campaignSettingsRepository;

        public KycService(ICampaignSettingsRepository campaignSettingsRepository)
        {
            _campaignSettingsRepository = campaignSettingsRepository;
        }

        public async Task<string> GetKycLink(string email, string kycId)
        {
            var settings = await _campaignSettingsRepository.GetAsync();
            if (string.IsNullOrEmpty(settings.KycCampaignId))
            {
                return "";
            }

            var kycMessage = new { campaignId = settings.KycCampaignId, email = email, kycId = kycId };
            var kycEncryptedMessage = EncryptionHelper.Encrypt(kycMessage.ToJson(), 
                settings.KycServiceEncriptionKey, settings.KycServiceEncriptionIv);
            var kycLink = settings.KycLinkTemplate.Replace("{kycEncryptedMessage}", kycEncryptedMessage);

            return kycLink;
        }

        public async Task<string> Encrypt(string message)
        {
            var settings = await _campaignSettingsRepository.GetAsync();

            return EncryptionHelper.Encrypt(message, settings.KycServiceEncriptionKey, 
                settings.KycServiceEncriptionIv);
        }

        public async Task<string> Decrypt(string message)
        {
            var settings = await _campaignSettingsRepository.GetAsync();

            return EncryptionHelper.Decrypt(message, settings.KycServiceEncriptionKey,
                settings.KycServiceEncriptionIv);
        }
    }
}
