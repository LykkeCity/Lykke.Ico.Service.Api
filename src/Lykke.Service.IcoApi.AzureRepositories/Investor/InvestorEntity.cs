using System;
using Lykke.AzureStorage.Tables;
using Lykke.AzureStorage.Tables.Entity.Annotation;
using Lykke.AzureStorage.Tables.Entity.ValueTypesMerging;
using Microsoft.WindowsAzure.Storage.Table;
using Lykke.Service.IcoApi.Core.Domain.Investor;

namespace Lykke.Services.IcoApi.AzureRepositories
{
    [ValueTypeMergingStrategy(ValueTypeMergingStrategy.UpdateAlways)]
    internal class InvestorEntity : AzureTableEntity, IInvestor
    {
        [IgnoreProperty]
        public string Email
        {
            get => RowKey;
        }

        public string TokenAddress { get; set; }
        public string RefundEthAddress { get; set; }
        public string RefundBtcAddress { get; set; }

        public string Phase { get; set; }
        public DateTime? PhaseUpdatedUtc { get; set; }

        public bool PayInAssigned { get; set; }

        public string PayInSmarcEthPublicKey { get; set; }
        public string PayInSmarcEthAddress { get; set; }
        public string PayInSmarcBtcPublicKey { get; set; }
        public string PayInSmarcBtcAddress { get; set; }

        public string PayInLogiEthPublicKey { get; set; }
        public string PayInLogiEthAddress { get; set; }
        public string PayInLogiBtcPublicKey { get; set; }
        public string PayInLogiBtcAddress { get; set; }

        public string PayInSmarc90Logi10EthPublicKey { get; set; }
        public string PayInSmarc90Logi10EthAddress { get; set; }
        public string PayInSmarc90Logi10BtcPublicKey { get; set; }
        public string PayInSmarc90Logi10BtcAddress { get; set; }

        public Guid? ConfirmationToken { get; set; }
        public DateTime? ConfirmationTokenCreatedUtc { get; set; }
        public DateTime? ConfirmedUtc { get; set; }

        public string KycRequestId { get; set; }
        public DateTime? KycRequestedUtc { get; set; }
        public bool? KycPassed { get; set; }
        public DateTime? KycPassedUtc { get; set; }
        public DateTime? KycManuallyUpdatedUtc { get; set; }

        public decimal AmountBtc { get; set; }
        public decimal AmountEth { get; set; }
        public decimal AmountFiat { get; set; }
        public decimal AmountUsd { get; set; }
        public decimal AmountSmarcToken { get; set; }
        public decimal AmountLogiToken { get; set; }
    }
}
