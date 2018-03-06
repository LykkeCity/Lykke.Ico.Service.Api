﻿using Lykke.Service.IcoApi.Core.Domain;
using Lykke.Service.IcoApi.Core.Domain.Fiat;
using Lykke.Service.IcoApi.Core.Domain.Investor;
using Newtonsoft.Json;
using System;
using System.ComponentModel.DataAnnotations;

namespace Lykke.Service.IcoApi.Models
{
    public class RegisterInvestorRequest
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }

    public class RegisterInvestorResponse
    {
        public RegisterResult Result { get; set; }
    }

    public class ConfirmInvestorRequest
    {
        [Required]
        public string Email { get; set; }

        [Required]
        public string VerificationCode { get; set; }
    }

    public class ConfirmInvestorResponse
    {
        public string AuthToken { get; set; }
    }

    public class InvestorRequest
    {
        [Required]
        public string TokenAddress { get; set; }

        public string RefundEthAddress { get; set; }

        public string RefundBtcAddress { get; set; }
    }

    public class InvestorResponse
    {
        public string Email { get; set; }

        public string TokenAddress { get; set; }
        public string RefundEthAddress { get; set; }
        public string RefundBtcAddress { get; set; }

        public string PayInSmarcEthAddress { get; set; }
        public string PayInSmarcBtcAddress { get; set; }

        public string PayInLogiEthAddress { get; set; }
        public string PayInLogiBtcAddress { get; set; }

        public string PayInSmarc90Logi10EthAddress { get; set; }
        public string PayInSmarc90Logi10BtcAddress { get; set; }

        public KycStatus KycStatus { get; set; }
        public string KycLink { get; set; }

        public decimal AmountBtc { get; set; }
        public decimal AmountEth { get; set; }
        public decimal AmountFiat { get; set; }
        public decimal AmountUsd { get; set; }
        public decimal AmountSmarcToken { get; set; }
        public decimal AmountLogiToken { get; set; }

        public static InvestorResponse Create(IInvestor investor, string kycLink)
        {
            var kycStatus = KycStatus.None;
            if (investor.KycRequestedUtc.HasValue)
            {
                kycStatus = KycStatus.Requested;
            }
            if (investor.KycPassed.HasValue)
            {
                kycStatus = investor.KycPassed.Value ? KycStatus.Success : KycStatus.Failed;
            }

            return new InvestorResponse
            {
                Email = investor.Email,
                TokenAddress = investor.TokenAddress,
                RefundEthAddress = investor.RefundEthAddress,
                RefundBtcAddress = investor.RefundBtcAddress,
                PayInSmarcEthAddress = investor.PayInSmarcEthAddress,
                PayInSmarcBtcAddress = investor.PayInSmarcBtcAddress,
                PayInLogiEthAddress = investor.PayInLogiEthAddress,
                PayInLogiBtcAddress = investor.PayInLogiBtcAddress,
                PayInSmarc90Logi10EthAddress = investor.PayInSmarc90Logi10EthAddress,
                PayInSmarc90Logi10BtcAddress = investor.PayInSmarc90Logi10BtcAddress,
                KycStatus = kycStatus,
                KycLink = kycLink,
                AmountBtc = investor.AmountBtc,
                AmountEth = investor.AmountEth,
                AmountFiat = investor.AmountFiat,
                AmountUsd = Decimal.Round(investor.AmountUsd, 2, MidpointRounding.AwayFromZero),
                AmountSmarcToken = investor.AmountSmarcToken,
                AmountLogiToken = investor.AmountLogiToken
            };
        }
    }

    public enum KycStatus
    {
        None,
        Requested,
        Failed,
        Success,
    }

    public class ChargeInvestorRequest
    {
        [Required]
        public string Token { get; set; }

        /// <summary>
        /// Amount in cents
        /// </summary>
        [Required, Range(1, Int32.MaxValue)]
        public int Amount { get; set; }
    }

    public class ChargeInvestorResponse
    {
        public FiatChargeStatus Status { get; set; }

        public string FailureCode { get; set; }

        public string FailureMessage { get; set; }

        public static ChargeInvestorResponse Create(FiatCharge charge)
        {
            return new ChargeInvestorResponse
            {
                Status = charge.Status,
                FailureCode = charge.FailureCode,
                FailureMessage = charge.FailureMessage
            };
        }
    }
}
