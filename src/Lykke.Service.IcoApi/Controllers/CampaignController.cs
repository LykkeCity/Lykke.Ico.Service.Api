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
using Lykke.Service.IcoApi.Core.Domain.Campaign;

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

            var now = DateTime.UtcNow;
            var campaignActive = false;

            var logiTokenInfo = await settings.GetLogiTokenInfo(_campaignInfoRepository, DateTime.Now);
            var smarcTokenInfo = await settings.GetSmarcTokenInfo(_campaignInfoRepository, DateTime.Now);
            var logiPresaleTokenInfo = await settings.GetLogiTokenInfo(_campaignInfoRepository, CampaignPhase.PreSale);
            var smarcPresaleTokenInfo = await settings.GetSmarcTokenInfo(_campaignInfoRepository, CampaignPhase.PreSale);
            var logiCrowdsaleTokenInfo = await settings.GetLogiTokenInfo(_campaignInfoRepository, CampaignPhase.CrowdSale);
            var smarcCrowdsaleTokenInfo = await settings.GetSmarcTokenInfo(_campaignInfoRepository, CampaignPhase.CrowdSale);

            if (logiPresaleTokenInfo != null || 
                smarcPresaleTokenInfo != null ||
                logiCrowdsaleTokenInfo != null || 
                smarcCrowdsaleTokenInfo != null)
            {
                campaignActive = true;
            }

            response = new CampaignResponse
            {
                CaptchaEnabled = settings.CaptchaEnable,
                CampaignActive = campaignActive,

                SmarcPhase = smarcTokenInfo.Tier,
                SmarcPhaseAmount = smarcTokenInfo.PhaseTokenAmount,
                SmarcPhaseAmountAvailable = smarcTokenInfo.PhaseTokenAmountAvailable,
                SmarcPhasePriceUsd = smarcTokenInfo.PriceUsd,

                SmarcPresaleAmount = smarcPresaleTokenInfo.PhaseTokenAmount,
                SmarcPresaleAmountAvailable = smarcPresaleTokenInfo.PhaseTokenAmountAvailable,
                SmarcPresalePriceUsd = smarcPresaleTokenInfo.PriceUsd,

                SmarcCrowdsaleTier = smarcCrowdsaleTokenInfo.Tier,
                SmarcCrowdsaleAmount = smarcCrowdsaleTokenInfo.PhaseTokenAmount,
                SmarcCrowdsaleAmountAvailable = smarcCrowdsaleTokenInfo.PhaseTokenAmountAvailable,
                SmarcCrowdsalePriceUsd = smarcCrowdsaleTokenInfo.PriceUsd,

                LogiPhase = logiTokenInfo.Tier,
                LogiPhaseAmount = logiTokenInfo.PhaseTokenAmount,
                LogiPhaseAmountAvailable = logiTokenInfo.PhaseTokenAmountAvailable,
                LogiPhasePriceUsd = logiTokenInfo.PriceUsd,

                LogiPresaleAmount = logiPresaleTokenInfo.PhaseTokenAmount,
                LogiPresaleAmountAvailable = logiPresaleTokenInfo.PhaseTokenAmountAvailable,
                LogiPresalePriceUsd = logiPresaleTokenInfo.PriceUsd,

                LogiCrowdsaleTier = logiCrowdsaleTokenInfo.Tier,
                LogiCrowdsaleAmount = logiCrowdsaleTokenInfo.PhaseTokenAmount,
                LogiCrowdsaleAmountAvailable = logiCrowdsaleTokenInfo.PhaseTokenAmountAvailable,
                LogiCrowdsalePriceUsd = logiCrowdsaleTokenInfo.PriceUsd,
            };

            _cache.Set(_key, response, DateTimeOffset.Now.AddSeconds(1));

            return response;
        }
    }
}
