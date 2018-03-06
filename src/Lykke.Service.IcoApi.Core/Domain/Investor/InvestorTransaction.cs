using System;

namespace Lykke.Service.IcoApi.Core.Domain.Investor
{
    public class InvestorTransaction : IInvestorTransaction
    {
        public string Email { get; set; }
        public string UniqueId { get; set; }
        public DateTime ProcessedUtc { get; }
        public DateTime CreatedUtc { get; set; }
        public CurrencyType Currency { get; set; }
        public string BlockId { get; set; }
        public string TransactionId { get; set; }
        public string PayInAddress { get; set; }

        public decimal Amount { get; set; }
        public decimal AmountUsd { get; set; }
        public decimal Fee { get; set; }

        public decimal SmarcAmountToken { get; set; }
        public decimal SmarcAmountUsd { get; set; }
        public decimal SmarcTokenPriceUsd { get; set; }
        public string SmarcTokenPriceContext { get; set; }

        public decimal LogiAmountToken { get; set; }
        public decimal LogiAmountUsd { get; set; }
        public decimal LogiTokenPriceUsd { get; set; }
        public string LogiTokenPriceContext { get; set; }

        public decimal ExchangeRate { get; set; }
        public string ExchangeRateContext { get; set; }
    }
}
