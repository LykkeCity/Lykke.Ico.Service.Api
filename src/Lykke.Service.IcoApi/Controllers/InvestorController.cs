﻿using System.Net;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using Lykke.Service.IcoApi.Core.Services;
using Lykke.Service.IcoApi.Models;
using Lykke.Service.IcoApi.Infrastructure;
using Lykke.Ico.Core.Services;
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
        private readonly IFiatService _fiatService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IKycService _kycService;
        private readonly IReferralCodeService _referralCodeService;

        public InvestorController(ILog log, IInvestorService investorService, IBtcService btcService,
            IEthService ethService, IFiatService fiatService, IHttpContextAccessor httpContextAccessor,
            IKycService kycService, IReferralCodeService referralCodeService)
        {
            _log = log;
            _investorService = investorService;
            _btcService = btcService;
            _ethService = ethService;
            _fiatService = fiatService;
            _httpContextAccessor = httpContextAccessor;
            _kycService = kycService;
            _referralCodeService = referralCodeService;
        }

        /// <summary>
        /// Register investor by sending confirmation email to provided address
        /// </summary>
        [HttpPost]
        [Route("register")]
        [ValidateReCaptcha]
        [ProducesResponseType(typeof(RegisterInvestorResponse), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> RegisterInvestor([FromBody] RegisterInvestorRequest model)
        {
            await _log.WriteInfoAsync(nameof(InvestorController), nameof(RegisterInvestor),
                $"model={model.ToJson()}, ip={GetRequestIP()}",
                "Register investor");

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (!string.IsNullOrEmpty(model.ReferralCode))
            {
                var email = await _referralCodeService.GetReferralEmail(model.ReferralCode);
                if (string.IsNullOrEmpty(email))
                {
                    await _log.WriteInfoAsync(nameof(InvestorController), nameof(RegisterInvestor),
                        $"model={model.ToJson()}, ip={GetRequestIP()}",
                        "Wrong referral code");

                    return BadRequest($"The referral code={model.ReferralCode} was not found");
                }
            }

            var result = await _investorService.RegisterAsync(model.Email, model.ReferralCode);

            return Ok(new RegisterInvestorResponse { Result = result } );
        }

        /// <summary>
        /// Login investor
        /// </summary>
        /// <remarks>
        /// Returns 204 in case if investor was not found or did not fill in Token Address. Returns 200 when summary email was sent
        /// </remarks>
        [HttpPost]
        [Route("login")]
        [ValidateReCaptcha]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> LoginInvestor([FromBody] LoginInvestorRequest model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            await _log.WriteInfoAsync(nameof(InvestorController), nameof(LoginInvestor),
                $"email={model.Email}, ip={GetRequestIP()}",
                "Login investor");

            var email = model.Email.Trim().ToLowCase();

            var investor = await _investorService.GetAsync(email);
            if (investor == null || string.IsNullOrEmpty(investor.TokenAddress))
            {
                return NoContent();
            }

            await _investorService.SendSummaryEmail(investor);

            return Ok();
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
            await _log.WriteInfoAsync(nameof(InvestorController), nameof(ConfirmInvestor), 
                $"token={token}, ip={GetRequestIP()}",
                "Confirm investor email");

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
        [ProducesResponseType(typeof(InvestorResponse), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> Get()
        {
            var email = GetAuthUserEmail();

            await _log.WriteInfoAsync(nameof(InvestorController), nameof(Get),
                            $"email={email}, ip={GetRequestIP()}",
                            "Get investor");

            var investor = await _investorService.GetAsync(email);
            var kycLink = await _kycService.GetKycLink(investor.Email, investor.KycRequestId);
            var response = InvestorResponse.Create(investor, kycLink);

            return Ok(response);
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
                $"email={email}, ip={GetRequestIP()}, model={model.ToJson()}",
                "Save investor addresses");

            await _investorService.UpdateAsync(email, model.TokenAddress, model.RefundEthAddress, model.RefundBtcAddress);

            return Ok();
        }

        ///// <summary>
        ///// Charge investor card
        ///// </summary>
        //[HttpPost]
        //[InvestorAuth]
        //[Route("charge")]
        //[ProducesResponseType(typeof(ChargeInvestorResponse), (int)HttpStatusCode.OK)]
        //[ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.InternalServerError)]
        //public async Task<IActionResult> ChargeInvestor([FromBody] ChargeInvestorRequest model)
        //{
        //    if (!ModelState.IsValid)
        //    {
        //        return BadRequest(ModelState);
        //    }

        //    var email = GetAuthUserEmail();

        //    await _log.WriteInfoAsync(
        //        nameof(InvestorController),
        //        nameof(ChargeInvestor),
        //        $"email={email}, ip={GetRequestIP()}, model={model.ToJson()}",
        //        "Charge investor card");

        //    var result = await _fiatService.Charge(email, model.Token, model.Amount);

        //    return Ok(ChargeInvestorResponse.Create(result));
        //}

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
