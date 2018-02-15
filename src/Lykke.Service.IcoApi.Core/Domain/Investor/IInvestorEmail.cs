using System;

namespace Lykke.Service.IcoApi.Core.Domain.Investor
{
    public interface IInvestorEmail
    {
        string Email { get; }

        DateTime WhenUtc { get; }

        string Type { get; }

        string Subject { get; set; }

        string Body { get; set; }
    }
}
