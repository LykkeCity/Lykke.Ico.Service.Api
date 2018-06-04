using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using AzureStorage;
using AzureStorage.Tables;
using Common.Log;
using Lykke.Service.IcoApi.Core.Domain.Campaign;
using Lykke.Service.IcoApi.Core.Repositories;
using Lykke.SettingsReader;

namespace Lykke.Services.IcoApi.AzureRepositories
{
    public class CampaignSettingsRepository : ICampaignSettingsRepository
    {
        private readonly INoSQLTableStorage<CampaignSettingsEntity> _table;
        private readonly INoSQLTableStorage<CampaignSettingsHistoryItemEntity> _history;

        private static string GetPartitionKey() => "";
        private static string GetRowKey() => "Settings";
        private static PropertyInfo[] _properties = typeof(ICampaignSettings).GetProperties();

        private bool AreDifferent(ICampaignSettings a, ICampaignSettings b)
        {
            return _properties.Any(p =>
            {
                var va = p.GetValue(a, null);
                var vb = p.GetValue(b, null);
                return (va != null && !va.Equals(vb)) || (vb != null && !vb.Equals(va));
            });
        }

        public CampaignSettingsRepository(IReloadingManager<string> connectionStringManager, ILog log)
        {
            _table = AzureTableStorage<CampaignSettingsEntity>.Create(connectionStringManager, "CampaignSettings", log);
            _history = AzureTableStorage<CampaignSettingsHistoryItemEntity>.Create(connectionStringManager, "CampaignSettingsHistory", log);
        }

        public async Task<ICampaignSettings> GetAsync()
        {
            return await _table.GetDataAsync(GetPartitionKey(), GetRowKey());
        }

        public async Task<IEnumerable<ICampaignSettingsHistoryItem>> GetHistoryAsync() 
        {
            return await _history.GetDataAsync();
        }

        public async Task SaveAsync(ICampaignSettings settings, string username)
        {
            var old = await _table.GetDataAsync(GetPartitionKey(), GetRowKey());
            
            if (old == null || AreDifferent(old, settings))
            {
                await _table.InsertOrReplaceAsync(new CampaignSettingsEntity
                {
                    PartitionKey = GetPartitionKey(),
                    RowKey = GetRowKey(),

                    PreSaleMinInvestAmountUsd = settings.PreSaleMinInvestAmountUsd,
                    PreSaleStartDateTimeUtc = settings.PreSaleStartDateTimeUtc,
                    PreSaleEndDateTimeUtc = settings.PreSaleEndDateTimeUtc,
                    PreSaleSmarcAmount = settings.PreSaleSmarcAmount,
                    PreSaleLogiAmount = settings.PreSaleLogiAmount,
                    PreSaleSmarcPriceUsd = settings.PreSaleSmarcPriceUsd,
                    PreSaleLogiPriceUsd = settings.PreSaleLogiPriceUsd,

                    CrowdSaleMinInvestAmountUsd = settings.CrowdSaleMinInvestAmountUsd,
                    CrowdSaleStartDateTimeUtc = settings.CrowdSaleStartDateTimeUtc,
                    CrowdSaleEndDateTimeUtc = settings.CrowdSaleEndDateTimeUtc,
                    CrowdSale1stTierSmarcPriceUsd = settings.CrowdSale1stTierSmarcPriceUsd,
                    CrowdSale1stTierSmarcAmount = settings.CrowdSale1stTierSmarcAmount,
                    CrowdSale1stTierLogiPriceUsd = settings.CrowdSale1stTierLogiPriceUsd,
                    CrowdSale1stTierLogiAmount = settings.CrowdSale1stTierLogiAmount,
                    CrowdSale2ndTierSmarcPriceUsd = settings.CrowdSale2ndTierSmarcPriceUsd,
                    CrowdSale2ndTierSmarcAmount = settings.CrowdSale2ndTierSmarcAmount,
                    CrowdSale2ndTierLogiPriceUsd = settings.CrowdSale2ndTierLogiPriceUsd,
                    CrowdSale2ndTierLogiAmount = settings.CrowdSale2ndTierLogiAmount,
                    CrowdSale3rdTierSmarcPriceUsd = settings.CrowdSale3rdTierSmarcPriceUsd,
                    CrowdSale3rdTierSmarcAmount = settings.CrowdSale3rdTierSmarcAmount,
                    CrowdSale3rdTierLogiPriceUsd = settings.CrowdSale3rdTierLogiPriceUsd,
                    CrowdSale3rdTierLogiAmount = settings.CrowdSale3rdTierLogiAmount,

                    RowndDownTokenDecimals = settings.RowndDownTokenDecimals,
                    EnableFrontEnd = settings.EnableFrontEnd,
                    MinEthExchangeRate = settings.MinEthExchangeRate,
                    MinBtcExchangeRate = settings.MinBtcExchangeRate,
                    CaptchaEnable = settings.CaptchaEnable,
                    CaptchaSecret = settings.CaptchaSecret,

                    KycEnableRequestSending = settings.KycEnableRequestSending,
                    KycCampaignId = settings.KycCampaignId,
                    KycLinkTemplate = settings.KycLinkTemplate,
                    KycServiceEncriptionKey = settings.KycServiceEncriptionKey,
                    KycServiceEncriptionIv = settings.KycServiceEncriptionIv
                });

                await _history.InsertAsync(new CampaignSettingsHistoryItemEntity(username, settings));
            }
        }
    }
}
