using System;

namespace Lykke.Service.IcoApi.Core.Domain.PrivateInvestor
{
    public interface IPrivateInvestor
    {
        string Email { get; }

        DateTime UpdatedUtc { get; set; }

        string KycRequestId { get; set; }

        DateTime? KycRequestedUtc { get; set; }

        bool? KycPassed { get; set; }

        DateTime? KycPassedUtc { get; set; }
    }
}
