using System.Net;
using System;
using System.Threading.Tasks;
using System.Security.Claims;
using Lykke.Service.IcoApi.Core.Services;
using Lykke.Service.IcoApi.Models;
using Lykke.Service.IcoApi.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Common.Log;
using Common;

namespace Lykke.Service.IcoApi.Controllers
{
    [Route("api/investor")]
    [Produces("application/json")]
    public class InvestorController : Controller
    {
        private readonly ILog _log;
        private readonly IInvestorService _investorService;
        private readonly IBtcService _btcService;
        private readonly IEthService _ethService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IKycService _kycService;

        public InvestorController(ILog log, IInvestorService investorService, IBtcService btcService,
            IEthService ethService, IHttpContextAccessor httpContextAccessor,
            IKycService kycService)
        {
            _log = log;
            _investorService = investorService;
            _btcService = btcService;
            _ethService = ethService;
            _httpContextAccessor = httpContextAccessor;
            _kycService = kycService;
        }

        /// <summary>
        /// Register investor by sending confirmation email to provided address
        /// </summary>
        [HttpPost]
        [Route("register")]
        [ValidateReCaptcha]
        [ProducesResponseType(typeof(RegisterInvestorResponse), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> RegisterInvestor([FromBody] RegisterInvestorRequest model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }            

            await _log.WriteInfoAsync(nameof(InvestorController), nameof(RegisterInvestor), 
                $"email={model.Email}", "Register investor");

            var result = await _investorService.RegisterAsync(model.Email);

            return Ok(new RegisterInvestorResponse { Result = result } );
        }

        /// <summary>
        /// Confirms investor email and returns auth token
        /// </summary>
        /// <param name="token"></param>
        /// <returns>The auth token</returns>
        [HttpGet]
        [Route("confirmation/{token}")]
        [ProducesResponseType(typeof(ConfirmInvestorResponse), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> ConfirmInvestor(Guid token)
        {
            await _log.WriteInfoAsync(nameof(InvestorController), nameof(ConfirmInvestor), 
                $"token={token}", "Confirm investor email");

            var success = await _investorService.ConfirmAsync(token);
            if (!success)
            {
                return NotFound();
            }

            return Ok(new ConfirmInvestorResponse
            {
                AuthToken = token.ToString()
            });
        }

        /// <summary>
        /// Get Investor info
        /// </summary>
        [HttpGet]
        [InvestorAuth]
        public async Task<InvestorResponse> Get()
        {
            var email = GetAuthUserEmail();

            await _log.WriteInfoAsync(nameof(InvestorController), nameof(Get),
                $"email={email}", "Get investor");

            var investor = await _investorService.GetAsync(email);
            var kycLink = await _kycService.GetKycLink(investor.Email, investor.KycRequestId);
            var response = InvestorResponse.Create(investor, kycLink);

            return response;
        }

        /// <summary>
        /// Save investor info, creates pay-in addresses, sends summary email
        /// </summary>
        [HttpPost]
        [InvestorAuth]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        public async Task<IActionResult> Post([FromBody] InvestorRequest model)
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

            var email = GetAuthUserEmail();

            var investor = await _investorService.GetAsync(email);
            if (!string.IsNullOrEmpty(investor.TokenAddress))
            {
                return BadRequest(@"The token address is already defined");
            }

            await _log.WriteInfoAsync(nameof(InvestorController), nameof(Post), 
                $"email={email}, model={model.ToJson()}", "Save investor addresses");

            await _investorService.UpdateAsync(email, model.TokenAddress, model.RefundEthAddress, model.RefundBtcAddress);

            return Ok();
        }

        /// <summary>
        /// Send fiat transaction details
        /// </summary>
        [HttpPost]
        [InvestorAuth]
        [Route("fiat")]
        public async Task<IActionResult> SendFiat([FromBody] SendFiatRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            if (string.IsNullOrEmpty(request.TransactionId))
            {
                return BadRequest($"transactionId can not be null or empty");
            }

            var email = GetAuthUserEmail();

            await _log.WriteInfoAsync(nameof(InvestorController), nameof(SendFiat),
                $"email={email}, request={request.ToJson()}", "Send fiat request");

            await _investorService.SendFiatTransaction(email, request.TransactionId, 
                request.Amount, request.Fee);

            return Ok();
        }

        private string GetAuthUserEmail()
        {
            return User.FindFirst(ClaimTypes.Email).Value;
        }
    }
}
