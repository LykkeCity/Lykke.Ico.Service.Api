﻿using Lykke.Ico.Core.Repositories.Investor;
using Lykke.Service.IcoApi.Core.Domain;
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

        public string ReferralCode { get; set; }
    }

    public class RegisterInvestorResponse
    {
        public RegisterResult Result { get; set; }
    }

    public class LoginInvestorRequest
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
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

        public string PayInEthAddress { get; set; }

        public string PayInBtcAddress { get; set; }

        public KycStatus KycStatus { get; set; }

        public string KycLink { get; set; }

        public decimal AmountBtc { get; set; }

        public decimal AmountEth { get; set; }

        public decimal AmountFiat { get; set; }

        public decimal AmountUsd { get; set; }

        public decimal AmountToken { get; set; }

        public string ReferralCode { get; set; }

        public string ReferralCodeApplied { get; set; }

        public int ReferralsNumber { get; set; }

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
                PayInEthAddress = investor.PayInEthAddress,
                PayInBtcAddress = investor.PayInBtcAddress,
                KycStatus = kycStatus,
                KycLink = kycLink,
                AmountBtc = investor.AmountBtc,
                AmountEth = investor.AmountEth,
                AmountFiat = investor.AmountFiat,
                AmountUsd = Decimal.Round(investor.AmountUsd, 2, MidpointRounding.AwayFromZero),
                AmountToken = investor.AmountToken,
                ReferralCode = investor.ReferralCode,
                ReferralCodeApplied = investor.ReferralCodeApplied,
                ReferralsNumber = investor.ReferralsNumber
            };
        }
    }

    public enum KycStatus
    {
        None,
        Requested,
        Failed,
        Success
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
