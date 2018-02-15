using System;

namespace Lykke.Service.IcoApi.Core.Domain.Investor
{
    public interface IInvestorRefund
    {
        string Email { get; }
        DateTime CreatedUtc { get; }
        InvestorRefundReason Reason { get; set; }
        string MessageJson { get; set; }
    }
}
