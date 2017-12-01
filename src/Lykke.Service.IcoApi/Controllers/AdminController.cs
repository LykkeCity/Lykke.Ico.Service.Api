using Common.Log;
using Lykke.Service.IcoApi.Core.Services;
using Lykke.Service.IcoApi.Infrastructure.Auth;
using Lykke.Service.IcoApi.Models;
using Lykke.Service.IcoExRate.Client;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
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
        /// Returns campain info
        /// </summary>
        [AdminAuth]
        [HttpGet("campaign")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.NotFound)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetCampainInfo()
        {
            return Ok(await _adminService.GetCampainInfo());
        }

        /// <summary>
        /// Returns investor info
        /// </summary>
        [AdminAuth]
        [HttpGet("investors/{email}")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.NotFound)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.InternalServerError)]
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
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.NotFound)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.InternalServerError)]
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
        [ProducesResponseType(typeof(InvestorHistoryResponse), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.NotFound)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetInvestorHistory([Required] string email)
        {
            return Ok(InvestorHistoryResponse.Create(await _adminService.GetInvestorHistory(email)));
        }

        /// <summary>
        /// Removes investor history data
        /// </summary>
        [AdminAuth]
        [HttpDelete("investors/{email}/history")]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.NotFound)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> DeleteInvestorHistory([Required] string email)
        {
            await _adminService.DeleteInvestorHistoryAsync(email);

            return NoContent();
        }

        /// <summary>
        /// Returns the list of emails sent to user
        /// </summary>
        [AdminAuth]
        [HttpGet("investors/{email}/emails")]
        [ProducesResponseType(typeof(InvestorEmailsResponse), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.NotFound)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetInvestorEmails([Required] string email)
        {
            return Ok(InvestorEmailsResponse.Create(await _adminService.GetInvestorEmails(email)));
        }              

        /// <summary>
        /// Generates and returns random etherium address
        /// </summary>
        [AdminAuth]
        [HttpGet("addresses/random/eth")]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.InternalServerError)]
        public IActionResult GetRandomEthAddress()
        {
            var key = _ethService.GeneratePublicKey();
            var address = _ethService.GetAddressByPublicKey(key);

            return Ok(new AddressResponse { Address = address } );
        }

        /// <summary>
        /// Generates and returns random bitcoin address
        /// </summary>
        [AdminAuth]
        [HttpGet("addresses/random/btc")]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.InternalServerError)]
        public IActionResult GetRandomBtcAddress()
        {
            var key = _btcService.GeneratePublicKey();
            var address = _btcService.GetAddressByPublicKey(key);

            return Ok(new AddressResponse { Address = address });
        }

        /// <remarks>
        /// Imports the scv file with keys
        /// </remarks>
        [AdminAuth]
        [HttpPost("addresses/pool/import")]
        [DisableRequestSizeLimit]
        [ProducesResponseType(typeof(int), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> ImportKeys([FromForm] IFormFile file)
        {
            using (var reader = new StreamReader(file.OpenReadStream()))
            {
                return Ok(await _adminService.ImportPublicKeys(reader));
            }
        }

        [AdminAuth]
        [HttpGet("rates/latest")]
        public async Task<IList<IcoExRate.Client.AutorestClient.Models.AverageRateResponse>> GetLatestRates()
        {
            return await _icoExRateClient.GetAverageRates(DateTime.UtcNow);
        }

        [AdminAuth]
        [HttpGet("rates/{assetPair}/{dateTimeUtc}")]
        public async Task<IcoExRate.Client.AutorestClient.Models.AverageRateResponse> GetRatesByPairAndDateTime(
            [Required] AssetPair assetPair,
            [Required] DateTime dateTimeUtc)
        {
            var pair = Enum.Parse<IcoExRate.Client.AutorestClient.Models.Pair>(Enum.GetName(typeof(AssetPair), assetPair), true);

            return await _icoExRateClient.GetAverageRate(pair, dateTimeUtc.ToUniversalTime());
        }
    }
}
