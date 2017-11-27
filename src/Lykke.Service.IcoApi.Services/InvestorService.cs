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

        public InvestorService(ILog log,
            IBtcService btcService,
            IEthService ethService,
            IInvestorRepository investorRepository,
            IInvestorAttributeRepository investorAttributeRepository,
            IAddressPoolRepository addressPoolRepository,
            ICampaignInfoRepository campaignInfoRepository,
            IQueuePublisher<InvestorConfirmationMessage> investorConfirmationQueuePublisher,
            IQueuePublisher<InvestorSummaryMessage> investorSummaryQueuePublisher)
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
                await _investorRepository.AddAsync(email, confirmationToken);
            }

            await _campaignInfoRepository.IncrementValue(CampaignInfoType.InvestorsConfirmed, 1);

            return true;
        }

        public async Task UpdateAsync(string email, string tokenAddress, string refundEthAddress, string refundBtcAddress)
        {
            var investor = await _investorRepository.GetAsync(email);
            var poolItem = await _addressPoolRepository.GetNextFree(email);

            await _log.WriteInfoAsync(nameof(InvestorService), nameof(UpdateAsync), $"Address pool item: {poolItem.ToJson()}");
            await _campaignInfoRepository.IncrementValue(CampaignInfoType.AddressPoolCurrentSize, -1);

            investor.TokenAddress = tokenAddress;
            investor.RefundEthAddress = refundEthAddress;
            investor.RefundBtcAddress = refundBtcAddress;
            investor.PayInEthPublicKey = poolItem.EthPublicKey;
            investor.PayInEthAddress = _ethService.GetAddressByPublicKey(poolItem.EthPublicKey);
            investor.PayInBtcPublicKey = poolItem.BtcPublicKey;
            investor.PayInBtcAddress = _btcService.GetAddressByPublicKey(poolItem.BtcPublicKey);

            await _log.WriteInfoAsync(nameof(InvestorService), nameof(UpdateAsync), $"Invertor to save: {investor.ToJson()}");
            await _investorRepository.UpdateAsync(investor);
            await _investorAttributeRepository.SaveAsync(InvestorAttributeType.PayInBtcAddress, email, investor.PayInBtcAddress);
            await _investorAttributeRepository.SaveAsync(InvestorAttributeType.PayInEthAddress, email, investor.PayInEthAddress);
            await SendSummaryEmail(investor);

            await _campaignInfoRepository.IncrementValue(CampaignInfoType.InvestorsFilledIn, 1);
        }

        private async Task SendConfirmationEmail(string email, Guid token)
        {
            var message = InvestorConfirmationMessage.Create(email, token);

            await _log.WriteInfoAsync(nameof(InvestorService), nameof(SendConfirmationEmail), $"Send InvestorConfirmationMessage: {message.ToJson()}");
            await _investorConfirmationQueuePublisher.SendAsync(message);
        }

        private async Task SendSummaryEmail(IInvestor investor)
        {
            var message = InvestorSummaryMessage.Create(investor.Email, investor.TokenAddress, investor.RefundBtcAddress,
                investor.RefundEthAddress, investor.PayInBtcAddress, investor.PayInEthAddress);

            await _log.WriteInfoAsync(nameof(InvestorService), nameof(SendSummaryEmail), $"Send InvestorSummaryMessage: {message.ToJson()}");
            await _investorSummaryQueuePublisher.SendAsync(message);
        }
    }
}
