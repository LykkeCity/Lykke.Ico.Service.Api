﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Lykke.Service.IcoApi.Core.Domain;
using Lykke.Service.IcoApi.Core.Domain.AddressPool;
using Lykke.Service.IcoApi.Core.Domain.Campaign;
using Lykke.Service.IcoApi.Core.Domain.Investor;
using EmailTemplateModel = Lykke.Service.IcoCommon.Client.Models.EmailTemplateModel;

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
        public decimal AmountToken { get; set; }

        public static FullInvestorResponse Create(IInvestor investor)
        {
            return new FullInvestorResponse
            {
                Email = investor.Email,
                TokenAddress = investor.TokenAddress,
                RefundEthAddress = investor.RefundEthAddress,
                RefundBtcAddress = investor.RefundBtcAddress,
                PayInEthPublicKey = investor.PayInEthPublicKey,
                PayInEthAddress = investor.PayInEthAddress,
                PayInBtcPublicKey = investor.PayInBtcPublicKey,
                PayInBtcAddress = investor.PayInBtcAddress,
                ConfirmationToken = investor.ConfirmationToken,
                ConfirmationTokenCreatedUtc = investor.ConfirmationTokenCreatedUtc,
                ConfirmedUtc = investor.ConfirmedUtc,
                KycRequestId = investor.KycRequestId,
                KycRequestedUtc = investor.KycRequestedUtc,
                KycPassed = investor.KycPassed,
                KycPassedUtc = investor.KycPassedUtc,
                KycManuallyUpdatedUtc = investor.KycManuallyUpdatedUtc,
                AmountBtc = investor.AmountBtc,
                AmountEth = investor.AmountEth,
                AmountFiat = investor.AmountFiat,
                AmountUsd = investor.AmountUsd,
                AmountToken = investor.AmountToken,
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

        public static InvestorEmailsResponse Create(IEnumerable<IcoCommon.Client.Models.EmailModel> emails)
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

        public static InvestorEmailModel Create(IcoCommon.Client.Models.EmailModel email)
        {
            return new InvestorEmailModel
            {
                Email = email.To,
                WhenUtc = email.SentUtc,
                Type = email.TemplateId,
                Subject = email.Subject,
                Body = email.Body
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
        public decimal Fee { get; set; }

        public decimal AmountToken { get; set; }
        public decimal TokenPrice { get; set; }
        public string TokenPriceContext { get; set; }

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
                Fee = item.Fee,
                AmountToken = item.AmountToken,
                TokenPrice = item.TokenPriceUsd,
                TokenPriceContext = item.TokenPriceContext,
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
        public DateTime PreSaleStartDateTimeUtc { get; set; }
        [Required]
        public DateTime PreSaleEndDateTimeUtc { get; set; }
        [Required]
        public decimal PreSaleTokenAmount { get; set; }
        [Required]
        public decimal PreSaleTokenPriceUsd { get; set; }

        [Required]
        public DateTime CrowdSaleStartDateTimeUtc { get; set; }
        [Required]
        public DateTime CrowdSaleEndDateTimeUtc { get; set; }

        [Required]
        public decimal CrowdSale1stTierTokenPriceUsd { get; set; }
        [Required]
        public decimal CrowdSale1stTierTokenAmount { get; set; }

        [Required]
        public decimal CrowdSale2ndTierTokenPriceUsd { get; set; }
        [Required]
        public decimal CrowdSale2ndTierTokenAmount { get; set; }

        [Required]
        public decimal CrowdSale3rdTierTokenPriceUsd { get; set; }
        [Required]
        public decimal CrowdSale3rdTierTokenAmount { get; set; }

        [Required]
        public decimal MinInvestAmountUsd { get; set; }
        [Required]
        public int RowndDownTokenDecimals { get; set; }
        [Required]
        public bool EnableFrontEnd { get; set; }

        [Required]
        public bool CaptchaEnable { get; set; }
        public string CaptchaSecret { get; set; }

        [Required]
        public bool KycEnableRequestSending { get; set; }
        public string KycCampaignId { get; set; }
        public string KycLinkTemplate { get; set; }
        public string KycServiceEncriptionKey { get; set; }
        public string KycServiceEncriptionIv { get; set; }

        public static CampaignSettingsModel Create(ICampaignSettings settings)
        {
            if (settings == null)
            {
                return null;
            }

            return new CampaignSettingsModel
            {
                PreSaleStartDateTimeUtc = settings.PreSaleStartDateTimeUtc,
                PreSaleEndDateTimeUtc = settings.PreSaleEndDateTimeUtc,
                PreSaleTokenAmount = settings.PreSaleTokenAmount,
                PreSaleTokenPriceUsd = settings.PreSaleTokenPriceUsd,
                CrowdSaleStartDateTimeUtc = settings.CrowdSaleStartDateTimeUtc,
                CrowdSaleEndDateTimeUtc = settings.CrowdSaleEndDateTimeUtc,
                CrowdSale1stTierTokenPriceUsd = settings.CrowdSale1stTierTokenPriceUsd,
                CrowdSale1stTierTokenAmount = settings.CrowdSale1stTierTokenAmount,
                CrowdSale2ndTierTokenPriceUsd = settings.CrowdSale2ndTierTokenPriceUsd,
                CrowdSale2ndTierTokenAmount = settings.CrowdSale2ndTierTokenAmount,
                CrowdSale3rdTierTokenPriceUsd = settings.CrowdSale3rdTierTokenPriceUsd,
                CrowdSale3rdTierTokenAmount = settings.CrowdSale3rdTierTokenAmount,
                MinInvestAmountUsd = settings.MinInvestAmountUsd,
                RowndDownTokenDecimals = settings.RowndDownTokenDecimals,
                EnableFrontEnd = settings.EnableFrontEnd,
                CaptchaEnable = settings.CaptchaEnable,
                CaptchaSecret = settings.CaptchaSecret,
                KycEnableRequestSending = settings.KycEnableRequestSending,
                KycCampaignId = settings.KycCampaignId,
                KycLinkTemplate = settings.KycLinkTemplate,
                KycServiceEncriptionKey = settings.KycServiceEncriptionKey,
                KycServiceEncriptionIv = settings.KycServiceEncriptionIv
            };
        }
    }

    public class CampaignSettingsHistoryItemModel : ICampaignSettingsHistoryItem
    {
        public string Username { get; set; }
        public string Settings { get; set; }
        public DateTime ChangedUtc { get; set; }

        public static CampaignSettingsHistoryItemModel Create(ICampaignSettingsHistoryItem historyItem)
        {
            if (historyItem == null)
            {
                return null;
            }

            return new CampaignSettingsHistoryItemModel
            {
                Username = historyItem.Username,
                Settings = historyItem.Settings,
                ChangedUtc = historyItem.ChangedUtc
            };
        }
    }

        public class TransactionMessageRequest
    {
        [Required]
        public string Email { get; set; }

        [Required]
        public decimal Amount { get; set; }

        [Required]
        public string PayInAddress { get; set; }

        /// <summary>
        /// If null then DateTime.UtcNow is used
        /// </summary>
        public DateTime? CreatedUtc { get; set; }

        /// <summary>
        /// If null then automatically generated as Guid string
        /// </summary>
        public string UniqueId { get; set; }
    }

    public class InvestorAddressPoolHistoryResponse
    {
        public AddressPoolHistoryItemModel[] Items { get; set; }

        public static InvestorAddressPoolHistoryResponse Create(IEnumerable<IAddressPoolHistoryItem> items)
        {
            return new InvestorAddressPoolHistoryResponse
            {
                Items = items.Select(f => AddressPoolHistoryItemModel.Create(f)).ToArray()
            };
        }
    }

    public class AddressPoolHistoryItemModel
    {
        public string Email { get; set; }

        public DateTime WhenUtc { get; set; }

        public string EthPublicKey { get; set; }

        public string BtcPublicKey { get; set; }

        public static AddressPoolHistoryItemModel Create(IAddressPoolHistoryItem item)
        {
            return new AddressPoolHistoryItemModel
            {
                Email = item.Email,
                WhenUtc = item.CreatedUtc,
                EthPublicKey = item.EthPublicKey,
                BtcPublicKey = item.BtcPublicKey
            };
        }
    }

    public class InvestorFailedTransactionsResponse
    {
        public InvestorFailedTransaction[] Items { get; set; }

        public static InvestorFailedTransactionsResponse Create(IEnumerable<IInvestorRefund> transactions)
        {
            return new InvestorFailedTransactionsResponse
            {
                Items = transactions.Select(f => InvestorFailedTransaction.Create(f)).ToArray()
            };
        }
    }

    public class InvestorFailedTransaction : IInvestorRefund
    {
        public string Email { get; set; }

        public DateTime CreatedUtc { get; set; }

        public InvestorRefundReason Reason { get; set; }

        public string MessageJson { get; set; }


        public static InvestorFailedTransaction Create(IInvestorRefund item)
        {
            return new InvestorFailedTransaction
            {
                Email = item.Email,
                CreatedUtc = item.CreatedUtc,
                Reason = item.Reason,
                MessageJson = item.MessageJson
            };
        }
    }

    public class EncryptionMessageRequest
    {
        [Required]
        public string Message { get; set; }
    }

    public class PoolKeysResponse
    {
        public PoolKeysResponse()
        {
            Keys = new List<PoolKeysModel>();
        }

        public List<PoolKeysModel> Keys { get; set; }
    }

    public class PoolKeysModel
    {
        public int Id { get; set; }

        public string BtcPublicKey { get; set; }

        public string EthPublicKey { get; set; }
    }

    public class LoginRequest
    {
        [Required]
        public string Username { get; set; }

        [Required]
        public string Password { get; set; }
    }

    public class EmailTemplateDataModel : EmailTemplateModel
    {
        public EmailTemplateDataModel()
        {
        }

        public EmailTemplateDataModel(EmailTemplateModel emailTemplate)
        {
            CampaignId = emailTemplate.CampaignId;
            TemplateId = emailTemplate.TemplateId;
            Body = emailTemplate.Body;
            Subject = emailTemplate.Subject;
            IsLayout = emailTemplate.IsLayout;
        }

        public object Data { get; set; }
    }

    public class GenerateTransactionQueueSasUrlRequest
    {
        public DateTime? ExpiryTime { get; set; }
    }
}
