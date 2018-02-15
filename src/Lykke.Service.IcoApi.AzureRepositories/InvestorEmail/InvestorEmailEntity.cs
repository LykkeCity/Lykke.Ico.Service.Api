using Lykke.AzureStorage.Tables;
using Lykke.Service.IcoApi.Core.Domain.Investor;
using Microsoft.WindowsAzure.Storage.Table;
using System;

namespace Lykke.Services.IcoApi.AzureRepositories
{
    internal class InvestorEmailEntity : AzureTableEntity, IInvestorEmail
    {
        [IgnoreProperty]
        public string Email { get => PartitionKey; }

        [IgnoreProperty]
        public DateTime WhenUtc { get => Timestamp.UtcDateTime; }

        public string Type { get; set; }

        public string Subject { get; set; }

        public string Body { get; set; }
    }
}
