using Common;
using Common.Log;
using CsvHelper;
using Lykke.Ico.Core;
using Lykke.Ico.Core.Repositories.CampaignInfo;
using Lykke.Ico.Core.Repositories.CampaignSettings;
using Lykke.Ico.Core.Repositories.Investor;
using Lykke.Ico.Core.Services;
using Lykke.Service.IcoApi.Core.Services;
using Lykke.Service.IcoApi.Infrastructure;
using Lykke.Service.IcoApi.Models;
using Lykke.Service.IcoExRate.Client;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

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


        public AdminController(ILog log, IInvestorService investorService, IAdminService adminService, 
            IBtcService btcService, IEthService ethService, IIcoExRateClient icoExRateClient,
            IPrivateInvestorService privateInvestorService, IKycService kycService,
            ICampaignService campaignService, IMemoryCache cache)
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

            var key = nameof(CampaignInfoType.AmountInvestedToken);
            var tokensSoldStr = info.ContainsKey(key) ? info[key] : "";
            if (!Decimal.TryParse(tokensSoldStr, out var tokensSold))
            {
                tokensSold = 0;
            }

            var tokenInfo = settings.GetTokenInfo(tokensSold, DateTime.UtcNow);
            if (tokenInfo != null)
            {
                info.Add("TokenPriceUsd", tokenInfo.Price.ToString());
                info.Add("Phase", Enum.GetName(typeof(TokenPricePhase), tokenInfo.Phase));
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

            return CampaignSettingsModel.Create(settings);
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

            await _campaignService.SaveCampaignSettings(settings);

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
        /// Update investor KYC status
        /// </summary>
        [AdminAuth]
        [DisableAdminMethods]
        [HttpPost("investors/{email}/kyc/{status}")]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> UpdateInvestorKycStatus([Required] string email, [Required] KycStatusManual status)
        {
            var investor = await _investorService.GetAsync(email);
            if (investor == null)
            {
                return NotFound(ErrorResponse.Create("Investor not found"));
            }

            await _log.WriteInfoAsync(nameof(AdminController), nameof(UpdateInvestorKycStatus),
               $"email={email}, status={Enum.GetName(typeof(KycStatusManual), status)}", 
               "Update investor kyc status");

            bool? kycPassed = null;
            if (status == KycStatusManual.Success)
            {
                kycPassed = true;
            }
            if (status == KycStatusManual.Failed)
            {
                kycPassed = false;
            }

            await _adminService.UpdateInvestorKycAsync(investor, kycPassed);

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
        /// Returns private or regular investor email by KycId
        /// </summary>
        [AdminAuth]
        [HttpGet("investors/kycId/equals/{kycId}")]
        public async Task<IActionResult> GetInvestorByKycId([Required] Guid kycId)
        {
            var privateInvestorEmail = await _privateInvestorService.GetEmailByKycId(kycId);
            if (!string.IsNullOrEmpty(privateInvestorEmail))
            {
                return Ok(new { type = "private", email = privateInvestorEmail });
            }

            var investorEmail = await _investorService.GetEmailByKycId(kycId);
            if (!string.IsNullOrEmpty(investorEmail))
            {
                return Ok(new { type = "regular", email = investorEmail });
            }

            return NotFound();
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
            return InvestorEmailsResponse.Create(await _adminService.GetInvestorEmails(email));
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
        /// Update private investor KYC status
        /// </summary>
        [AdminAuth]
        [DisableAdminMethods]
        [HttpPost("investors/private/{email}/kyc/{status}")]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> UpdatePrivateInvestorKycStatus([Required] string email, [Required] KycStatusManual status)
        {
            var investor = await _privateInvestorService.GetAsync(email);
            if (investor == null)
            {
                return NotFound(ErrorResponse.Create("Private investor not found"));
            }

            await _log.WriteInfoAsync(nameof(AdminController), nameof(UpdatePrivateInvestorKycStatus),
               $"email={email}, status={Enum.GetName(typeof(KycStatusManual), status)}",
               "Update private investor kyc status");

            bool? kycPassed = null;
            if (status == KycStatusManual.Success)
            {
                kycPassed = true;
            }
            if (status == KycStatusManual.Failed)
            {
                kycPassed = false;
            }

            await _privateInvestorService.SaveManualKycResultAsync(investor.Email, kycPassed);

            return Ok();
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
        /// Resends the all failed transactions to queue again
        /// </summary>
        [AdminAuth]
        [HttpPost("transactions/failed/resend")]
        public async Task ResendFailedTransactions()
        {
            await _adminService.ResendRefunds();
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

        /// <summary>
        /// Returns all investors in scv format
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
        /// Returns all private investors in scv format
        /// </summary>
        [AdminAuth]
        [HttpGet("reports/csv/investors/private")]
        public async Task<FileContentResult> GetAllPrivateInvestorsCsv()
        {
            var investors = await _privateInvestorService.GetAllAsync();
            var records = investors.Select(f => 
                PrivateInvestorResponse.Create(f, _kycService.GetKycLink(f.Email, f.KycRequestId).Result));

            using (var writer = new StringWriter())
            {
                var scv = new CsvWriter(writer);

                scv.WriteRecords(records);

                return File(Encoding.UTF8.GetBytes(writer.ToString()), "text/csv", "private-investors.csv");
            }
        }

        /// <summary>
        /// Returns all transactions in scv format
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
        /// Returns all failed transactions in scv format
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
        /// Get report for txs recalcalcation for 20m crowdsale phase
        /// </summary>
        [AdminAuth]
        [HttpGet("transactions/recalculate/20M")]
        public async Task<IActionResult> GetKycReminders()
        {
            var report = await _adminService.Recalculate20MTxs(false);

            return File(Encoding.UTF8.GetBytes(report), "text/csv", "report.txt");
        }

        /// <summary>
        /// Recalcalcate txs and related data for 20m crowdsale phase
        /// </summary>
        [AdminAuth]
        [HttpPost("transactions/recalculate/20M")]
        public async Task<IActionResult> PostKycReminderEmails([Required] string confirmation)
        {
            if (confirmation != "confirm")
            {
                return BadRequest($"The confirmation={confirmation} is not valid");
            }

            var report = await _adminService.Recalculate20MTxs(true);

            return File(Encoding.UTF8.GetBytes(report), "text/csv", "report.txt");
        }

        /// <summary>
        /// Generate referral codes for investors/private investors
        /// </summary>
        [AdminAuth]
        [HttpPost("referrals/generateCode")]
        public async Task<IActionResult> GenerateReferralCode([Required] string confirmation)
        {
            if (confirmation != "confirm")
            {
                return BadRequest($"The confirmation={confirmation} is not valid");
            }

            await _log.WriteInfoAsync(nameof(AdminController), nameof(GenerateReferralCode),
                $"Generate referral code for all investors");

            var result = await _adminService.GenerateReferralCodes();

            return Ok(result.Select(f => new { email = f.Email, code = f.Code }));
        }

        /// <summary>
        /// Send email with referral code to investor
        /// </summary>
        [AdminAuth]
        [HttpPost("referrals/{email}/sendEmail")]
        public async Task<IActionResult> SendInvestorEmailWithReferralCode([Required] string email)
        {
            var investor = await _investorService.GetAsync(email);
            if (investor == null)
            {
                return NotFound($"Investor with email={email} was not found");
            }
            if (string.IsNullOrEmpty(investor.ReferralCode))
            {
                return BadRequest($"Investor with email={email} does not have referral code");
            }

            await _log.WriteInfoAsync(nameof(AdminController), nameof(SendToAllInvestorsEmailWithReferralCode),
                $"email={email}", "Send referral code to investor");

            await _adminService.SendEmailWithReferralCode(investor);

            return Ok();
        }

        /// <summary>
        /// Send emails with referral code to all investors
        /// </summary>
        [AdminAuth]
        [HttpPost("referrals/sendEmails")]
        public async Task<IActionResult> SendToAllInvestorsEmailWithReferralCode([Required] string confirmation)
        {
            if (confirmation != "confirm")
            {
                return BadRequest($"The confirmation={confirmation} is not valid");
            }

            await _log.WriteInfoAsync(nameof(AdminController), nameof(SendToAllInvestorsEmailWithReferralCode),
                $"Send referral codes wto all investors");

            var result = new List<string>();
            var investors = await _adminService.GetAllInvestors();

            foreach (var investor in investors)
            {
                if (!string.IsNullOrEmpty(investor.ReferralCode))
                {
                    await _adminService.SendEmailWithReferralCode(investor);

                    result.Add(investor.Email);
                }
            }

            return Ok(result);
        }
    }
}
