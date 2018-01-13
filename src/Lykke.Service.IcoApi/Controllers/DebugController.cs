using Common.Log;
using Lykke.Ico.Core;
using Lykke.Ico.Core.Services;
using Lykke.Service.IcoApi.Core.Services;
using Lykke.Service.IcoApi.Infrastructure;
using Lykke.Service.IcoApi.Models;
using Lykke.Service.IcoExRate.Client;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Threading.Tasks;

namespace Lykke.Service.IcoApi.Controllers
{
    [Route("api/debug")]
    [Produces("application/json")]
    public class DebugController : Controller
    {
        private readonly ILog _log;
        private readonly IInvestorService _investorService;
        private readonly IAdminService _adminService;
        private readonly IBtcService _btcService;
        private readonly IEthService _ethService;
        private readonly IIcoExRateClient _icoExRateClient;
        private readonly IUrlEncryptionService _urlEncryptionService;

        public DebugController(ILog log, IInvestorService investorService, IAdminService adminService,
            IBtcService btcService, IEthService ethService, IIcoExRateClient icoExRateClient,
            IUrlEncryptionService urlEncryptionService)
        {
            _log = log;
            _investorService = investorService;
            _adminService = adminService;
            _btcService = btcService;
            _ethService = ethService;
            _icoExRateClient = icoExRateClient;
            _urlEncryptionService = urlEncryptionService;
        }

        /// <summary>
        /// Removes investor from db. History is not deleted
        /// </summary>
        [AdminAuth]
        [DisableDebugMethods]
        [HttpDelete("investors/{email}")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        public async Task<IActionResult> DeleteInvestor([Required] string email)
        {
            await _adminService.DeleteInvestorAsync(email);

            return NoContent();
        }

        /// <summary>
        /// Removes all data for investor from db
        /// </summary>
        /// <remarks>
        /// profile history, address pool history, emails, transactions
        /// </remarks>
        [AdminAuth]
        [DisableDebugMethods]
        [HttpDelete("investors/{email}/all")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        public async Task<IActionResult> DeleteInvestorAllData([Required] string email)
        {
            await _adminService.DeleteInvestorAllDataAsync(email);

            return NoContent();
        }

        /// <summary>
        /// Returns latest exchange rates
        /// </summary>
        [AdminAuth]
        [DisableDebugMethods]
        [HttpGet("rates/latest")]
        public async Task<IList<IcoExRate.Client.AutorestClient.Models.AverageRateResponse>> GetLatestRates()
        {
            return await _icoExRateClient.GetAverageRates(DateTime.UtcNow);
        }

        /// <summary>
        /// Returns exchange rate for provided pair and time
        /// </summary>
        [AdminAuth]
        [DisableDebugMethods]
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
        [DisableDebugMethods]
        [HttpGet("eth/address/{key}")]
        public AddressResponse GetEthAddressByKey([Required] string key)
        {
            return new AddressResponse { Address = _ethService.GetAddressByPublicKey(key) };
        }

        /// <summary>
        /// Generates and returns random ethereum address
        /// </summary>
        [AdminAuth]
        [DisableDebugMethods]
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
        [DisableDebugMethods]
        [HttpGet("eth/{address}/balance")]
        public async Task<decimal> GetEthBalance([Required] string address)
        {
            return await _ethService.GetBalance(address);
        }

        /// <summary>
        /// Sends ethers to address
        /// </summary>
        [AdminAuth]
        [DisableDebugMethods]
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
        /// Sends ether transaction message to queue
        /// </summary>
        [AdminAuth]
        [DisableDebugMethods]
        [HttpPost("eth/send/tx")]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> SendEthTransactionMessage([FromBody] TransactionMessageRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            await _adminService.SendTransactionMessageAsync(request.Email, CurrencyType.Ether,
                request.CreatedUtc, request.UniqueId, request.Amount);

            return Ok();
        }

        /// <summary>
        /// Returns bitcoin address by public key
        /// </summary>
        [AdminAuth]
        [DisableDebugMethods]
        [HttpGet("btc/address/{key}")]
        public AddressResponse GetBtcAddressByKey([Required] string key)
        {
            return new AddressResponse { Address = _btcService.GetAddressByPublicKey(key) };
        }

        /// <summary>
        /// Generates and returns random bitcoin address
        /// </summary>
        [AdminAuth]
        [DisableDebugMethods]
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
        [DisableDebugMethods]
        [HttpGet("btc/{address}/balance")]
        public decimal GetBtcBalance([Required] string address)
        {
            return _btcService.GetBalance(address);
        }

        /// <summary>
        /// Sends bitcoins to address
        /// </summary>
        [AdminAuth]
        [DisableDebugMethods]
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

        /// <summary>
        /// Sends bitcoin transaction message to queue
        /// </summary>
        [AdminAuth]
        [DisableDebugMethods]
        [HttpPost("btc/send/tx")]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> SendBtcTransactionMessage([FromBody] TransactionMessageRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            await _adminService.SendTransactionMessageAsync(request.Email, CurrencyType.Bitcoin,
                request.CreatedUtc, request.UniqueId, request.Amount);

            return Ok();
        }

        /// <summary>
        /// Sends fiat transaction message to queue
        /// </summary>
        [AdminAuth]
        [DisableDebugMethods]
        [HttpPost("fiat/send/tx")]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> SendFiatTransactionMessage([FromBody] TransactionMessageRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _adminService.SendTransactionMessageAsync(request.Email, CurrencyType.Fiat,
                request.CreatedUtc, request.UniqueId, request.Amount);

            return Ok(result);
        }

        /// <summary>
        /// Encrypt kyc message
        /// </summary>
        [AdminAuth]
        [DisableDebugMethods]
        [HttpPost("kyc/encrypt")]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.OK)]
        public IActionResult EncryptKycMessage([FromBody] EncryptionMessageRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = _urlEncryptionService.Encrypt(request.Message);

            return Ok(result);
        }

        /// <summary>
        /// Decrypt kyc message
        /// </summary>
        [AdminAuth]
        [DisableDebugMethods]
        [HttpPost("kyc/decrypt")]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.OK)]
        public IActionResult DecryptKycMessage([FromBody] EncryptionMessageRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = _urlEncryptionService.Decrypt(request.Message);

            return Ok(result);
        }
    }
}
