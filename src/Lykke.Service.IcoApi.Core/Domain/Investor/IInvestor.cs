using System;

namespace Lykke.Service.IcoApi.Core.Domain.Investor
{
    public interface IInvestor
    {
        string Email { get; }

        string TokenAddress { get; set; }
        string RefundEthAddress { get; set; }
        string RefundBtcAddress { get; set; }

        bool PayInAssigned { get; set; }

        string PayInSmarcEthPublicKey { get; set; }
        string PayInSmarcEthAddress { get; set; }
        string PayInSmarcBtcPublicKey { get; set; }
        string PayInSmarcBtcAddress { get; set; }

        string PayInLogiEthPublicKey { get; set; }
        string PayInLogiEthAddress { get; set; }
        string PayInLogiBtcPublicKey { get; set; }
        string PayInLogiBtcAddress { get; set; }

        string PayInSmarc90Logi10EthPublicKey { get; set; }
        string PayInSmarc90Logi10EthAddress { get; set; }
        string PayInSmarc90Logi10BtcPublicKey { get; set; }
        string PayInSmarc90Logi10BtcAddress { get; set; }

        Guid? ConfirmationToken { get; set; }
        DateTime? ConfirmationTokenCreatedUtc { get; set; }
        DateTime? ConfirmedUtc { get; set; }

        string KycRequestId { get; set; }
        DateTime? KycRequestedUtc { get; set; }
        bool? KycPassed { get; set; }
        DateTime? KycPassedUtc { get; set; }
        DateTime? KycManuallyUpdatedUtc { get; set; }

        decimal AmountBtc { get; set; }
        decimal AmountEth { get; set; }
        decimal AmountFiat { get; set; }
        decimal AmountUsd { get; set; }
        decimal AmountSmarcToken { get; set; }
        decimal AmountLogiToken { get; set; }
    }
}
