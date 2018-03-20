﻿using System.Collections.Generic;
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

        private static bool AreDifferent(ICampaignSettings a, ICampaignSettings b)
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
            return (await _table.GetDataAsync(GetPartitionKey(), GetRowKey())) ?? new CampaignSettingsEntity();
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
                await _table.InsertOrMergeAsync(new CampaignSettingsEntity
                {
                    PartitionKey = GetPartitionKey(),
                    RowKey = GetRowKey(),
                    PreSaleStartDateTimeUtc = settings.PreSaleStartDateTimeUtc,
                    PreSaleEndDateTimeUtc = settings.PreSaleEndDateTimeUtc,
                    PreSaleTokenAmount = settings.PreSaleTokenAmount,
                    PreSaleTokenPriceUsd = settings.PreSaleTokenPriceUsd,
                    CrowdSaleStartDateTimeUtc = settings.CrowdSaleStartDateTimeUtc,
                    CrowdSaleEndDateTimeUtc = settings.CrowdSaleEndDateTimeUtc,
                    CrowdSale1stTierTokenPriceUsd = settings.CrowdSale1stTierTokenPriceUsd,
                    CrowdSale1stTierTokenAmount = settings.CrowdSale1stTierTokenAmount,
                    CrowdSale2ndTierTokenPriceUsd = settings.CrowdSale2ndTierTokenPriceUsd,
                    CrowdSale2ndTierTokenAmount = settings.CrowdSale2ndTierTokenAmount,
                    CrowdSale3rdTierTokenPriceUsd = settings.CrowdSale3rdTierTokenPriceUsd,
                    CrowdSale3rdTierTokenAmount = settings.CrowdSale3rdTierTokenAmount,
                    MinInvestAmountUsd = settings.MinInvestAmountUsd,
                    RowndDownTokenDecimals = settings.RowndDownTokenDecimals,
                    EnableFrontEnd = settings.EnableFrontEnd,
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
