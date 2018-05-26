using System;
using Lykke.AzureStorage.Tables;
using Lykke.AzureStorage.Tables.Entity.Annotation;
using Lykke.AzureStorage.Tables.Entity.ValueTypesMerging;
using Microsoft.WindowsAzure.Storage.Table;
using Lykke.Service.IcoApi.Core.Domain.Investor;

namespace Lykke.Services.IcoApi.AzureRepositories
{
    [ValueTypeMergingStrategy(ValueTypeMergingStrategy.UpdateAlways)]
    internal class InvestorTransactionRefundEntity : AzureTableEntity, IInvestorTransactionRefund
    {
        [IgnoreProperty]
        public string Email
        {
            get => PartitionKey;
        }

        [IgnoreProperty]
        public string UniqueId
        {
            get => RowKey;
        }

        [IgnoreProperty]
        public DateTime CreatedUtc
        {
            get => Timestamp.UtcDateTime;
        }

        public string MessageJson { get; set; }
    }
}
