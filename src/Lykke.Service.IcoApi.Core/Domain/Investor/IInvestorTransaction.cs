using System;

namespace Lykke.Service.IcoApi.Core.Domain.Investor
{
    public interface IInvestorTransaction
    {
        string Email { get; }
        string UniqueId { get; }
        DateTime ProcessedUtc { get; }
        DateTime CreatedUtc { get; set; }
        CurrencyType Currency { get; set; }
        string BlockId { get; set; }
        string TransactionId { get; set; }
        string PayInAddress { get; set; }

        decimal Amount { get; set; }
        decimal AmountUsd { get; set; }
        decimal Fee { get; set; }

        decimal SmarcAmountUsd { get; set; }
        decimal SmarcAmountToken { get; set; }
        decimal SmarcTokenPriceUsd { get; set; }
        string SmarcTokenPriceContext { get; set; }

        decimal LogiAmountUsd { get; set; }
        decimal LogiAmountToken { get; set; }
        decimal LogiTokenPriceUsd { get; set; }
        string LogiTokenPriceContext { get; set; }

        decimal ExchangeRate { get; set; }
        string ExchangeRateContext { get; set; }
    }
}
