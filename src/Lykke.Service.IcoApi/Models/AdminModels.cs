using Lykke.Ico.Core;
using Lykke.Ico.Core.Repositories.CryptoInvestment;
using Lykke.Ico.Core.Repositories.EmailHistory;
using Lykke.Ico.Core.Repositories.Investor;
using Lykke.Ico.Core.Repositories.InvestorHistory;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace Lykke.Service.IcoApi.Models
{
    public class FullInvestorResponse
    {
        public string Email { get; set; }

        public string TokenAddress { get; set; }

        public string RefundEthAddress { get; set; }

        public string RefundBtcAddress { get; set; }

        public string PayInEthAddress { get; set; }

        public string PayInBtcAddress { get; set; }

        public Guid? ConfirmationToken { get; set; }

        public DateTime? ConfirmationDateTimeUtc { get; set; }

        public DateTime UpdatedUtc { get; set; }

        public static FullInvestorResponse Create(IInvestor investor)
        {
            return new FullInvestorResponse
            {
                Email = investor.Email,
                TokenAddress = investor.TokenAddress,
                RefundEthAddress = investor.RefundEthAddress,
                RefundBtcAddress = investor.RefundBtcAddress,
                PayInEthAddress = investor.PayInEthAddress,
                PayInBtcAddress = investor.PayInBtcAddress,
                ConfirmationToken = investor.ConfirmationToken,
                ConfirmationDateTimeUtc = investor.ConfirmationDateTimeUtc,
                UpdatedUtc = investor.UpdatedUtc
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

        public static InvestorEmailsResponse Create(IEnumerable<IEmailHistoryItem> emails)
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

        public static InvestorEmailModel Create(IEmailHistoryItem item)
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

        public static InvestorTransactionsResponse Create(IEnumerable<ICryptoInvestment> transactions)
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

        public string InternalId { get; set; }

        public DateTime WhenUtc { get; set; }

        public CurrencyType Currency { get; set; }

        public string Transaction { get; set; }

        public decimal Amount { get; set; }

        public decimal AmountUsd { get; set; }

        public decimal AmountVld { get; set; }

        public decimal ExchangeRate { get; set; }

        public decimal ExchangeRateVldUsd { get; set; }

        public string ExchangeRateContext { get; set; }

        public static InvestorTransactionModel Create(ICryptoInvestment item)
        {
            return new InvestorTransactionModel
            {
                Email = item.InvestorEmail,
                InternalId = item.TransactionId,
                WhenUtc = item.BlockTimestamp,
                Currency = item.CurrencyType,
                Transaction = item.CurrencyType == CurrencyType.Bitcoin ? item.TransactionId.Substring(0, item.TransactionId.IndexOf("-")) : item.TransactionId,
                Amount = item.Amount,
                AmountUsd = item.AmountUsd,
                AmountVld = item.AmountVld,
                ExchangeRateVldUsd = item.Price,
                ExchangeRate = item.ExchangeRate,
                ExchangeRateContext = item.Context
            };
        }
    }

    public enum AssetPair
    {
        BtcUsd,
        EthUsd
    }

    public class SendBtcRequest
    {
        [Required]
        public string BtcAddress { get; set; }

        [Required]
        public decimal Amount { get; set; }
    }
}
