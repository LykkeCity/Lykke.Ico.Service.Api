using Common;
using Common.Log;
using System;
using System.Threading.Tasks;
using Lykke.Service.IcoApi.Core.Services;
using Lykke.Service.IcoApi.Core.Domain;
using Lykke.Service.IcoApi.Core.Settings.ServiceSettings;
using Lykke.Service.IcoCommon.Client;
using Lykke.Service.IcoCommon.Client.Models;
using Lykke.Service.IcoApi.Core.Repositories;
using Lykke.Service.IcoApi.Core.Queues;
using Lykke.Service.IcoApi.Core.Queues.Emails;
using Lykke.Service.IcoApi.Core.Domain.Investor;
using Lykke.Service.IcoApi.Core.Domain.Campaign;

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
        private readonly IIcoCommonServiceClient _icoCommonServiceClient;
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
            IIcoCommonServiceClient icoCommonServiceClient,
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
            _icoCommonServiceClient = icoCommonServiceClient;
            _investorConfirmationQueuePublisher = investorConfirmationQueuePublisher;
            _investorSummaryQueuePublisher = investorSummaryQueuePublisher;
            _icoApiSettings = icoApiSettings;
        }

        public async Task<IInvestor> GetAsync(string email)
        {
            email = email.ToLowCase();

            return await _investorRepository.GetAsync(email);
        }

        public async Task<string> GetEmailByKycId(Guid kycId)
        {
            return await _investorAttributeRepository.GetInvestorEmailAsync(
                InvestorAttributeType.KycId,
                kycId.ToString());
        }

        public async Task<RegisterResult> RegisterAsync(string email)
        {
            email = email.ToLowCase();

            var investor = await _investorRepository.GetAsync(email);
            if (investor == null)
            {
                var token = Guid.NewGuid();

                await _log.WriteInfoAsync(nameof(InvestorService), nameof(RegisterAsync), 
                    $"email={email}, token={token}", "Create investor");

                await _investorRepository.AddAsync(email, token);
                await _investorAttributeRepository.SaveAsync(InvestorAttributeType.ConfirmationToken, 
                    email, token.ToString());
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
            var email = await _investorAttributeRepository.GetInvestorEmailAsync(
                InvestorAttributeType.ConfirmationToken, 
                confirmationToken.ToString());
            if (string.IsNullOrEmpty(email))
            {
                await _log.WriteInfoAsync(nameof(InvestorService), nameof(ConfirmAsync), 
                    $"confirmationToken={confirmationToken}", "Token was not found");

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
            email = email.ToLowCase();

            var poolItem = await _addressPoolRepository.GetNextFree(email);

            await _log.WriteInfoAsync(nameof(InvestorService), nameof(UpdateAsync), 
                $"poolItem={poolItem.ToJson()}", "Retrieved address pool item");
            await _campaignInfoRepository.IncrementValue(CampaignInfoType.AddressPoolCurrentSize, -1);

            var payInEthPublicKey = poolItem.EthPublicKey;
            var payInEthAddress = _ethService.GetAddressByPublicKey(poolItem.EthPublicKey);
            var payInBtcPublicKey = poolItem.BtcPublicKey;
            var payInBtcAddress = _btcService.GetAddressByPublicKey(poolItem.BtcPublicKey);

            await _log.WriteInfoAsync(nameof(InvestorService), nameof(UpdateAsync), 
                $"email={email}, tokenAddress={tokenAddress}, refundEthAddress={refundEthAddress}, " +
                $"refundBtcAddress={refundBtcAddress}, payInEthPublicKey={payInEthPublicKey}, payInEthAddress={payInEthAddress}, " +
                $"payInBtcPublicKey={payInBtcPublicKey}, payInBtcAddress={payInBtcAddress}",
                "Update investor data");
            await _investorRepository.SaveAddressesAsync(email, tokenAddress, refundEthAddress, refundBtcAddress,
                payInEthPublicKey, payInEthAddress, payInBtcPublicKey, payInBtcAddress);

            await _investorAttributeRepository.SaveAsync(InvestorAttributeType.PayInBtcAddress, email, payInBtcAddress);
            await _investorAttributeRepository.SaveAsync(InvestorAttributeType.PayInEthAddress, email, payInEthAddress);
            await _campaignInfoRepository.IncrementValue(CampaignInfoType.InvestorsFilledIn, 1);

            await _icoCommonServiceClient.AddPayInAddressAsync(new PayInAddressModel
            {
                Address = payInBtcAddress,
                CampaignId = _icoApiSettings.CampaignId,
                Currency = IcoCommon.Client.Models.CurrencyType.BTC,
                Email = email
            });

            await _icoCommonServiceClient.AddPayInAddressAsync(new PayInAddressModel
            {
                Address = payInEthAddress,
                CampaignId = _icoApiSettings.CampaignId,
                Currency = IcoCommon.Client.Models.CurrencyType.ETH,
                Email = email
            });

            var investor = await _investorRepository.GetAsync(email);
            await SendSummaryEmail(investor);
        }

        public async Task SaveKycResultAsync(string email, string kycStatus)
        {
            email = email.ToLowCase();

            var kycPassed = kycStatus.ToString().ToUpper() == "OK";

            await _investorRepository.SaveKycResultAsync(email, kycPassed);

            if (kycPassed)
            {
                await _campaignInfoRepository.IncrementValue(CampaignInfoType.InvestorsKycPassed, 1);
            }
        }

        private async Task SendConfirmationEmail(string email, Guid token)
        {
            var message = new InvestorConfirmationMessage
            {
                EmailTo = email,
                ConfirmationLink = _icoApiSettings.SiteEmailConfirmationPageUrl.Replace("{token}", token.ToString())
            };

            await _log.WriteInfoAsync(nameof(InvestorService), nameof(SendConfirmationEmail), 
                $"message={message.ToJson()}", "Send investor confirmation message");
            await _investorConfirmationQueuePublisher.SendAsync(message);
        }

        private async Task SendSummaryEmail(IInvestor investor)
        {
            var message = new InvestorSummaryMessage
            {
                EmailTo = investor.Email,
                LinkToSummaryPage = _icoApiSettings.SiteSummaryPageUrl.Replace("{token}", investor.ConfirmationToken.Value.ToString()),
                TokenAddress = investor.TokenAddress,
                RefundBtcAddress = investor.RefundBtcAddress,
                RefundEthAddress = investor.RefundEthAddress,
                LinkBtcAddress = $"{_icoApiSettings.BtcTrackerUrl}address",
                LinkEthAddress = $"{_icoApiSettings.EthTrackerUrl}address"
            };

            await _log.WriteInfoAsync(nameof(InvestorService), nameof(SendSummaryEmail), 
                $"message={message.ToJson()}", "Send investor summary message");
            await _investorSummaryQueuePublisher.SendAsync(message);
        }
    }
}
