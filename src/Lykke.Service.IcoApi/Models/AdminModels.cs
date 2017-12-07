using Lykke.Ico.Core;
using Lykke.Ico.Core.Repositories.CampaignSettings;
using Lykke.Ico.Core.Repositories.Investor;
using Lykke.Ico.Core.Repositories.InvestorEmail;
using Lykke.Ico.Core.Repositories.InvestorHistory;
using Lykke.Ico.Core.Repositories.InvestorTransaction;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace Lykke.Service.IcoApi.Models
{
    public class FullInvestorResponse : IInvestor
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

        public static FullInvestorResponse Create(IInvestor investor)
        {
            return new FullInvestorResponse
            {
                Email = investor.Email,
                TokenAddress = investor.TokenAddress,
                RefundEthAddress = investor.RefundEthAddress,
                RefundBtcAddress = investor.RefundBtcAddress,
                PayInEthAddress = investor.PayInEthAddress,
                PayInEthPublicKey = investor.PayInEthPublicKey,
                PayInBtcAddress = investor.PayInBtcAddress,
                PayInBtcPublicKey = investor.PayInBtcPublicKey,
                UpdatedUtc = investor.UpdatedUtc,
                ConfirmationToken = investor.ConfirmationToken,
                ConfirmationTokenCreatedUtc = investor.ConfirmationTokenCreatedUtc,
                ConfirmedUtc = investor.ConfirmedUtc,
                KycRequestId = investor.KycRequestId,
                KycRequestedUtc = investor.KycRequestedUtc,
                KycPassed = investor.KycPassed,
                KycPassedUtc = investor.KycPassedUtc,
                AmountBtc = investor.AmountBtc,
                AmountEth = investor.AmountEth,
                AmountFiat = investor.AmountFiat,
                AmountUsd = investor.AmountUsd,
                AmountToken = investor.AmountToken
            };
        }
    }

    public class InvestorHistoryResponse
    {
        public InvestorHistoryModel[] Items { get; set; }

        public static InvestorHistoryResponse Create(IEnumerable<IInvestorHistoryItem> items)
        {
            return new InvestorHistoryResponse { Items = items.Select(f => InvestorHistoryModel.Create(f)).ToArray() };
        }
    }

    public class InvestorHistoryModel : IInvestorHistoryItem
    {
        public string Email { get; set; }

        public DateTime WhenUtc { get; set; }

        public InvestorHistoryAction Action { get; set; }

        public string Json { get; set; }

        public static InvestorHistoryModel Create(IInvestorHistoryItem item)
        {
            return new InvestorHistoryModel
            {
                Email = item.Email,
                WhenUtc = item.WhenUtc,
                Action = item.Action,
                Json = item.Json
            };
        }
    }

    public class InvestorEmailsResponse
    {
        public InvestorEmailModel[] Emails { get; set; }

        public static InvestorEmailsResponse Create(IEnumerable<IInvestorEmail> emails)
        {
            return new InvestorEmailsResponse { Emails = emails.Select(f => InvestorEmailModel.Create(f)).ToArray() };
        }
    }

    public class InvestorEmailModel
    {
        public string Email { get; set; }

        public DateTime WhenUtc { get; set; }

        public string Type { get; set; }

        public string Subject { get; set; }

        public string Body { get; set; }

        public static InvestorEmailModel Create(IInvestorEmail item)
        {
            return new InvestorEmailModel
            {
                Email = item.Email,
                WhenUtc = item.WhenUtc,
                Type = item.Type,
                Subject = item.Subject,
                Body = item.Body
            };
        }
    }

    public class InvestorTransactionsResponse
    {
        public InvestorTransactionModel[] Transactions { get; set; }

        public static InvestorTransactionsResponse Create(IEnumerable<IInvestorTransaction> transactions)
        {
            return new InvestorTransactionsResponse
            {
                Transactions = transactions.Select(f => InvestorTransactionModel.Create(f)).ToArray()
            };
        }
    }

    public class InvestorTransactionModel
    {
        public string Email { get; set; }

        public string UniqueId { get; set; }

        public string TransactionId { get; set; }

        public DateTime CreatedUtc { get; set; }

        public CurrencyType Currency { get; set; }

        public decimal Amount { get; set; }

        public decimal AmountUsd { get; set; }

        public decimal AmountToken { get; set; }

        public decimal Fee { get; set; }

        public decimal TokenPrice { get; set; }

        public decimal ExchangeRate { get; set; }

        public string ExchangeRateContext { get; set; }

        public static InvestorTransactionModel Create(IInvestorTransaction item)
        {
            return new InvestorTransactionModel
            {
                Email = item.Email,
                UniqueId = item.UniqueId,
                CreatedUtc = item.CreatedUtc,
                Currency = item.Currency,
                TransactionId = item.TransactionId,
                Amount = item.Amount,
                AmountUsd = item.AmountUsd,
                AmountToken = item.AmountToken,
                Fee = item.Fee,
                TokenPrice = item.TokenPrice,
                ExchangeRate = item.ExchangeRate,
                ExchangeRateContext = item.ExchangeRateContext
            };
        }
    }

    public enum AssetPair
    {
        BtcUsd,
        EthUsd
    }

    public class SendMoneyRequest
    {
        [Required]
        public string Address { get; set; }

        [Required]
        public decimal Amount { get; set; }
    }

    public class CampaignSettingsModel : ICampaignSettings
    {
        [Required]
        public DateTime StartDateTimeUtc { get; set; }

        [Required]
        public DateTime EndDateTimeUtc { get; set; }

        [Required]
        public int TotalTokensAmount { get; set; }

        [Required]
        public decimal TokenBasePriceUsd { get; set; }

        [Required]
        public int TokenDecimals { get; set; }

        [Required]
        public decimal MinInvestAmountUsd { get; set; }

        public static CampaignSettingsModel Create(ICampaignSettings settings)
        {
            if (settings == null)
            {
                return null;
            }

            return new CampaignSettingsModel
            {
                StartDateTimeUtc = settings.StartDateTimeUtc,
                EndDateTimeUtc = settings.EndDateTimeUtc,
                TotalTokensAmount = settings.TotalTokensAmount,
                TokenBasePriceUsd = settings.TokenBasePriceUsd,
                TokenDecimals = settings.TokenDecimals,
                MinInvestAmountUsd = settings.MinInvestAmountUsd
            };
        }
    }
}
