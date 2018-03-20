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

        public decimal AmountToken { get; set; }
        public decimal TokenPriceUsd { get; set; }
        public string TokenPriceContext { get; set; }

        public decimal ExchangeRate { get; set; }
        public string ExchangeRateContext { get; set; }
    }
}
