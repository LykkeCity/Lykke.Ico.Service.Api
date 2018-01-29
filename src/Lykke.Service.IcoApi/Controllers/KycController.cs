﻿using Common.Log;
using Lykke.Ico.Core.Services;
using Lykke.Service.IcoApi.Core.Services;
using Lykke.Service.IcoApi.Infrastructure;
using Lykke.Service.IcoApi.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Threading.Tasks;

namespace Lykke.Service.IcoApi.Controllers
{
    [Route("api/kyc")]
    public class KycController : Controller
    {
        private readonly ILog _log;
        private readonly IUrlEncryptionService _urlEncryptionService;
        private readonly IInvestorService _investorService;
        private readonly IPrivateInvestorService _privateInvestorService;

        public KycController(ILog log, IUrlEncryptionService urlEncryptionService, IInvestorService investorService,
            IPrivateInvestorService privateInvestorService)
        {
            _log = log;
            _urlEncryptionService = urlEncryptionService;
            _investorService = investorService;
            _privateInvestorService = privateInvestorService;
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
                message = _urlEncryptionService.Decrypt(request.Message);
            }
            catch
            {
                return BadRequest("Failed to decrypt message");
            }

            await _log.WriteInfoAsync(nameof(KycController), nameof(SaveKycResults),
                $"message={message}", "Decrypted kyc message");

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
            if (string.IsNullOrEmpty(kycMessage.KycStatus))
            {
                return BadRequest("KycStatus is null or empty");
            }

            var investorEmail = await _investorService.GetEmailByKycId(kycMessage.KycId);
            if (!string.IsNullOrEmpty(investorEmail))
            {
                await _investorService.SaveKycResultAsync(investorEmail, kycMessage.KycStatus);

                return Ok();
            }

            var privateInvestorEmail = await _privateInvestorService.GetEmailByKycId(kycMessage.KycId);
            if (!string.IsNullOrEmpty(privateInvestorEmail))
            {
                await _privateInvestorService.SaveKycResultAsync(privateInvestorEmail, kycMessage.KycStatus);

                return Ok();
            }

            return BadRequest($"Investor was not found for provided KycId={kycMessage.KycId}");
        }

        /// <summary>
        /// Encrypt text
        /// </summary>
        [DisableDebugMethods]
        [HttpPost("debug/encrypt/{text}")]
        public string EncryptKycMessage([Required] string text)
        {
            return _urlEncryptionService.Encrypt(text);
        }

        /// <summary>
        /// Decrypt text
        /// </summary>
        [DisableDebugMethods]
        [HttpPost("debug/decrypt/{text}")]
        public string DecryptKycMessage([Required] string text)
        {
            return _urlEncryptionService.Decrypt(text);
        }
    }
}