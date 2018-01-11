using Common.Log;
using Lykke.Service.IcoApi.Core.Services;
using Lykke.Service.IcoApi.Infrastructure;
using Lykke.Service.IcoApi.Models;
using Lykke.Service.IcoExRate.Client;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
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

        public AdminController(ILog log, IInvestorService investorService, IAdminService adminService, 
            IBtcService btcService, IEthService ethService, IIcoExRateClient icoExRateClient)
        {
            _log = log;
            _investorService = investorService;
            _adminService = adminService;
            _btcService = btcService;
            _ethService = ethService;
            _icoExRateClient = icoExRateClient;
        }

        /// <summary>
        /// Returns campaign info
        /// </summary>
        [AdminAuth]
        [HttpGet("campaign/info")]
        public async Task<Dictionary<string, string>> GetCampaignInfo()
        {
            return await _adminService.GetCampaignInfo();
        }

        /// <summary>
        /// Returns campaign settings
        /// </summary>
        [AdminAuth]
        [HttpGet("campaign/settings")]
        public async Task<CampaignSettingsModel> GetCampaignSettings()
        {
            var settings = await _adminService.GetCampaignSettings();

            return CampaignSettingsModel.Create(settings);
        }

        /// <summary>
        /// Save campaign settings
        /// </summary>
        [AdminAuth]
        [DisableWhenOnProd]
        [HttpPost("campaign/settings")]
        public async Task<IActionResult> SaveCampaignSettings([FromBody] CampaignSettingsModel settings)
        {
            settings.PreSaleStartDateTimeUtc = settings.PreSaleStartDateTimeUtc.ToUniversalTime();
            settings.PreSaleEndDateTimeUtc = settings.PreSaleEndDateTimeUtc.ToUniversalTime();
            settings.CrowdSaleStartDateTimeUtc = settings.CrowdSaleStartDateTimeUtc.ToUniversalTime();
            settings.CrowdSaleEndDateTimeUtc = settings.CrowdSaleEndDateTimeUtc.ToUniversalTime();

            await _adminService.SaveCampaignSettings(settings);

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
        public async Task<InvestorRefundsResponse> GetInvestorFailedTransactions([Required] string email)
        {
            return InvestorRefundsResponse.Create(await _adminService.GetInvestorRefunds(email));
        }

        /// <summary>
        /// Returns the list of all failed transactions
        /// </summary>
        [AdminAuth]
        [HttpGet("transactions/failed")]
        public async Task<InvestorRefundsResponse> GetFailedTransactions()
        {
            return InvestorRefundsResponse.Create(await _adminService.GetRefunds());
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
        [DisableWhenOnProd]
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
