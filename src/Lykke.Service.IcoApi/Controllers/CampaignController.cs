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
using Lykke.Ico.Core.Repositories.CampaignInfo;

namespace Lykke.Service.IcoApi.Controllers
{
    [Route("api/campaign")]
    [Produces("application/json")]
    public class CampaignController : Controller
    {
        private readonly ILog _log;
        private readonly ICampaignService _campaignService;

        public CampaignController(ILog log, ICampaignService campaignService)
        {
            _log = log;
            _campaignService = campaignService;
        }

        /// <summary>
        /// Returns public campaign info
        /// </summary>
        [HttpGet]
        public async Task<CampaignResponse> GetCampaign()
        {
            var settings = await _campaignService.GetCampaignSettings();

            var investorsConfirmed = await _campaignService.GetCampaignInfoValue(CampaignInfoType.InvestorsConfirmed);
            if (!Int32.TryParse(investorsConfirmed, out var investors))
            {
                investors = 0;
            }

            var amountInvestedToken = await _campaignService.GetCampaignInfoValue(CampaignInfoType.AmountInvestedToken);
            if (!Decimal.TryParse(amountInvestedToken, out var tokensSold))
            {
                tokensSold = 0;
            }

            return new CampaignResponse
            {
                StartDateTimeUtc = settings.StartDateTimeUtc,
                EndDateTimeUtc = settings.EndDateTimeUtc,
                TokensTotal = settings.TotalTokensAmount,
                Investors = investors,
                TokensSold = tokensSold
            };
        }
    }
}
