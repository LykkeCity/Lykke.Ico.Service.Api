using Common;
using Common.Log;
using Lykke.Service.IcoApi.Core.Domain.Campaign;
using Lykke.Service.IcoApi.Core.Domain.Investor;
using Lykke.Service.IcoApi.Core.Domain.Token;
using Lykke.Service.IcoApi.Core.Services;
using Lykke.Service.IcoApi.Infrastructure;
using Lykke.Service.IcoApi.Models;
using Lykke.Service.IcoApi.Services.Extensions;
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
    }
}
