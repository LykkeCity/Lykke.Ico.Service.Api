using Common;
using Common.Log;
using System;
using System.Threading.Tasks;
using Lykke.Service.IcoApi.Core.Services;
using Lykke.Ico.Core.Repositories.Investor;
using Lykke.Ico.Core.Repositories.InvestorAttribute;
using Lykke.Ico.Core.Helpers;
using Lykke.Ico.Core.Queues.Emails;
using Lykke.Ico.Core.Queues;
using System.Linq;

namespace Lykke.Service.IcoApi.Services
{
    public class InvestorService : IInvestorService
    {
        private readonly ILog _log;
        private readonly IInvestorRepository _investorRepository;
        private readonly IInvestorAttributeRepository _investorAttributeRepository;
        private readonly IQueuePublisher<InvestorConfirmationMessage> _investorConfirmationQueuePublisher;
        private readonly IQueuePublisher<InvestorSummaryMessage> _investorSummaryQueuePublisher;
        private readonly string _btcNetwork = "RegTest";

        public InvestorService(ILog log,
            IInvestorRepository investorRepository,
            IInvestorAttributeRepository investorAttributeRepository,
            IQueuePublisher<InvestorConfirmationMessage> investorConfirmationQueuePublisher,
            IQueuePublisher<InvestorSummaryMessage> investorSummaryQueuePublisher)
        {
            _log = log;
            _investorRepository = investorRepository;
            _investorAttributeRepository = investorAttributeRepository;
            _investorConfirmationQueuePublisher = investorConfirmationQueuePublisher;
            _investorSummaryQueuePublisher = investorSummaryQueuePublisher;
        }

        public async Task<IInvestor> GetAsync(string email)
        {
            return await _investorRepository.GetAsync(email);
        }

        public async Task RegisterAsync(string email)
        {
            var investor = await _investorRepository.GetAsync(email);
            if (investor == null)
            {
                await SendConfirmationEmail(email);
                return;
            }

            if (string.IsNullOrEmpty(investor.TokenAddress))
            {
                await ResendConfirmationEmail(investor);
                return;
            }

            await SendSummaryEmail(investor);
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

            return true;
        }

        public async Task<IInvestor> UpdateAsync(string email, string tokenAddress, string refundEthAddress, string refundBtcAddress)
        {
            var investor = await _investorRepository.GetAsync(email);

            var payInEthPublicKey = EthHelper.GeneratePublicKeys(1).First();
            var payInEthAddress = EthHelper.GetAddressByPublicKey(payInEthPublicKey);
            var payInBtcPublicKey = BtcHelper.GeneratePublicKeys(1).First();
            var payInBtcAddress = BtcHelper.GetAddressByPublicKey(payInBtcPublicKey, _btcNetwork);

            investor.TokenAddress = tokenAddress;
            investor.RefundEthAddress = refundEthAddress;
            investor.RefundBtcAddress = refundBtcAddress;
            investor.PayInEthPublicKey = payInEthPublicKey;
            investor.PayInEthAddress = payInEthAddress;
            investor.PayInBtcPublicKey = payInBtcPublicKey;
            investor.PayInBtcAddress = payInBtcAddress;

            await _investorRepository.UpdateAsync(investor);
            await SendSummaryEmail(investor);

            return investor;
        }

        public async Task DeleteAsync(string email)
        {
            await _investorRepository.RemoveAsync(email);
        }

        private async Task SendConfirmationEmail(string email)
        {
            var token = Guid.NewGuid();
            await _log.WriteInfoAsync(nameof(InvestorService), nameof(SendConfirmationEmail), $"New confirmationToken: {token.ToString()}");
            await _investorAttributeRepository.SaveAsync(InvestorAttributeType.ConfirmationToken, email, token.ToString());

            var message = InvestorConfirmationMessage.Create(email, token);
            await _log.WriteInfoAsync(nameof(InvestorService), nameof(SendConfirmationEmail), $"Send InvestorConfirmationMessage: {message.ToJson()}");
            await _investorConfirmationQueuePublisher.SendAsync(message);
        }

        private async Task ResendConfirmationEmail(IInvestor investor)
        {
            if (!investor.ConfirmationToken.HasValue)
            {
                throw new Exception($"Investor with email={investor.Email} does not have ConfirmationToken");
            }

            var message = InvestorConfirmationMessage.Create(investor.Email, investor.ConfirmationToken.Value);
            await _log.WriteInfoAsync(nameof(InvestorService), nameof(ResendConfirmationEmail), $"Resend InvestorConfirmationMessage: {message.ToJson()}");
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
