using System;
using System.Linq;
using System.Net;
using Lykke.Service.IcoApi.Core.Services;
using Lykke.Service.IcoApi.Models;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.SwaggerGen.Annotations;
using Lykke.Service.IcoApi.Infrastructure.Auth;

namespace Lykke.Service.IcoApi.Controllers
{
    [Route("api/ico")]
    [Produces("application/json")]
    public class IcoController : Controller
    {
        /// <summary>
        /// Register user by senting confirmation email to provided address
        /// </summary>
        [HttpPost]
        [Route("user/register")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.InternalServerError)]
        public IActionResult RegisterUser([FromBody] RegisterUserRequest model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            return Ok();
        }

        /// <summary>
        /// Confirms user email and returns auth token on success
        /// </summary>
        /// <param name="model"></param>
        /// <returns>The auth token</returns>
        [HttpGet]
        [Route("user/confirm")]
        [ProducesResponseType(typeof(ConfirmUserResponse), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.InternalServerError)]
        public IActionResult ConfirmUser([FromQuery] ConfirmUserRequest model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            return Ok(new ConfirmUserResponse
            {
                AuthToken = "TEMP"
            });
        }

        /// <summary>
        /// Get user wallet addresses
        /// </summary>
        /// <remarks>
        /// If rndAddress is empty, then it needs to ask user to fill in it first to create CashIn addresses
        /// </remarks>
        [HttpGet]
        [UserAuth]
        [Route("user/wallet")]
        [ProducesResponseType(typeof(UserWalletResponse), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.InternalServerError)]
        public IActionResult GetUserWallet()
        {
            return Ok(new UserWalletResponse());
        }

        /// <summary>
        /// Save user wallet addresses, creates CashIn addresses and send summary email on success
        /// </summary>
        [HttpPost]
        [UserAuth]
        [Route("user/wallet")]
        [ProducesResponseType(typeof(UserWalletResponse), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.InternalServerError)]
        public IActionResult SaveUserWallet([FromBody] UserWalletRequest model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            return Ok(new UserWalletResponse());
        }
    }
}
