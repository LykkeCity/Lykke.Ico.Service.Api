using System;

namespace Lykke.Service.IcoApi.Core.Domain.Ico
{
    public class Investor
    {
        public string Email { get; set; }

        public string VldAddress { get; set; }

        public string PayInEthPublicKey { get; set; }

        public string PayInBtcPublicKey { get; set; }

        public string RefundEthAddress { get; set; }

        public string RefundBtcAddress { get; set; }

        public DateTime CreationDateTime { get; set; }

        public string IpAddress { get; set; }

        public Guid ConfirmationToken { get; set; }

        public static Investor Create(string email, string ipAddress)
        {
            return new Investor
            {
                Email = email,
                ConfirmationToken = Guid.NewGuid(),
                IpAddress = ipAddress
            };
        }
    }
}
