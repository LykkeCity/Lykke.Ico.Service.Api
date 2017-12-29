using Common.Log;
using Lykke.Service.IcoApi.Core.Services;
using Lykke.Service.IcoApi.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Threading.Tasks;

namespace Lykke.Service.IcoApi.Controllers
{
    [Route("api/kyc")]
    public class KycController : Controller
    {
        private readonly ILog _log;
        private readonly IEncryptionService _encryptionService;
        private readonly IInvestorService _investorService;

        public KycController(ILog log, IEncryptionService encryptionService, IInvestorService investorService)
        {
            _log = log;
            _encryptionService = encryptionService;
            _investorService = investorService;
        }

        /// <summary>
        /// Save KYC results from KYC provider
        /// </summary>
        [HttpPost]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        public async Task<IActionResult> SaveKycResults([Required, FromBody] KycRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            if (string.IsNullOrEmpty(request.Message))
            {
                return BadRequest("Message can not be empty");
            }

            var message = "";
            try
            {
                message = _encryptionService.Decrypt(request.Message);
            }
            catch
            {
                return BadRequest("Failed to decrypt message");
            }

            KycMessage kycMessage;
            try
            {
                kycMessage = JsonConvert.DeserializeObject<KycMessage>(message);
            }
            catch
            {
                return BadRequest("Failed to deserialize message");
            }
            if (kycMessage == null)
            {
                return BadRequest("Decrypted message is null");
            }
            if (string.IsNullOrEmpty(kycMessage.KycStat))
            {
                return BadRequest("KycStat is null or empty");
            }

            var email = await _investorService.GetEmailByKycId(kycMessage.KycId);
            if (string.IsNullOrEmpty(email))
            {
                return BadRequest($"Investor not found for provided KycId={kycMessage.KycId}");
            }

            await _investorService.SaveKycResultAsync(email, kycMessage.KycStat);

            return Ok();
        }
    }
}
