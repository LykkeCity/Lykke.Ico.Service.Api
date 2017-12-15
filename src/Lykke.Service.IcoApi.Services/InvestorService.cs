using Common;
using Common.Log;
using System;
using System.Threading.Tasks;
using Lykke.Service.IcoApi.Core.Services;
using Lykke.Ico.Core.Repositories.Investor;
using Lykke.Ico.Core.Repositories.InvestorAttribute;
using Lykke.Ico.Core.Queues.Emails;
using Lykke.Ico.Core.Queues;
using Lykke.Ico.Core.Repositories.AddressPool;
using Lykke.Service.IcoApi.Core.Domain;
using Lykke.Ico.Core.Repositories.CampaignInfo;
using Lykke.Service.IcoApi.Core.Settings.ServiceSettings;

namespace Lykke.Service.IcoApi.Services
{
    public class InvestorService : IInvestorService
    {
        private readonly ILog _log;
        private readonly IBtcService _btcService;
        private readonly IEthService _ethService;
        private readonly IInvestorRepository _investorRepository;
        private readonly IInvestorAttributeRepository _investorAttributeRepository;
        private readonly IAddressPoolRepository _addressPoolRepository;
        private readonly ICampaignInfoRepository _campaignInfoRepository;
        private readonly IQueuePublisher<InvestorConfirmationMessage> _investorConfirmationQueuePublisher;
        private readonly IQueuePublisher<InvestorSummaryMessage> _investorSummaryQueuePublisher;
        private readonly IcoApiSettings _icoApiSettings;

        public InvestorService(ILog log,
            IBtcService btcService,
            IEthService ethService,
            IInvestorRepository investorRepository,
            IInvestorAttributeRepository investorAttributeRepository,
            IAddressPoolRepository addressPoolRepository,
            ICampaignInfoRepository campaignInfoRepository,
            IQueuePublisher<InvestorConfirmationMessage> investorConfirmationQueuePublisher,
            IQueuePublisher<InvestorSummaryMessage> investorSummaryQueuePublisher,
            IcoApiSettings icoApiSettings)
        {
            _log = log;
            _btcService = btcService;
            _ethService = ethService;
            _investorRepository = investorRepository;
            _investorAttributeRepository = investorAttributeRepository;
            _addressPoolRepository = addressPoolRepository;
            _campaignInfoRepository = campaignInfoRepository;
            _investorConfirmationQueuePublisher = investorConfirmationQueuePublisher;
            _investorSummaryQueuePublisher = investorSummaryQueuePublisher;
            _icoApiSettings = icoApiSettings;
        }

        public async Task<IInvestor> GetAsync(string email)
        {
            return await _investorRepository.GetAsync(email);
        }

        public async Task<RegisterResult> RegisterAsync(string email)
        {
            var investor = await _investorRepository.GetAsync(email);
            if (investor == null)
            {
                var token = Guid.NewGuid();

                await _log.WriteInfoAsync(nameof(InvestorService), nameof(RegisterAsync), $"Create investor for email={email} and token={token}");

                await _investorRepository.AddAsync(email, token);
                await _investorAttributeRepository.SaveAsync(InvestorAttributeType.ConfirmationToken, email, token.ToString());
                await _campaignInfoRepository.IncrementValue(CampaignInfoType.InvestorsRegistered, 1);

                await SendConfirmationEmail(email, token);

                return RegisterResult.ConfirmationEmailSent;
            }

            if (string.IsNullOrEmpty(investor.TokenAddress))
            {
                await SendConfirmationEmail(investor.Email, investor.ConfirmationToken.Value);

                return RegisterResult.ConfirmationEmailSent;
            }

            await SendSummaryEmail(investor);

            return RegisterResult.SummaryEmailSent;
        }

        public async Task<bool> ConfirmAsync(Guid confirmationToken)
        {
            var email = await _investorAttributeRepository.GetInvestorEmailAsync(InvestorAttributeType.ConfirmationToken, confirmationToken.ToString());
            if (string.IsNullOrEmpty(email))
            {
                await _log.WriteInfoAsync(nameof(InvestorService), nameof(ConfirmAsync), $"Token {confirmationToken} is not found");
                return false;
            }

            var investor = await _investorRepository.GetAsync(email);
            if (investor == null)
            {
                throw new Exception($"Investor with email={email} was not found");
            }
            if (investor.ConfirmedUtc.HasValue)
            {
                return true;
            }

            await _investorRepository.ConfirmAsync(email);
            await _campaignInfoRepository.IncrementValue(CampaignInfoType.InvestorsConfirmed, 1);

            return true;
        }

        public async Task UpdateAsync(string email, string tokenAddress, string refundEthAddress, string refundBtcAddress)
        {
            var poolItem = await _addressPoolRepository.GetNextFree(email);

            await _log.WriteInfoAsync(nameof(InvestorService), nameof(UpdateAsync), $"Address pool item: {poolItem.ToJson()}");
            await _campaignInfoRepository.IncrementValue(CampaignInfoType.AddressPoolCurrentSize, -1);

            var payInEthPublicKey = poolItem.EthPublicKey;
            var payInEthAddress = _ethService.GetAddressByPublicKey(poolItem.EthPublicKey);
            var payInBtcPublicKey = poolItem.BtcPublicKey;
            var payInBtcAddress = _btcService.GetAddressByPublicKey(poolItem.BtcPublicKey);

            await _log.WriteInfoAsync(nameof(InvestorService), nameof(UpdateAsync), 
                $"Invertor to save: tokenAddress={tokenAddress}, refundEthAddress={refundEthAddress}, refundBtcAddress={refundBtcAddress}" +
                $"payInEthPublicKey={payInEthPublicKey}, payInEthAddress={payInEthAddress}, payInBtcPublicKey={payInBtcPublicKey}, payInBtcAddress={payInBtcAddress}");
            await _investorRepository.SaveAddressesAsync(email, tokenAddress, refundEthAddress, refundBtcAddress,
                payInEthPublicKey, payInEthAddress, payInBtcPublicKey, payInBtcAddress);

            await _investorAttributeRepository.SaveAsync(InvestorAttributeType.PayInBtcAddress, email, payInBtcAddress);
            await _investorAttributeRepository.SaveAsync(InvestorAttributeType.PayInEthAddress, email, payInEthAddress);
            await _campaignInfoRepository.IncrementValue(CampaignInfoType.InvestorsFilledIn, 1);

            var investor = await _investorRepository.GetAsync(email);
            await SendSummaryEmail(investor);
        }

        private async Task SendConfirmationEmail(string email, Guid token)
        {
            var message = new InvestorConfirmationMessage
            {
                EmailTo = email,
                ConfirmationLink = $"{_icoApiSettings.IcoSiteUrl}participate/verify/{token}" 
            };

            await _log.WriteInfoAsync(nameof(InvestorService), nameof(SendConfirmationEmail), $"Send InvestorConfirmationMessage: {message.ToJson()}");
            await _investorConfirmationQueuePublisher.SendAsync(message);
        }

        private async Task SendSummaryEmail(IInvestor investor)
        {
            var message = new InvestorSummaryMessage
            {
                EmailTo = investor.Email,
                TokenAddress = investor.TokenAddress,
                RefundBtcAddress = investor.RefundBtcAddress,
                RefundEthAddress = investor.RefundEthAddress,
                PayInBtcAddress = investor.PayInBtcAddress,
                PayInEthAddress = investor.PayInEthAddress,
                LinkBtcAddress = $"{_icoApiSettings.BtcTrackerUrl}address",
                LinkEthAddress = $"{_icoApiSettings.EthTrackerUrl}address"
            };

            await _log.WriteInfoAsync(nameof(InvestorService), nameof(SendSummaryEmail), $"Send InvestorSummaryMessage: {message.ToJson()}");
            await _investorSummaryQueuePublisher.SendAsync(message);
        }
    }
}
