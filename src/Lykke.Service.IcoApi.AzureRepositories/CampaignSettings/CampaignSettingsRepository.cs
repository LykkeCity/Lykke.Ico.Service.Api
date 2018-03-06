using AzureStorage;
using AzureStorage.Tables;
using Common.Log;
using Lykke.Service.IcoApi.Core.Domain.Campaign;
using Lykke.Service.IcoApi.Core.Repositories;
using Lykke.SettingsReader;
using System;
using System.Threading.Tasks;

namespace Lykke.Services.IcoApi.AzureRepositories
{
    public class CampaignSettingsRepository : ICampaignSettingsRepository
    {
        private readonly INoSQLTableStorage<CampaignSettingsEntity> _table;
        private static string GetPartitionKey() => "";
        private static string GetRowKey() => "Settings";

        public CampaignSettingsRepository(IReloadingManager<string> connectionStringManager, ILog log)
        {
            _table = AzureTableStorage<CampaignSettingsEntity>.Create(connectionStringManager, "CampaignSettings", log);
        }

        public async Task<ICampaignSettings> GetAsync()
        {
            return await _table.GetDataAsync(GetPartitionKey(), GetRowKey());
        }

        public async Task SaveAsync(ICampaignSettings settings)
        {
            await _table.InsertOrMergeAsync(new CampaignSettingsEntity
            {
                PartitionKey = GetPartitionKey(),
                RowKey = GetRowKey(),
                PreSaleStartDateTimeUtc = settings.PreSaleStartDateTimeUtc,
                PreSaleEndDateTimeUtc = settings.PreSaleEndDateTimeUtc,
                PreSaleSmarcAmount = settings.PreSaleSmarcAmount,
                PreSaleLogiAmount = settings.PreSaleLogiAmount,
                PreSaleSmarcPriceUsd = settings.PreSaleSmarcPriceUsd,
                PreSaleLogiPriceUsd = settings.PreSaleLogiPriceUsd,
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
                MinInvestAmountUsd = settings.MinInvestAmountUsd,
                RowndDownTokenDecimals = settings.RowndDownTokenDecimals,
                CaptchaEnable = settings.CaptchaEnable,
                KycEnableRequestSending = settings.KycEnableRequestSending,
                KycCampaignId = settings.KycCampaignId,
                KycLinkTemplate = settings.KycLinkTemplate,
                KycServiceEncriptionKey = settings.KycServiceEncriptionKey,
                KycServiceEncriptionIv = settings.KycServiceEncriptionIv
            });
        }
    }
}
