﻿using System;
using Lykke.AzureStorage.Tables;
using Lykke.AzureStorage.Tables.Entity.Annotation;
using Lykke.AzureStorage.Tables.Entity.ValueTypesMerging;
using Microsoft.WindowsAzure.Storage.Table;
using Lykke.Service.IcoApi.Core.Domain.PrivateInvestor;

namespace Lykke.Services.IcoApi.AzureRepositories
{
    [ValueTypeMergingStrategy(ValueTypeMergingStrategy.UpdateAlways)]
    internal class PrivateInvestorEntity : AzureTableEntity, IPrivateInvestor
    {
        [IgnoreProperty]
        public string Email
        {
            get => RowKey;
        }

        public DateTime UpdatedUtc { get; set; }

        public string KycRequestId { get; set; }

        public DateTime? KycRequestedUtc { get; set; }

        public bool? KycPassed { get; set; }

        public DateTime? KycPassedUtc { get; set; }
    }
}