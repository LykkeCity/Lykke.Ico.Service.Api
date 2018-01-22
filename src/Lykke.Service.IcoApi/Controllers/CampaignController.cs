using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Common.Log;
using Lykke.Service.IcoApi.Core.Services;
using Lykke.Service.IcoApi.Models;
using Lykke.Ico.Core.Repositories.CampaignInfo;
using Lykke.Ico.Core.Repositories.CampaignSettings;

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

            var tokenInfo = settings.GetTokenInfo(tokensSold, DateTime.UtcNow);

            return new CampaignResponse
            {
                PreSaleStartDateTimeUtc = settings.PreSaleStartDateTimeUtc,
                PreSaleEndDateTimeUtc = settings.PreSaleEndDateTimeUtc,
                PreSaleTokensTotal = settings.PreSaleTotalTokensAmount,
                CrowdSaleStartDateTimeUtc = settings.CrowdSaleStartDateTimeUtc,
                CrowdSaleEndDateTimeUtc = settings.CrowdSaleEndDateTimeUtc,
                CrowdSaleTokensTotal = settings.CrowdSaleTotalTokensAmount,
                TokensTotal = settings.GetTotalTokensAmount(),
                Investors = investors,
                TokensSold = tokensSold,
                TokenPriceUsd = tokenInfo.Price,
                Phase = tokenInfo.Phase,
                CaptchaEnabled = settings.CaptchaEnable
            };
        }
    }
}
