using Lykke.Service.IcoApi.Core.Services;
using Lykke.Service.IcoApi.Models;
using Lykke.Service.IcoExRate.Client;
using Lykke.Service.IcoExRate.Client.AutorestClient.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace Lykke.Service.IcoApi.Controllers
{
    [Route("api/exrate")]
    public class ExRateController : Controller
    {
        private readonly IIcoExRateClient _icoExRateClient;
        private readonly ICampaignService _campaignService;
        private readonly IMemoryCache _cache;

        public ExRateController(IIcoExRateClient icoExRateClient,
            ICampaignService campaignService,
            IMemoryCache cache)
        {
            _campaignService = campaignService;
            _icoExRateClient = icoExRateClient;
            _cache = cache;
        }

        /// <summary>
        /// Returns latest exchange rate
        /// </summary>
        [HttpGet("{assetPair}")]
        public async Task<AverageRateResponse> GetLatestRate([Required] AssetPair assetPair)
        {
            var key = $"LatestExRate_{Enum.GetName(typeof(AssetPair), assetPair)}";

            var response = _cache.Get<AverageRateResponse>(key);
            if (response != null)
            {
                return response;
            }

            var pair = Enum.Parse<Pair>(Enum.GetName(typeof(AssetPair), assetPair), true);
            var rate = await _icoExRateClient.GetAverageRate(pair, DateTime.UtcNow);
            var settings = await _campaignService.GetCampaignSettings();

            if (pair == Pair.ETHUSD &&
                rate.AverageRate.HasValue &&
                settings.MinEthExchangeRate.HasValue &&
                rate.AverageRate.Value < (double)settings.MinEthExchangeRate.Value)
            {
                rate.AverageRate = (double)settings.MinEthExchangeRate.Value;
            }

            if (pair == Pair.BTCUSD &&
                rate.AverageRate.HasValue &&
                settings.MinBtcExchangeRate.HasValue &&
                rate.AverageRate.Value < (double)settings.MinBtcExchangeRate.Value)
            {
                rate.AverageRate = (double)settings.MinBtcExchangeRate.Value;
            }

            _cache.Set(key, rate, DateTimeOffset.Now.AddSeconds(5));

            return rate;
        }
    }
}
