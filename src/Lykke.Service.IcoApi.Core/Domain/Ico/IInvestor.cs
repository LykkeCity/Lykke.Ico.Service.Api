using System;
using System.Collections.Generic;
using System.Text;

namespace Lykke.Service.IcoApi.Core.Domain.Ico
{
    public interface IInvestor
    {
        string Email { get; }

        string VldAddress { get; }

        string PayInEthAddress { get; }

        string PayInBtcAddress { get; }

        string RefundEthAddress { get; }

        string RefundBtcAddress { get; }

        DateTime CreationDateTime { get; }

        Guid ConfirmationToken { get; }

        DateTime ConfirmationTokenDateTime { get; }

        Guid? AuthToken { get; }

        DateTime? AuthTokenDateTime { get; }

        string IpAddress { get; }
    }
}
