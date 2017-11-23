using System.Net;
using Lykke.Service.IcoApi.Core.Services;
using Lykke.Service.IcoApi.Models;
using Microsoft.AspNetCore.Mvc;
using Lykke.Service.IcoApi.Infrastructure.Auth;
using Microsoft.AspNetCore.Http;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
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

        public InvestorController(ILog log, IInvestorService investorService, IBtcService btcService,
            IEthService ethService, IHttpContextAccessor httpContextAccessor)
        {
            _log = log;
            _investorService = investorService;
            _btcService = btcService;
            _ethService = ethService;
            _httpContextAccessor = httpContextAccessor;
        }

        /// <summary>
        /// Register investor by sending confirmation email to provided address
        /// </summary>
        [HttpPost]
        [Route("register")]
        [ProducesResponseType(typeof(RegisterInvestorResponse), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> RegisterInvestor([FromBody] RegisterInvestorRequest model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            await _log.WriteInfoAsync(
                nameof(InvestorController), 
                nameof(RegisterInvestor), 
                $"Register investor with email={model.Email} from ip={GetRequestIP()}");

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
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> ConfirmInvestor(Guid token)
        {
            await _log.WriteInfoAsync(
                nameof(InvestorController), 
                nameof(ConfirmInvestor), 
                $"Confirm investor with token={token} from ip={GetRequestIP()}");

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
        /// <remarks>
        /// If rndAddress is empty, then it needs to ask user to fill in it first to create pay-in addresses
        /// </remarks>
        [HttpGet]
        [InvestorAuth]
        [ProducesResponseType(typeof(InvestorResponse), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> Get()
        {
            var email = GetAuthUserEmail();

            await _log.WriteInfoAsync(
                nameof(InvestorController), 
                nameof(Get), 
                $"Get investor with email={email} from ip={GetRequestIP()}");

            var investor = await _investorService.GetAsync(email);

            return Ok(InvestorResponse.Create(investor));
        }

        /// <summary>
        /// Save investor info, creates pay-in addresses, sends summary email
        /// </summary>
        [HttpPost]
        [InvestorAuth]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> Post([FromBody] InvestorRequest model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            if (!_ethService.ValidateAddress(model.TokenAddress))
            {
                return BadRequest($"The address={model.TokenAddress} is invalid IRC20 address");
            }
            if (!string.IsNullOrEmpty(model.RefundEthAddress) && !_ethService.ValidateAddress(model.RefundEthAddress))
            {
                return BadRequest($"The address={model.RefundEthAddress} is invalid ETH address");
            }
            if (!string.IsNullOrEmpty(model.RefundBtcAddress) && !_btcService.ValidateAddress(model.RefundBtcAddress))
            {
                return BadRequest($"The address={model.RefundBtcAddress} is invalid BTC address");
            }

            var email = GetAuthUserEmail();

            var investor = await _investorService.GetAsync(email);
            if (!string.IsNullOrEmpty(investor.TokenAddress))
            {
                return BadRequest(@"The token address is already defined");
            }

            await _log.WriteInfoAsync( nameof(InvestorController), nameof(Post), $"Save investor: {model.ToJson()}");
            await _investorService.UpdateAsync(email, model.TokenAddress, model.RefundEthAddress, model.RefundBtcAddress);

            return Ok();
        }

        private string GetAuthUserEmail()
        {
            return User.FindFirst(ClaimTypes.Email).Value;
        }

        private string GetRequestIP()
        {
            // todo support new "Forwarded" header (2014) https://en.wikipedia.org/wiki/X-Forwarded-For

            // X-Forwarded-For (csv list):  Using the First entry in the list seems to work
            // for 99% of cases however it has been suggested that a better (although tedious)
            // approach might be to read each IP from right to left and use the first public IP.
            // http://stackoverflow.com/a/43554000/538763

            var ip = SplitCsv(GetHeaderValueAs<string>("X-Forwarded-For")).FirstOrDefault();
            if (!String.IsNullOrWhiteSpace(ip))
            {
                return ip;
            }

            ip = _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString();
            if (!String.IsNullOrWhiteSpace(ip))
            {
                return ip;
            }

            return GetHeaderValueAs<string>("REMOTE_ADDR");
        }

        private T GetHeaderValueAs<T>(string headerName)
        {
            if (_httpContextAccessor.HttpContext?.Request?.Headers?.TryGetValue(headerName, out var values) ?? false)
            {
                var rawValues = values.ToString();   // writes out as Csv when there are multiple.
                if (!string.IsNullOrEmpty(rawValues))
                {
                    return (T)Convert.ChangeType(values.ToString(), typeof(T));
                }
            }

            return default(T);
        }

        private List<string> SplitCsv(string csvList)
        {
            if (string.IsNullOrWhiteSpace(csvList))
            {
                return new List<string>();
            }

            return csvList
                .TrimEnd(',')
                .Split(',')
                .AsEnumerable<string>()
                .Select(s => s.Trim())
                .ToList();
        }
    }
}
