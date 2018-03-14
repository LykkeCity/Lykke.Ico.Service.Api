using System;
using Lykke.AzureStorage.Tables;
using Lykke.AzureStorage.Tables.Entity.Annotation;
using Lykke.AzureStorage.Tables.Entity.ValueTypesMerging;
using Lykke.Service.IcoApi.Core.Domain.Campaign;

namespace Lykke.Services.IcoApi.AzureRepositories
{
    [ValueTypeMergingStrategy(ValueTypeMergingStrategy.UpdateAlways)]
    public class CampaignSettingsHistoryItemEntity : AzureTableEntity, ICampaignSettingsHistoryItem
    {
        public string Username { get; set; }
        public string Settings { get; set; }
        public DateTime ChangedUtc { get => Timestamp.UtcDateTime; }
    }
}
