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
using Lykke.Service.IcoApi.Core.Emails;
using Lykke.Service.IcoApi.Core.Domain.Investor;
using Lykke.Service.IcoApi.Core.Domain.Campaign;
using Lykke.Service.IcoApi.Core.Queues.Messages;
using Lykke.Service.IcoApi.Core.Queues;
using Lykke.Service.IcoApi.Services.Extensions;

namespace Lykke.Service.IcoApi.Services
{
    public class InvestorService : IInvestorService
    {
        private readonly ILog _log;
        private readonly ICampaignService _campaignService;
        private readonly IInvestorRepository _investorRepository;
        private readonly IInvestorAttributeRepository _investorAttributeRepository;
        private readonly IAddressPoolRepository _addressPoolRepository;
        private readonly ICampaignInfoRepository _campaignInfoRepository;
        private readonly IIcoCommonServiceClient _icoCommonServiceClient;
        private readonly IQueuePublisher<InvestorMessage> _investorPublisher;
        private readonly IQueuePublisher<TransactionMessage> _transactionQueuePublisher;
        private readonly IcoApiSettings _icoApiSettings;

        public InvestorService(ILog log,
            ICampaignService campaignService,
            IInvestorRepository investorRepository,
            IInvestorAttributeRepository investorAttributeRepository,
            IAddressPoolRepository addressPoolRepository,
            IAddressPoolHistoryRepository addressPoolHistoryRepository,
            ICampaignInfoRepository campaignInfoRepository,
            IIcoCommonServiceClient icoCommonServiceClient,
            IQueuePublisher<InvestorMessage> investorPublisher,
            IQueuePublisher<TransactionMessage> transactionQueuePublisher,
            IcoApiSettings icoApiSettings)
        {
            _log = log;
            _campaignService = campaignService;
            _investorRepository = investorRepository;
            _investorAttributeRepository = investorAttributeRepository;
            _addressPoolRepository = addressPoolRepository;
            _campaignInfoRepository = campaignInfoRepository;
            _icoCommonServiceClient = icoCommonServiceClient;
            _investorPublisher = investorPublisher;
            _transactionQueuePublisher = transactionQueuePublisher;
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
                var settings = await _campaignService.GetCampaignSettings();
                var phase = settings.GetPhaseString(DateTime.UtcNow);

                var token = Guid.NewGuid();

                await _log.WriteInfoAsync(nameof(InvestorService), nameof(RegisterAsync), 
                   new { email, token, phase }.ToJson(), "Create investor");

                await _investorRepository.AddAsync(email, token, phase);
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

            await _investorRepository.SaveAddressesAsync(email, tokenAddress, refundEthAddress, refundBtcAddress);
            await _investorPublisher.SendAsync(new InvestorMessage { Email = email });
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
            var message = new Confirmation
            {
                AuthToken = token.ToString()
            };

            await _icoCommonServiceClient.SendEmailAsync(new EmailDataModel
            {
                To = email,
                TemplateId = "confirmation",
                CampaignId = Consts.CAMPAIGN_ID,
                Data = message
            });
        }

        private async Task SendSummaryEmail(IInvestor investor)
        {
            var message = new Summary
            {
                AuthToken = investor.ConfirmationToken.Value.ToString(),
                TokenAddress = investor.TokenAddress,
                RefundBtcAddress = investor.RefundBtcAddress,
                RefundEthAddress = investor.RefundEthAddress
            };

            await _icoCommonServiceClient.SendEmailAsync(new EmailDataModel
            {
                To = investor.Email,
                TemplateId = "summary",
                CampaignId = Consts.CAMPAIGN_ID,
                Data = message
            });
        }

        public async Task SendFiatTransaction(string email, string transactionId, TxType txType, decimal amount, decimal fee)
        {
            var message = new TransactionMessage
            {
                Email = email,
                Amount = amount,
                CreatedUtc = DateTime.UtcNow,
                Currency = Core.Domain.CurrencyType.Fiat,
                Type = txType,
                Fee = fee,
                TransactionId = transactionId,
                UniqueId = transactionId
            };

            await _log.WriteInfoAsync(nameof(InvestorService), nameof(SendFiatTransaction),
                $"message={message.ToJson()}", "Transaction message to send");

            try
            {
                await _transactionQueuePublisher.SendAsync(message);
            }
            catch (Exception ex)
            {
                await _log.WriteErrorAsync(nameof(InvestorService), nameof(SendFiatTransaction),
                    $"Failed to send transaction message", ex);

                throw;
            }
        }
    }
}
