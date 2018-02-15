using System;
using Lykke.AzureStorage.Tables;
using Lykke.AzureStorage.Tables.Entity.Annotation;
using Lykke.AzureStorage.Tables.Entity.ValueTypesMerging;
using Microsoft.WindowsAzure.Storage.Table;
using Lykke.Service.IcoApi.Core.Domain.Investor;

namespace Lykke.Services.IcoApi.AzureRepositories
{
    [ValueTypeMergingStrategy(ValueTypeMergingStrategy.UpdateAlways)]
    internal class InvestorRefundEntity : AzureTableEntity, IInvestorRefund
    {
        [IgnoreProperty]
        public string Email
        {
            get => PartitionKey;
        }

        [IgnoreProperty]
        public DateTime CreatedUtc
        {
            get => Timestamp.UtcDateTime;
        }

        public InvestorRefundReason Reason { get; set; }
        public string MessageJson { get; set; }
    }
}
