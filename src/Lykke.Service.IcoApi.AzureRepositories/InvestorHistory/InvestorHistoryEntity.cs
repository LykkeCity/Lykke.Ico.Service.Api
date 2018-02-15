using Lykke.AzureStorage.Tables;
using Lykke.Service.IcoApi.Core.Domain.Investor;
using Microsoft.WindowsAzure.Storage.Table;
using System;

namespace Lykke.Services.IcoApi.AzureRepositories
{
    internal class InvestorHistoryEntity : AzureTableEntity, IInvestorHistoryItem
    {
        [IgnoreProperty]
        public string Email { get => PartitionKey; }

        [IgnoreProperty]
        public DateTime WhenUtc { get => Timestamp.UtcDateTime; }

        public InvestorHistoryAction Action { get; set; }

        public string Json { get; set; }
    }
}
