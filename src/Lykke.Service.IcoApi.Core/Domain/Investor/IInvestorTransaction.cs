﻿using System;

namespace Lykke.Service.IcoApi.Core.Domain.Investor
{
    public interface IInvestorTransaction
    {
        string Email { get; }
        string UniqueId { get; }
        DateTime CreatedUtc { get; set; }
        CurrencyType Currency { get; set; }
        string BlockId { get; set; }
        string TransactionId { get; set; }
        string PayInAddress { get; set; }
        decimal Amount { get; set; }
        decimal AmountUsd { get; set; }
        decimal AmountToken { get; set; }
        decimal Fee { get; set; }
        decimal TokenPrice { get; set; }
        string TokenPriceContext { get; set; }
        decimal ExchangeRate { get; set; }
        string ExchangeRateContext { get; set; }
    }
}