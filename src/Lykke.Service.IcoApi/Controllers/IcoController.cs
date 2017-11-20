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

namespace Lykke.Service.IcoApi.Controllers
{
    [Route("api/ico")]
    [Produces("application/json")]
    public class IcoController : Controller
    {
        private readonly IInvestorService _investorService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public IcoController(IInvestorService investorService, IHttpContextAccessor httpContextAccessor)
        {
            _investorService = investorService;
            _httpContextAccessor = httpContextAccessor;
        }

        /// <summary>
        /// Register investor by sending confirmation email to provided address
        /// </summary>
        [HttpPost]
        [Route("investor/register")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> RegisterInvestor([FromBody] RegisterInvestorRequest model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var ipAddress = GetRequestIP();

            await _investorService.RegisterAsync(model.Email, ipAddress);

            return Ok();
        }

        /// <summary>
        /// Confirms investor email and returns auth token
        /// </summary>
        /// <param name="token"></param>
        /// <returns>The auth token</returns>
        [HttpGet]
        [Route("investor/confirmation/{token}")]
        [ProducesResponseType(typeof(ConfirmInvestorResponse), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> ConfirmInvestor(Guid token)
        {
            var ipAddress = GetRequestIP();

            var success = await _investorService.ConfirmAsync(token, ipAddress);
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
        [Route("investor")]
        [ProducesResponseType(typeof(InvestorResponse), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetInvestor()
        {
            var email = User.FindFirst(ClaimTypes.Email).Value;

            var investor = await _investorService.GetAsync(email);
            if (investor == null)
            {
                throw new Exception($"Failed to find investor with email={email}");
            }

            return Ok(InvestorResponse.Create(investor));
        }

        /// <summary>
        /// Save investor info, creates pay-in addresses, sends summary email
        /// </summary>
        [HttpPost]
        [InvestorAuth]
        [Route("investor")]
        [ProducesResponseType(typeof(InvestorResponse), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> SaveInvestor([FromBody] InvestorRequest model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var email = User.FindFirst(ClaimTypes.Email).Value;

            await _investorService.UpdateAddressesAsync(
                email, 
                model.TokenAddress,
                model.RefundEthAddress,
                model.RefundBtcAddress);

            return Ok(new InvestorResponse());
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
