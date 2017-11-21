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
using Lykke.Ico.Core.Repositories.AddressPool;
using Lykke.Service.IcoApi.Core.Domain;

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
        private readonly IQueuePublisher<InvestorConfirmationMessage> _investorConfirmationQueuePublisher;
        private readonly IQueuePublisher<InvestorSummaryMessage> _investorSummaryQueuePublisher;

        public InvestorService(ILog log,
            IBtcService btcService,
            IEthService ethService,
            IInvestorRepository investorRepository,
            IInvestorAttributeRepository investorAttributeRepository,
            IAddressPoolRepository addressPoolRepository,
            IQueuePublisher<InvestorConfirmationMessage> investorConfirmationQueuePublisher,
            IQueuePublisher<InvestorSummaryMessage> investorSummaryQueuePublisher)
        {
            _log = log;
            _btcService = btcService;
            _ethService = ethService;
            _investorRepository = investorRepository;
            _investorAttributeRepository = investorAttributeRepository;
            _addressPoolRepository = addressPoolRepository;
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
                await SendConfirmationEmail(email);

                return RegisterResult.ConfirmationEmailSent;
            }

            if (string.IsNullOrEmpty(investor.TokenAddress))
            {
                await ResendConfirmationEmail(investor);

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

            return true;
        }

        public async Task UpdateAsync(string email, string tokenAddress, string refundEthAddress, string refundBtcAddress)
        {
            var investor = await _investorRepository.GetAsync(email);

            var poolItem = await _addressPoolRepository.GetNextFreeAsync(email);
            await _log.WriteInfoAsync(nameof(InvestorService), nameof(UpdateAsync), $"Address pool item: {poolItem.ToJson()}");

            var payInEthPublicKey = poolItem.EthPublicKey;
            var payInEthAddress = _ethService.GetAddressByPublicKey(payInEthPublicKey);
            var payInBtcPublicKey = poolItem.BtcPublicKey;
            var payInBtcAddress = _btcService.GetAddressByPublicKey(payInBtcPublicKey);

            investor.TokenAddress = tokenAddress;
            investor.RefundEthAddress = refundEthAddress;
            investor.RefundBtcAddress = refundBtcAddress;
            investor.PayInEthPublicKey = payInEthPublicKey;
            investor.PayInEthAddress = payInEthAddress;
            investor.PayInBtcPublicKey = payInBtcPublicKey;
            investor.PayInBtcAddress = payInBtcAddress;

            await _log.WriteInfoAsync(nameof(InvestorService), nameof(UpdateAsync), $"Invertor to save: {investor.ToJson()}");

            await _investorRepository.UpdateAsync(investor);
            await _investorAttributeRepository.SaveAsync(InvestorAttributeType.BtcPublicKey, email, payInEthPublicKey);
            await _investorAttributeRepository.SaveAsync(InvestorAttributeType.EthPublicKey, email, payInEthPublicKey);

            await SendSummaryEmail(investor);
        }

        public async Task DeleteAsync(string email)
        {
            var inverstor = await _investorRepository.GetAsync(email);
            if (inverstor != null)
            {
                if (inverstor.ConfirmationToken.HasValue)
                {
                    await _investorAttributeRepository.RemoveAsync(InvestorAttributeType.ConfirmationToken, inverstor.ConfirmationToken.ToString());
                }
                if (string.IsNullOrEmpty(inverstor.PayInBtcPublicKey))
                {
                    await _investorAttributeRepository.RemoveAsync(InvestorAttributeType.BtcPublicKey, inverstor.PayInBtcPublicKey);
                }
                if (string.IsNullOrEmpty(inverstor.PayInEthPublicKey))
                {
                    await _investorAttributeRepository.RemoveAsync(InvestorAttributeType.EthPublicKey, inverstor.PayInEthPublicKey);
                }

                await _investorRepository.RemoveAsync(email);
            }

            await _addressPoolRepository.RemoveAsync(email);

            // TODO
            // Remove transations
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
