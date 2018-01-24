using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Common.Log;
using Lykke.Service.IcoApi.Core.Services;
using Lykke.Service.IcoApi.Models;
using Lykke.Ico.Core.Repositories.CampaignInfo;
using Lykke.Ico.Core.Repositories.CampaignSettings;
using Lykke.Ico.Core;
using System.Linq;
using Lykke.Ico.Core.Repositories.InvestorRefund;

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
            var failedTxs = await _campaignService.GetRefunds();

            var now = DateTime.UtcNow;
            var campaignActive = false;

            if (settings.IsPreSale(now) &&
                !failedTxs.Any(f => f.Reason == InvestorRefundReason.PreSaleTokensSoldOut))
            {
                campaignActive = true;
            }
            if (settings.IsCrowdSale(now) &&
                !failedTxs.Any(f => f.Reason == InvestorRefundReason.TokensSoldOut))
            {
                campaignActive = true;
            }

            return new CampaignResponse
            {
                CaptchaEnabled = settings.CaptchaEnable,
                CampaignActive = campaignActive
            };
        }
    }
}
