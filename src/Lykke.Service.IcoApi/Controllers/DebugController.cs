using Common.Log;
using Lykke.Ico.Core;
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

        public DebugController(ILog log, IInvestorService investorService, IAdminService adminService,
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
        /// Removes investor from db. History is not deleted
        /// </summary>
        [AdminAuth]
        [DisableWhenOnProd]
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
        [DisableWhenOnProd]
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
        [DisableWhenOnProd]
        [HttpGet("rates/latest")]
        public async Task<IList<IcoExRate.Client.AutorestClient.Models.AverageRateResponse>> GetLatestRates()
        {
            return await _icoExRateClient.GetAverageRates(DateTime.UtcNow);
        }

        /// <summary>
        /// Returns exchange rate for provided pair and time
        /// </summary>
        [AdminAuth]
        [DisableWhenOnProd]
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
        [DisableWhenOnProd]
        [HttpGet("eth/address/{key}")]
        public AddressResponse GetEthAddressByKey([Required] string key)
        {
            return new AddressResponse { Address = _ethService.GetAddressByPublicKey(key) };
        }

        /// <summary>
        /// Generates and returns random ethereum address
        /// </summary>
        [AdminAuth]
        [DisableWhenOnProd]
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
        [DisableWhenOnProd]
        [HttpGet("eth/{address}/balance")]
        public async Task<decimal> GetEthBalance([Required] string address)
        {
            return await _ethService.GetBalance(address);
        }

        /// <summary>
        /// Sends ethers to address
        /// </summary>
        [AdminAuth]
        [DisableWhenOnProd]
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
        [DisableWhenOnProd]
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
        [DisableWhenOnProd]
        [HttpGet("btc/address/{key}")]
        public AddressResponse GetBtcAddressByKey([Required] string key)
        {
            return new AddressResponse { Address = _btcService.GetAddressByPublicKey(key) };
        }

        /// <summary>
        /// Generates and returns random bitcoin address
        /// </summary>
        [AdminAuth]
        [DisableWhenOnProd]
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
        [DisableWhenOnProd]
        [HttpGet("btc/{address}/balance")]
        public decimal GetBtcBalance([Required] string address)
        {
            return _btcService.GetBalance(address);
        }

        /// <summary>
        /// Sends bitcoins to address
        /// </summary>
        [AdminAuth]
        [DisableWhenOnProd]
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
        [DisableWhenOnProd]
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
        [DisableWhenOnProd]
        [HttpPost("fiat/send/tx")]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> SendFiatTransactionMessage([FromBody] TransactionMessageRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            await _adminService.SendTransactionMessageAsync(request.Email, CurrencyType.Fiat,
                request.CreatedUtc, request.UniqueId, request.Amount);

            return Ok();
        }
    }
}
