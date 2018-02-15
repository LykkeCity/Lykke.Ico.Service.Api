﻿using Lykke.Service.IcoApi.Core.Domain.Investor;
using System;

namespace Lykke.Job.IcoInvestment.Tests
{
    public class Investor : IInvestor
    {
        public string Email { get; set; }

        public string TokenAddress { get; set; }

        public string RefundEthAddress { get; set; }

        public string RefundBtcAddress { get; set; }

        public string PayInEthPublicKey { get; set; }

        public string PayInEthAddress { get; set; }

        public string PayInBtcPublicKey { get; set; }

        public string PayInBtcAddress { get; set; }

        public DateTime UpdatedUtc { get; set; }

        public Guid? ConfirmationToken { get; set; }

        public DateTime? ConfirmationTokenCreatedUtc { get; set; }

        public DateTime? ConfirmedUtc { get; set; }

        public string KycRequestId { get; set; }

        public DateTime? KycRequestedUtc { get; set; }

        public bool? KycPassed { get; set; }

        public DateTime? KycPassedUtc { get; set; }

        public decimal AmountBtc { get; set; }

        public decimal AmountEth { get; set; }

        public decimal AmountFiat { get; set; }

        public decimal AmountUsd { get; set; }

        public decimal AmountToken { get; set; }
    }
}
