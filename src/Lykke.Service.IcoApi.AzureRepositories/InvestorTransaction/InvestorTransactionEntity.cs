﻿using System;
using Lykke.AzureStorage.Tables;
using Lykke.AzureStorage.Tables.Entity.Annotation;
using Lykke.AzureStorage.Tables.Entity.ValueTypesMerging;
using Microsoft.WindowsAzure.Storage.Table;
using Lykke.Service.IcoApi.Core.Domain.Investor;
using Lykke.Service.IcoApi.Core.Domain;

namespace Lykke.Services.IcoApi.AzureRepositories
{
    [ValueTypeMergingStrategy(ValueTypeMergingStrategy.UpdateAlways)]
    internal class InvestorTransactionEntity : AzureTableEntity, IInvestorTransaction
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
        public DateTime ProcessedUtc
        {
            get => Timestamp.DateTime.ToUniversalTime();
        }

        public DateTime CreatedUtc { get; set; }
        public CurrencyType Currency { get; set; }
        public string TransactionId { get; set; }
        public string BlockId { get; set; }
        public string PayInAddress { get; set; }

        public decimal Amount { get; set; }
        public decimal AmountUsd { get; set; }
        public decimal Fee { get; set; }

        public decimal AmountToken { get; set; }
        public decimal TokenPriceUsd { get; set; }
        public string TokenPriceContext { get; set; }

        public decimal ExchangeRate { get; set; }
        public string ExchangeRateContext { get; set; }
    }
}
