using System;
using Microsoft.WindowsAzure.Storage.Table;

namespace Lykke.Service.IcoApi.AzureRepositories.Entities
{
    public class InvestorEntity : TableEntity
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
    }
}
