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
        private readonly IMemoryCache _cache;
        private readonly string _key = "LatestExRate";

        public ExRateController(IIcoExRateClient icoExRateClient,
            IMemoryCache cache)
        {
            _icoExRateClient = icoExRateClient;
            _cache = cache;
        }

        /// <summary>
        /// Returns latest exchange rate
        /// </summary>
        [HttpGet("{assetPair}")]
        public async Task<AverageRateResponse> GetLatestRate([Required] AssetPair assetPair)
        {
            var response = _cache.Get<AverageRateResponse>(_key);
            if (response != null)
            {
                return response;
            }

            var pair = Enum.Parse<Pair>(Enum.GetName(typeof(AssetPair), assetPair), true);
            var rate = await _icoExRateClient.GetAverageRate(pair, DateTime.UtcNow);

            _cache.Set(_key, response, DateTimeOffset.Now.AddSeconds(10));

            return rate;
        }
    }
}
