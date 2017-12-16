using Common;
using Common.Log;
using Lykke.Service.IcoApi.Core.Services;
using Lykke.Service.IcoApi.Infrastructure.Auth;
using Lykke.Service.IcoApi.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Threading.Tasks;

namespace Lykke.Service.IcoApi.Controllers
{
    [Route("api/support")]
    [Produces("application/json")]
    public class SupportController : Controller
    {
        private readonly ILog _log;
        private readonly ISupportService _supportService;
        private readonly IInvestorService _investorService;
        private readonly IBtcService _btcService;
        private readonly IEthService _ethService;

        public SupportController(ILog log, ISupportService supportService, IInvestorService investorService,
            IBtcService btcService, IEthService ethService)
        {
            _log = log;
            _supportService = supportService;
            _investorService = investorService;
            _btcService = btcService;
            _ethService = ethService;
        }

        /// <summary>
        /// Update campaign settings
        /// </summary>
        [AdminAuth]
        [HttpPost("campaign/settings")]
        public async Task<IActionResult> UpdateMinInvestAmount([Required, FromQuery] decimal minInvestAmountUsd)
        {
            await _log.WriteInfoAsync(nameof(SupportController), nameof(UpdateMinInvestAmount),
               $"minInvestAmountUsd={minInvestAmountUsd}", "Update mininmum investment amount");

            await _supportService.UpdateMinInvestAmount(minInvestAmountUsd);

            return Ok();
        }

        /// <summary>
        /// Update investor token and refunds addresses
        /// </summary>
        [AdminAuth]
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

            await _log.WriteInfoAsync(nameof(SupportController), nameof(UpdateInvestor),
               $"email={email}, model={model.ToJson()}", "Update investor addresses");

            await _supportService.UpdateInvestorAsync(email, model.TokenAddress,
                model.RefundEthAddress, model.RefundBtcAddress);

            return Ok();
        }
    }
}
