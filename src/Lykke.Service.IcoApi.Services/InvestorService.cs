using Lykke.Service.IcoApi.Core.Services;
using System.Threading.Tasks;
using Lykke.Ico.Core.Repositories.Investor;
using Lykke.Ico.Core.Contracts.Queues;
using Lykke.Ico.Core.Services;
using Common.Log;
using Lykke.Ico.Core.Contracts.Repositories;
using Common;
using Lykke.Ico.Core.Repositories.InvestorToken;
using System;

namespace Lykke.Service.IcoApi.Services
{
    public class InvestorService : IInvestorService
    {
        private readonly ILog _log;
        private readonly IInvestorRepository _investorRepository;
        private readonly IInvestorConfirmationRepository _investorConfirmationRepository;
        private readonly IQueuePublisher<InvestorConfirmationMessage> _investorConfirmationQueuePublisher;
        private readonly IQueuePublisher<InvestorSummaryMessage> _investorSummaryQueuePublisher;


        public InvestorService(ILog log,
            IInvestorRepository investorRepository,
            IInvestorConfirmationRepository investorConfirmationRepository,
            IQueuePublisher<InvestorConfirmationMessage> investorConfirmationQueuePublisher,
            IQueuePublisher<InvestorSummaryMessage> investorSummaryQueuePublisher)
        {
            _log = log;
            _investorRepository = investorRepository;
            _investorConfirmationRepository = investorConfirmationRepository;
            _investorConfirmationQueuePublisher = investorConfirmationQueuePublisher;
            _investorSummaryQueuePublisher = investorSummaryQueuePublisher;
        }

        public async Task<IInvestor> GetAsync(string email)
        {
            return await _investorRepository.GetAsync(email);
        }

        public async Task<IInvestorConfirmation> GetConfirmation(Guid confirmationToken)
        {
            return await _investorConfirmationRepository.GetAsync(confirmationToken);
        }

        public async Task RegisterAsync(string email, string ipAddress)
        {
            await _log.WriteInfoAsync(nameof(InvestorService), nameof(RegisterAsync), $"Register investor with email={email} from ip={ipAddress}");

            var investor = await _investorRepository.GetAsync(email);
            if (investor == null)
            {
                await SendConfirmationEmail(email, ipAddress);
                return;
            }

            await _log.WriteInfoAsync(nameof(InvestorService), nameof(RegisterAsync), $"Exisitng investor: {investor.ToJson()}");

            if (string.IsNullOrEmpty(investor.TokenAddress))
            {
                await ResendConfirmationEmail(email, ipAddress);
                return;
            }

            await SendSummaryEmail(investor);
        }

        public async Task<bool> ConfirmAsync(Guid confirmationToken, string ipAddress)
        {
            await _log.WriteInfoAsync(nameof(InvestorService), nameof(RegisterAsync), $"Confirm investor with token={confirmationToken} from ip={ipAddress}");

            var confimation = await _investorConfirmationRepository.GetAsync(confirmationToken);
            if (confimation == null)
            {
                await _log.WriteInfoAsync(nameof(InvestorService), nameof(ConfirmAsync), $"Token {confirmationToken} is not found");
                return false;
            }

            await _investorRepository.AddAsync(confimation.Email, ipAddress);
            return true;
        }

        public async Task UpdateAddressesAsync(string email, string tokenAddress, string refundEthAddress, string refundBtcAddress)
        {
            await _investorRepository.UpdateAddressesAsync(email, tokenAddress, refundEthAddress, refundBtcAddress);
        }

        public async Task UpdatePayInAddressesAsync(string email, string payInEthPublicKey, string payInBtcPublicKey)
        {
            await _investorRepository.UpdatePayInAddressesAsync(email, payInEthPublicKey, payInBtcPublicKey);
        }

        public async Task DeleteAsync(string email)
        {
            await _investorRepository.RemoveAsync(email);
        }

        private async Task SendConfirmationEmail(string email, string ipAddress)
        {
            var confirmationToken = await _investorConfirmationRepository.AddAsync(email, ipAddress);
            await _log.WriteInfoAsync(nameof(InvestorService), nameof(RegisterAsync), $"New confirmationToken: {confirmationToken.ToJson()}");

            var message = InvestorConfirmationMessage.Create(email, confirmationToken.ConfirmationToken);

            await _log.WriteInfoAsync(nameof(InvestorService), nameof(RegisterAsync), $"Send InvestorConfirmationMessage: {message.ToJson()}");
            await _investorConfirmationQueuePublisher.SendAsync(message);
        }

        private async Task ResendConfirmationEmail(string email, string ipAddress)
        {
            var confirmationToken = await _investorConfirmationRepository.GetAsync(email);
            if (confirmationToken == null)
            {
                confirmationToken = await _investorConfirmationRepository.AddAsync(email, ipAddress);
            }

            var message = InvestorConfirmationMessage.Create(email, confirmationToken.ConfirmationToken);

            await _log.WriteInfoAsync(nameof(InvestorService), nameof(RegisterAsync), $"TokenAddress is empty. Send InvestorConfirmationMessage: {message.ToJson()}");
            await _investorConfirmationQueuePublisher.SendAsync(message);
        }

        private async Task SendSummaryEmail(IInvestor investor)
        {
            var message = InvestorSummaryMessage.Create(investor.Email, investor.TokenAddress, investor.RefundBtcAddress,
                investor.RefundEthAddress, investor.PayInBtcPublicKey, investor.PayInEthPublicKey);

            await _log.WriteInfoAsync(nameof(InvestorService), nameof(RegisterAsync), $"Send InvestorSummaryMessage: {message.ToJson()}");
            await _investorSummaryQueuePublisher.SendAsync(message);
        }
    }
}
