using System;

namespace Lykke.Service.IcoApi.Core.Domain.Investor
{
    public interface IInvestorTransactionRefund
    {
        string Email { get; }
        string UniqueId { get; }
        DateTime CreatedUtc { get; }
        string MessageJson { get; set; }
    }
}
