using System;

namespace Lykke.Service.IcoApi.Core.Domain.Ico
{
    public class Investor : IInvestor
    {
        public string Email { get; set; }

        public string VldAddress { get; set; }

        public string PayInEthAddress { get; set; }

        public string PayInBtcAddress { get; set; }

        public string RefundEthAddress { get; set; }

        public string RefundBtcAddress { get; set; }

        public DateTime CreationDateTime { get; set; }

        public string IpAddress { get; set; }

        public Guid ConfirmationToken { get; set; }

        public DateTime ConfirmationTokenDateTime { get; set; }

        public Guid? AuthToken { get; set; }

        public DateTime? AuthTokenDateTime { get; set; }
    }
}
