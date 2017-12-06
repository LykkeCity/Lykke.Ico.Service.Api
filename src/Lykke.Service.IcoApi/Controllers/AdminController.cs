using Common.Log;
using Lykke.Service.IcoApi.Core.Services;
using Lykke.Service.IcoApi.Infrastructure.Auth;
using Lykke.Service.IcoApi.Models;
using Lykke.Service.IcoExRate.Client;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
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
        [HttpPost("campaign/settings")]
        public async Task<IActionResult> SaveCampaignSettings([FromBody] CampaignSettingsModel settings)
        {
            settings.StartDateTimeUtc = settings.StartDateTimeUtc.ToUniversalTime();
            settings.EndDateTimeUtc = settings.EndDateTimeUtc.ToUniversalTime();

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
        /// Removes investor from db. History is not deleted
        /// </summary>
        [AdminAuth]
        [HttpDelete("investors/{email}")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        public async Task<IActionResult> DeleteInvestor([Required] string email)
        {
            await _adminService.DeleteInvestorAsync(email);

            return NoContent();
        }

        /// <summary>
        /// Returns the history of investor changes
        /// </summary>
        [AdminAuth]
        [HttpGet("investors/{email}/history")]
        public async Task<InvestorHistoryResponse> GetInvestorHistory([Required] string email)
        {
            return InvestorHistoryResponse.Create(await _adminService.GetInvestorHistory(email));
        }

        /// <summary>
        /// Returns the list of emails sent to user
        /// </summary>
        [AdminAuth]
        [HttpGet("investors/{email}/emails")]
        public async Task<InvestorEmailsResponse> GetInvestorEmails([Required] string email)
        {
            return InvestorEmailsResponse.Create(await _adminService.GetInvestorEmails(email));
        }

        /// <summary>
        /// Returns the list of investor transactions
        /// </summary>
        [AdminAuth]
        [HttpGet("investors/{email}/transactions")]
        public async Task<InvestorTransactionsResponse> GetInvestorTransactions([Required] string email)
        {
            return InvestorTransactionsResponse.Create(await _adminService.GetInvestorTransactions(email));
        }

        /// <summary>
        /// Imports the scv file with public keys
        /// </summary>
        [AdminAuth]
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
        /// Returns latest exchange rates
        /// </summary>
        [AdminAuth]
        [HttpGet("rates/latest")]
        public async Task<IList<IcoExRate.Client.AutorestClient.Models.AverageRateResponse>> GetLatestRates()
        {
            return await _icoExRateClient.GetAverageRates(DateTime.UtcNow);
        }

        /// <summary>
        /// Returns exchange rate for provided pair and time
        /// </summary>
        [AdminAuth]
        [HttpGet("rates/{assetPair}/{dateTimeUtc}")]
        public async Task<IcoExRate.Client.AutorestClient.Models.AverageRateResponse> GetRatesByPairAndDateTime(
            [Required] AssetPair assetPair,
            [Required] DateTime dateTimeUtc)
        {
            var pair = Enum.Parse<IcoExRate.Client.AutorestClient.Models.Pair>(Enum.GetName(typeof(AssetPair), assetPair), true);

            return await _icoExRateClient.GetAverageRate(pair, dateTimeUtc.ToUniversalTime());
        }

        /// <summary>
        /// Returns ethereum address by public key
        /// </summary>
        [AdminAuth]
        [HttpGet("eth/address/{key}")]
        public AddressResponse GetEthAddressByKey([Required] string key)
        {
            return new AddressResponse { Address = _ethService.GetAddressByPublicKey(key) };
        }

        /// <summary>
        /// Generates and returns random ethereum address
        /// </summary>
        [AdminAuth]
        [HttpGet("eth/address/random")]
        public AddressResponse GetRandomEthAddress()
        {
            var key = _ethService.GeneratePublicKey();
            var address = _ethService.GetAddressByPublicKey(key);

            return new AddressResponse { Address = address };
        }

        /// <summary>
        /// Returns ether address balance
        /// </summary>
        [AdminAuth]
        [HttpGet("eth/{address}/balance")]
        public async Task<decimal> GetEthBalance([Required] string address)
        {
            return await _ethService.GetBalance(address);
        }

        /// <summary>
        /// Sends ethers to address
        /// </summary>
        [AdminAuth]
        [HttpPost("eth/send")]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> SendEth([FromBody] SendMoneyRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            return Ok(await _ethService.SendToAddress(request.Address, request.Amount));
        }

        /// <summary>
        /// Returns bitcoin address by public key
        /// </summary>
        [AdminAuth]
        [HttpGet("btc/address/{key}")]
        public AddressResponse GetBtcAddressByKey([Required] string key)
        {
            return new AddressResponse { Address = _btcService.GetAddressByPublicKey(key) };
        }

        /// <summary>
        /// Generates and returns random bitcoin address
        /// </summary>
        [AdminAuth]
        [HttpGet("btc/address/random")]
        public AddressResponse GetRandomBtcAddress()
        {
            var key = _btcService.GeneratePublicKey();
            var address = _btcService.GetAddressByPublicKey(key);

            return new AddressResponse { Address = address };
        }

        /// <summary>
        /// Returns address balance
        /// </summary>
        [AdminAuth]
        [HttpGet("btc/{address}/balance")]
        public decimal GetBtcBalance([Required] string address)
        {
            return _btcService.GetBalance(address);
        }

        /// <summary>
        /// Sends bitcoins to address
        /// </summary>
        [AdminAuth]
        [HttpPost("btc/send")]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.OK)]
        public IActionResult SendBtc([FromBody] SendMoneyRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            return Ok(_btcService.SendToAddress(request.Address, request.Amount));
        }
    }
}
