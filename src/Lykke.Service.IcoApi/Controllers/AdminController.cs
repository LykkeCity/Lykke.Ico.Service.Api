using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Common;
using Common.Log;
using CsvHelper;
using Lykke.Common.ApiLibrary.Contract;
using Lykke.Service.IcoApi.Core.Domain;
using Lykke.Service.IcoApi.Core.Domain.Campaign;
using Lykke.Service.IcoApi.Core.Domain.Investor;
using Lykke.Service.IcoApi.Core.Emails;
using Lykke.Service.IcoApi.Core.Repositories;
using Lykke.Service.IcoApi.Core.Services;
using Lykke.Service.IcoApi.Core.Settings.ServiceSettings;
using Lykke.Service.IcoApi.Infrastructure;
using Lykke.Service.IcoApi.Models;
using Lykke.Service.IcoApi.Services.Extensions;
using Lykke.Service.IcoCommon.Client;
using Lykke.Service.IcoExRate.Client;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using EmailDataModel = Lykke.Service.IcoCommon.Client.Models.EmailDataModel;
using EmailTemplateAddOrUpdateRequest = Lykke.Service.IcoCommon.Client.Models.EmailTemplateAddOrUpdateRequest;
using EmailTemplateHistoryItemModel = Lykke.Service.IcoCommon.Client.Models.EmailTemplateHistoryItemModel;
using EmailTemplateModel = Lykke.Service.IcoCommon.Client.Models.EmailTemplateModel;
using CampaignSettingsCreateOrUpdateRequest = Lykke.Service.IcoCommon.Client.Models.CampaignSettingsCreateOrUpdateRequest;

namespace Lykke.Service.IcoApi.Controllers
{
    [Route("api/admin")]
    [Produces("application/json")]
    public class AdminController : Controller
    {
        private readonly ILog _log;
        private readonly IInvestorService _investorService;
        private readonly IAdminService _adminService;
        private readonly IBtcService _btcService;
        private readonly IEthService _ethService;
        private readonly IIcoExRateClient _icoExRateClient;
        private readonly IPrivateInvestorService _privateInvestorService;
        private readonly IKycService _kycService;
        private readonly ICampaignService _campaignService;
        private readonly IMemoryCache _cache;
        private readonly IcoApiSettings _settings;
        private readonly IIcoCommonServiceClient _icoCommonServiceClient;
        private readonly IAuthService _authService;
        private readonly ICampaignInfoRepository _campaignInfoRepository;

        public AdminController(ILog log, IInvestorService investorService, IAdminService adminService, 
            IBtcService btcService, IEthService ethService, IIcoExRateClient icoExRateClient,
            IPrivateInvestorService privateInvestorService, IKycService kycService,
            ICampaignService campaignService, IMemoryCache cache, IcoApiSettings settings, 
            IIcoCommonServiceClient icoCommonServiceClient, IAuthService authService,
            ICampaignInfoRepository campaignInfoRepository)
        {
            _log = log;
            _investorService = investorService;
            _adminService = adminService;
            _btcService = btcService;
            _ethService = ethService;
            _icoExRateClient = icoExRateClient;
            _privateInvestorService = privateInvestorService;
            _kycService = kycService;
            _campaignService = campaignService;
            _cache = cache;
            _settings = settings;
            _icoCommonServiceClient = icoCommonServiceClient;
            _authService = authService;
            _campaignInfoRepository = campaignInfoRepository;
        }

        /// <summary>
        /// Returns campaign info
        /// </summary>
        [AdminAuth]
        [HttpGet("campaign/info")]
        public async Task<Dictionary<string, string>> GetCampaignInfo()
        {
            var info = await _adminService.GetCampaignInfo();
            var settings = await _campaignService.GetCampaignSettings();

            if (settings != null)
            {
                info.Add("CrowdSaleTotalAmount", settings.GetCrowdSaleAmount().ToString(CultureInfo.InvariantCulture));

                var tokenInfo = await settings.GetTokenInfo(_campaignInfoRepository, DateTime.UtcNow);
                if (string.IsNullOrEmpty(tokenInfo.Error))
                {
                    info.Add("Phase", Enum.GetName(typeof(CampaignPhase), tokenInfo.Phase));
                    info.Add("PhaseTokenPriceUsd", tokenInfo.PriceUsd?.ToString(CultureInfo.InvariantCulture));
                    info.Add("PhaseTokenAmountAvailable", tokenInfo.PhaseTokenAmountAvailable?.ToString(CultureInfo.InvariantCulture));
                }
            }

            return info;
        }

        /// <summary>
        /// Returns campaign settings
        /// </summary>
        [AdminAuth]
        [HttpGet("campaign/settings")]
        public async Task<CampaignSettingsModel> GetCampaignSettings()
        {
            var settings = await _campaignService.GetCampaignSettings();
            var commonSettings = await _icoCommonServiceClient.GetCampaignSettingsAsync(Consts.CampaignId);

            return CampaignSettingsModel.Create(settings, commonSettings);
        }

        /// <summary>
        /// Returns campaign settings history
        /// </summary>
        [AdminAuth]
        [HttpGet("campaign/settings/history")]
        public async Task<CampaignSettingsHistoryItemModel[]> GetCampaignSettingsHistory()
        {
            var history = await _campaignService.GetCampaignSettingsHistory();

            return history
                .Select(item => CampaignSettingsHistoryItemModel.Create(item))
                .ToArray();
        }

        /// <summary>
        /// Save campaign settings
        /// </summary>
        [AdminAuth]
        [DisableAdminMethods]
        [HttpPost("campaign/settings")]
        public async Task<IActionResult> SaveCampaignSettings([FromBody] CampaignSettingsModel settings)
        {
            settings.PreSaleStartDateTimeUtc = settings.PreSaleStartDateTimeUtc.ToUniversalTime();
            settings.PreSaleEndDateTimeUtc = settings.PreSaleEndDateTimeUtc.ToUniversalTime();
            settings.CrowdSaleStartDateTimeUtc = settings.CrowdSaleStartDateTimeUtc.ToUniversalTime();
            settings.CrowdSaleEndDateTimeUtc = settings.CrowdSaleEndDateTimeUtc.ToUniversalTime();

            await _log.WriteInfoAsync(nameof(AdminController), nameof(SaveCampaignSettings),
               $"settings={settings.ToJson()}", "Save campaign settings");

            await _campaignService.SaveCampaignSettings(settings, User.Identity.Name);

            if (settings.CommonSettings != null)
            {
                await _icoCommonServiceClient.CreateOrUpdateCampaignSettingsAsync(Consts.CampaignId, 
                    new CampaignSettingsCreateOrUpdateRequest
                    {
                        CampaignSettings = settings.CommonSettings,
                        Username = User.Identity.Name
                    });
            }

            return Ok();
        }

        /// <summary>
        /// Returns investor info
        /// </summary>
        [AdminAuth]
        [HttpGet("investors/{email}")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetInvestor([Required] string email)
        {
            var investor = await _investorService.GetAsync(email);
            if (investor == null)
            {
                return NotFound(ErrorResponse.Create("Investor not found"));
            }

            return Ok(FullInvestorResponse.Create(investor));
        }

        /// <summary>
        /// Update investor token and refunds addresses
        /// </summary>
        [AdminAuth]
        [DisableAdminMethods]
        [HttpPost("investors/{email}")]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> UpdateInvestor([Required] string email, [FromBody] InvestorRequest model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            if (!_ethService.ValidateAddress(model.TokenAddress))
            {
                return BadRequest($"The token address={model.TokenAddress} is invalid IRC20 address");
            }
            if (!string.IsNullOrEmpty(model.RefundEthAddress) && !_ethService.ValidateAddress(model.RefundEthAddress))
            {
                return BadRequest($"The refund ETH address={model.RefundEthAddress} is invalid ETH address");
            }
            if (!string.IsNullOrEmpty(model.RefundBtcAddress) && !_btcService.ValidateAddress(model.RefundBtcAddress))
            {
                return BadRequest($"The refund BTC address={model.RefundBtcAddress} is invalid BTC address");
            }

            var investor = await _investorService.GetAsync(email);
            if (investor == null)
            {
                return NotFound(ErrorResponse.Create("Investor not found"));
            }

            await _log.WriteInfoAsync(nameof(AdminController), nameof(UpdateInvestor),
               $"email={email}, model={model.ToJson()}", "Update investor addresses");

            await _adminService.UpdateInvestorAsync(email, model.TokenAddress,
                model.RefundEthAddress, model.RefundBtcAddress);

            return Ok();
        }

        /// <summary>
        /// Returns investors invested more than {amount} USD
        /// </summary>
        [AdminAuth]
        [HttpGet("investors/amountUsd/moreThan/{amount}")]
        public async Task<IEnumerable<FullInvestorResponse>> GetInvestorsInvestedMoreThanAmountUsd([Required] decimal amount)
        {
            var cachKey = $"AllInvestorsWithAmountUsdMoreThen-{amount}";

            if (!_cache.TryGetValue(cachKey, out IEnumerable<IInvestor> investors))
            {
                var cacheEntryOptions = new MemoryCacheEntryOptions()
                    .SetSlidingExpiration(TimeSpan.FromMinutes(5));

                investors = await _adminService.GetAllInvestors();
                investors = investors.Where(f => f.AmountUsd > amount).OrderByDescending(f => f.AmountUsd);

                _cache.Set(cachKey, investors, cacheEntryOptions);
            }

            return investors.Select(f => FullInvestorResponse.Create(f));
        }

        /// <summary>
        /// Returns the history of investor profile changes
        /// </summary>
        [AdminAuth]
        [HttpGet("investors/{email}/history")]
        public async Task<InvestorHistoryResponse> GetInvestorHistory([Required] string email)
        {
            return InvestorHistoryResponse.Create(await _adminService.GetInvestorHistory(email));
        }

        /// <summary>
        /// Returns the list of emails sent to investor
        /// </summary>
        [AdminAuth]
        [HttpGet("investors/{email}/emails")]
        public async Task<InvestorEmailsResponse> GetInvestorEmails([Required] string email)
        {
            email = email.ToLowCase();

            var emails = await _icoCommonServiceClient.GetSentEmailsAsync(email, Consts.CampaignId);

            return InvestorEmailsResponse.Create(emails);
        }

        /// <summary>
        /// Returns the list of transactions maid by investor
        /// </summary>
        [AdminAuth]
        [HttpGet("investors/{email}/transactions")]
        public async Task<InvestorTransactionsResponse> GetInvestorTransactions([Required] string email)
        {
            return InvestorTransactionsResponse.Create(await _adminService.GetInvestorTransactions(email));
        }

        /// <summary>
        /// Returns the list of failed transactions for investor
        /// </summary>
        [AdminAuth]
        [HttpGet("investors/{email}/transactions/failed")]
        public async Task<InvestorFailedTransactionsResponse> GetInvestorFailedTransactions([Required] string email)
        {
            return InvestorFailedTransactionsResponse.Create(await _adminService.GetInvestorRefunds(email));
        }

        /// <summary>
        /// Get private investor
        /// </summary>
        [AdminAuth]
        [HttpGet("investors/private")]
        [ProducesResponseType(typeof(PrivateInvestorResponse), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetPrivateInvestor([Required] string email)
        {
            var investor = await _privateInvestorService.GetAsync(email);
            if (investor == null)
            {
                return NotFound();
            }

            var kycLink = await _kycService.GetKycLink(investor.Email, investor.KycRequestId);
            var response = PrivateInvestorResponse.Create(investor, kycLink);

            return Ok(response);
        }

        /// <summary>
        /// Create private investor and genarates KYC link
        /// </summary>
        [AdminAuth]
        [DisableAdminMethods]
        [HttpPost("investors/private")]
        [ProducesResponseType(typeof(PrivateInvestorResponse), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> CreatePrivateInvestor([FromBody, Required] CreatePrivateInvestorRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var investor = await _privateInvestorService.GetAsync(request.Email);
            if (investor == null)
            {
                await _privateInvestorService.CreateAsync(request.Email);
                await _privateInvestorService.RequestKycAsync(request.Email);

                investor = await _privateInvestorService.GetAsync(request.Email);
            }

            var kycLink = await _kycService.GetKycLink(investor.Email, investor.KycRequestId);
            var response = PrivateInvestorResponse.Create(investor, kycLink);

            return Ok(response);
        }

        /// <summary>
        /// Returns the list of all failed transactions
        /// </summary>
        [AdminAuth]
        [HttpGet("transactions/failed")]
        public async Task<InvestorFailedTransactionsResponse> GetFailedTransactions()
        {
            return InvestorFailedTransactionsResponse.Create(await _adminService.GetRefunds());
        }

        /// <summary>
        /// Returns the latest transactions
        /// </summary>
        [AdminAuth]
        [HttpGet("transactions/latest")]
        public async Task<InvestorTransactionsResponse> GetLatestTransactions()
        {
            return InvestorTransactionsResponse.Create(await _adminService.GetLatestTransactions());
        }

        /// <summary>
        /// Returns all investors in csv format
        /// </summary>
        [AdminAuth]
        [HttpGet("reports/csv/investors")]
        public async Task<FileContentResult> GetAllInvestorsCsv()
        {
            var investors = await _adminService.GetAllInvestors();
            var records = investors.Select(f => FullInvestorResponse.Create(f));

            using (var writer = new StringWriter())
            {
                var scv = new CsvWriter(writer);

                scv.WriteRecords(records);

                return File(Encoding.UTF8.GetBytes(writer.ToString()), "text/csv", "investors.csv");
            }
        }

        /// <summary>
        /// Returns all transactions in csv format
        /// </summary>
        [AdminAuth]
        [HttpGet("reports/csv/transactions")]
        public async Task<FileContentResult> GetAllTransactionsCsv()
        {
            var txs = await _adminService.GetAllInvestorTransactions();
            var response = InvestorTransactionsResponse.Create(txs);

            using (var writer = new StringWriter())
            {
                var scv = new CsvWriter(writer);

                scv.WriteRecords(response.Transactions);

                return File(Encoding.UTF8.GetBytes(writer.ToString()), "text/csv", "transactions.csv");
            }
        }

        /// <summary>
        /// Returns all failed transactions in csv format
        /// </summary>
        [AdminAuth]
        [HttpGet("reports/csv/transactions/failed")]
        public async Task<FileContentResult> GetAllFailedTransactionsCsv()
        {
            var txs = await _adminService.GetAllInvestorFailedTransactions();
            var response = InvestorFailedTransactionsResponse.Create(txs);

            using (var writer = new StringWriter())
            {
                var scv = new CsvWriter(writer);

                scv.WriteRecords(response.Items);

                return File(Encoding.UTF8.GetBytes(writer.ToString()), "text/csv", "transactions-failed.csv");
            }
        }

        /// <summary>
        /// Returns the list of public keys.
        /// </summary>
        /// <remarks>
        /// sample: /api/pool/keys/1,2,100
        /// </remarks>
        [AdminAuth]
        [HttpGet("pool/keys/{ids}")]
        public async Task<PoolKeysResponse> GetPoolPublicKeys([Required] string ids)
        {
            var response = new PoolKeysResponse();
            var array = ids.Split(',').Select(f => Int32.Parse(f)).ToArray();
            var keys = await _adminService.GetPublicKeys(array);

            response.Keys.AddRange(keys.Select(f => new PoolKeysModel
            {
                Id = f.Id,
                BtcPublicKey = f.BtcPublicKey,
                EthPublicKey = f.EthPublicKey
            }));

            return response;
        }

        /// <summary>
        /// Imports the scv file with public keys
        /// </summary>
        [AdminAuth]
        [DisableAdminMethods]
        [HttpPost("pool/import")]
        [DisableRequestSizeLimit]
        public async Task ImportKeys([FromForm] IFormFile file)
        {
            using (var reader = new StreamReader(file.OpenReadStream()))
            {
                await _adminService.ImportPublicKeys(reader);
            }
        }

        /// <summary>
        /// Returns campaign email templates content.
        /// </summary>
        /// <returns></returns>
        [AdminAuth]
        [HttpGet("campaign/email/templates")]
        public async Task<IList<EmailTemplateDataModel>> GetCampaignEmailTemplates()
        {
            var templates = await _icoCommonServiceClient.GetCampaignEmailTemplatesAsync(Consts.CampaignId);

            // add data model to templates, if any exists 
            var templateModels = templates
                .Select(t => new EmailTemplateDataModel(t)
                {
                    Data = EmailDataAttribute.DataModels.TryGetValue(t.TemplateId, out var data) ?
                        JObject.FromObject(data) :
                        null
                })
                .ToList();

            // add templates for new data models
            foreach (var templateId in EmailDataAttribute.DataModels.Keys)
            {
                if (templateModels.All(t => t.TemplateId != templateId))
                {
                    templateModels.Add(new EmailTemplateDataModel()
                    {
                        CampaignId = Consts.CampaignId,
                        TemplateId = templateId,
                        Data = JObject.FromObject(EmailDataAttribute.DataModels[templateId])
                    });
                }
            }

            // add layout, if there is no any
            if (templateModels.All(x => !x.IsLayout))
            {
                templateModels.Add(new EmailTemplateDataModel()
                {
                    CampaignId = Consts.CampaignId,
                    TemplateId = "layout",
                    IsLayout = true
                });
            }

            return templateModels;
        }

        /// <summary>
        /// Returns email template history.
        /// </summary>
        /// <returns></returns>
        [AdminAuth]
        [HttpGet("campaign/email/templates/{templateId}/history")]
        public async Task<IList<EmailTemplateHistoryItemModel>> GetEmailTemplateHistory([FromRoute] string templateId)
        {
            return await _icoCommonServiceClient.GetEmailTemplateHistoryAsync(Consts.CampaignId, templateId);
        }

        /// <summary>
        /// Returns campaign email templates content.
        /// </summary>
        /// <returns></returns>
        [AdminAuth]
        [HttpPost("campaign/email/templates")]
        public async Task<IActionResult> AddOrUpdateCampaignEmailTemplate([FromBody] EmailTemplateModel emailTemplate)
        {
            if (string.IsNullOrEmpty(emailTemplate.TemplateId))
            {
                return BadRequest(ErrorResponse.Create("TemplateId is required"));
            }

            emailTemplate.CampaignId = Consts.CampaignId;

            await _log.WriteInfoAsync(nameof(AdminController), nameof(AddOrUpdateCampaignEmailTemplate),
                $"emailTemplate={emailTemplate.ToJson()}", "Admin UI - Edit campaign email template");

            await _icoCommonServiceClient.AddOrUpdateEmailTemplateAsync(new EmailTemplateAddOrUpdateRequest(emailTemplate, User.Identity.Name));

            return Ok();
        }

        /// <summary>
        /// Sends an e-mail.
        /// </summary>
        /// <param name="emailData"></param>
        /// <returns></returns>
        [AdminAuth]
        [HttpPost("campaign/email")]
        public async Task<IActionResult> SendEmail([FromBody] EmailDataModel emailData)
        {
            if (string.IsNullOrEmpty(emailData.TemplateId))
            {
                return BadRequest(ErrorResponse.Create("TemplateId is required"));
            }

            if (string.IsNullOrEmpty(emailData.To))
            {
                return BadRequest(ErrorResponse.Create("To is required"));
            }

            emailData.CampaignId = Consts.CampaignId;

            await _log.WriteInfoAsync(nameof(AdminController), nameof(SendEmail),
                $"emailData={emailData.ToJson()}", "Admin UI - Send test email");

            await _icoCommonServiceClient.SendEmailAsync(emailData);

            return Ok();
        }

        /// <summary>
        /// Checks user name and password and returns authentication token.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ErrorResponseFactory.Create(ModelState));
            }

            var authToken = await _authService.Login(request.Username, request.Password);

            if (string.IsNullOrEmpty(authToken))
            {
                return StatusCode(StatusCodes.Status403Forbidden);
            }
            else
            {
                await _log.WriteInfoAsync(nameof(AdminController), nameof(Login),
                    $"request.Username={request.Username}", "Admin UI - login");

                return Ok(authToken);
            }
        }

        /// <summary>
        /// Generates Shared Access Signature URL for adding transaction messages to queue.
        /// </summary>
        /// <returns></returns>
        [AdminAuth]
        [HttpPost("transactions/sas")]
        public string GenerateTransactionQueueSasUrl([FromBody] GenerateTransactionQueueSasUrlRequest request)
        {
            return _adminService.GenerateTransactionQueueSasUrl(request?.ExpiryTime);
        }

        /// <summary>
        /// Get investors list to whom KYC reminder will be sent
        /// </summary>
        [AdminAuth]
        [HttpGet("investors/send/kycReminderEmails/{type}")]
        public async Task<SendKycReminderEmailsResponse> GetKycReminders([Required] KycReminderType type)
        {
            var investorsToSend = await GetKycReminderInvestors(type);

            return SendKycReminderEmailsResponse.Create(investorsToSend);
        }

        /// <summary>
        /// Sends KYC reminder emails
        /// </summary>
        [AdminAuth]
        [HttpPost("investors/send/kycReminderEmails/{type}")]
        public async Task<IActionResult> SendKycReminderEmails([Required] string confirmation, [Required] KycReminderType type)
        {
            if (confirmation != "confirm")
            {
                return BadRequest($"The confirmation={confirmation} is not valid");
            }

            var investorsToSend = await GetKycReminderInvestors(type);

            await _log.WriteInfoAsync(nameof(AdminController), nameof(SendKycReminderEmails),
                $"type={Enum.GetName(typeof(KycReminderType), type)}",
                $"Send kyc reminder emails to {investorsToSend.Count()} investors");

            await _adminService.SendKycReminderEmails(investorsToSend);

            return Ok(SendKycReminderEmailsResponse.Create(investorsToSend));
        }

        private async Task<IEnumerable<IInvestor>> GetKycReminderInvestors(KycReminderType type)
        {
            var investors = await _adminService.GetAllInvestors();

            switch (type)
            {
                case KycReminderType.NotCompletedKyc:
                    return investors.Where(f => !string.IsNullOrEmpty(f.KycRequestId) && !f.KycPassed.HasValue);
                case KycReminderType.FailedKyc:
                    return investors.Where(f => !string.IsNullOrEmpty(f.KycRequestId) &&
                        f.KycPassed.HasValue && !f.KycPassed.Value);
                default:
                    throw new Exception("Not supported KycReminderType");
            }
        }
    }
}
