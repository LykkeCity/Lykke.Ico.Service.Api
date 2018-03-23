using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Common.Log;
using Lykke.Service.IcoApi.Core.Services;
using Lykke.Service.IcoApi.Models;
using Lykke.Service.IcoApi.Services.Extensions;
using Lykke.Service.IcoApi.Core.Domain.Investor;
using Lykke.Service.IcoApi.Core.Repositories;
using Microsoft.Extensions.Caching.Memory;

namespace Lykke.Service.IcoApi.Controllers
{
    [Route("api/campaign")]
    [Produces("application/json")]
    public class CampaignController : Controller
    {
        private readonly ILog _log;
        private readonly ICampaignService _campaignService;
        private readonly ICampaignInfoRepository _campaignInfoRepository;
        private readonly IMemoryCache _cache;
        private readonly string _key = "CampaignResponse";

        public CampaignController(ILog log, 
            ICampaignService campaignService,
            ICampaignInfoRepository campaignInfoRepository,
            IMemoryCache cache)
        {
            _log = log;
            _campaignService = campaignService;
            _campaignInfoRepository = campaignInfoRepository;
            _cache = cache;
        }

        /// <summary>
        /// Returns public campaign info
        /// </summary>
        [HttpGet]
        public async Task<CampaignResponse> GetCampaign()
        {
            var response = _cache.Get<CampaignResponse>(_key);
            if (response != null)
            {
                return response;
            }

            var settings = await _campaignService.GetCampaignSettings();
            var failedTxs = await _campaignService.GetRefunds();

            var now = DateTime.UtcNow;
            var campaignActive = false;

            var logiTokenInfo = await settings.GetLogiTokenInfo(_campaignInfoRepository, DateTime.Now);
            var smarcTokenInfo = await settings.GetSmarcTokenInfo(_campaignInfoRepository, DateTime.Now);

            if (settings.IsPreSale(now) &&
                !failedTxs.Any(f => f.Reason == InvestorRefundReason.PreSaleTokensSoldOut))
            {
                campaignActive = true;
            }
            if (settings.IsCrowdSale(now) &&
                !failedTxs.Any(f => f.Reason == InvestorRefundReason.CrowdSaleTokensSoldOut))
            {
                campaignActive = true;
            }

            response = new CampaignResponse
            {
                CaptchaEnabled = settings.CaptchaEnable,
                CampaignActive = campaignActive,
                SmarcPhase = smarcTokenInfo.Phase,
                SmarcPhaseAmount = smarcTokenInfo.PhaseTokenAmount,
                SmarcPhaseAmountAvailable = smarcTokenInfo.PhaseTokenAmountAvailable,
                SmarcPhasePriceUsd = smarcTokenInfo.PriceUsd,
                LogiPhase = logiTokenInfo.Phase,
                LogiPhaseAmount = logiTokenInfo.PhaseTokenAmount,
                LogiPhaseAmountAvailable = logiTokenInfo.PhaseTokenAmountAvailable,
                LogiPhasePriceUsd = logiTokenInfo.PriceUsd,
            };

            _cache.Set(_key, response, DateTimeOffset.Now.AddSeconds(1));

            return response;
        }
    }
}
