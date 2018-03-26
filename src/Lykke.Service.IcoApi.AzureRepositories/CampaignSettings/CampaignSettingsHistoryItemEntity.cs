using System;
using System.Globalization;
using Common;
using Lykke.AzureStorage.Tables;
using Lykke.AzureStorage.Tables.Entity.Annotation;
using Lykke.AzureStorage.Tables.Entity.ValueTypesMerging;
using Lykke.Service.IcoApi.Core.Domain.Campaign;
using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Lykke.Services.IcoApi.AzureRepositories
{
    [ValueTypeMergingStrategy(ValueTypeMergingStrategy.UpdateAlways)]
    public class CampaignSettingsHistoryItemEntity : AzureTableEntity, ICampaignSettingsHistoryItem
    {
        public CampaignSettingsHistoryItemEntity()
        {
        }

        public CampaignSettingsHistoryItemEntity(string username, ICampaignSettings settings)
        {
            ChangedUtc = DateTime.UtcNow;
            Username = username;
            Settings = JsonConvert.SerializeObject(settings,
                Formatting.Indented,
                new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Ignore,
                    ContractResolver = new CamelCasePropertyNamesContractResolver()
                });
        }

        [IgnoreProperty]
        public DateTime ChangedUtc
        {
            get => DateTime.ParseExact(PartitionKey, "O", CultureInfo.InvariantCulture);
            set => PartitionKey = value.ToString("O");
        }

        [IgnoreProperty]
        public string Username
        {
            get => RowKey;
            set => RowKey = value;
        }

        public string Settings
        {
            get;
            set;
        }
    }
}
