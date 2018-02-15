using System;

namespace Lykke.Service.IcoApi.Core.Domain.Investor
{
    public interface IInvestorHistoryItem
    {
        string Email { get; }

        DateTime WhenUtc { get; }

        InvestorHistoryAction Action { get; set; }

        string Json { get; set; }
    }
}
