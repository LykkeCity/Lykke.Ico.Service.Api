using System.Net;
using Lykke.Service.IcoApi.Core.Services;
using Lykke.Service.IcoApi.Models;
using Microsoft.AspNetCore.Mvc;
using Lykke.Service.IcoApi.Infrastructure.Auth;
using System.Threading.Tasks;

namespace Lykke.Service.IcoApi.Controllers
{
    [Route("api/ico")]
    [Produces("application/json")]
    public class IcoController : Controller
    {
        private readonly IInvestorService _investorService;

        public IcoController(IInvestorService investorService)
        {
            _investorService = investorService;
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

            await _investorService.RegisterAsync(model.Email);

            return Ok();
        }

        /// <summary>
        /// Confirms investor email and returns auth token
        /// </summary>
        /// <param name="model"></param>
        /// <returns>The auth token</returns>
        [HttpGet]
        [Route("investor/confirm")]
        [ProducesResponseType(typeof(ConfirmInvestorResponse), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.InternalServerError)]
        public IActionResult ConfirmInvestor([FromQuery] ConfirmInvestorRequest model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            return Ok(new ConfirmInvestorResponse
            {
                AuthToken = "TEMP"
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
        public IActionResult GetInvestorInfo()
        {
            return Ok(new InvestorResponse());
        }

        /// <summary>
        /// Save investor info, creates pay-in addresses, sends summary email
        /// </summary>
        [HttpPost]
        [InvestorAuth]
        [Route("investor")]
        [ProducesResponseType(typeof(InvestorResponse), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.InternalServerError)]
        public IActionResult SaveInvestorInfo([FromBody] InvestorRequest model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            return Ok(new InvestorResponse());
        }
    }
}
