using Lykke.AzureStorage.Tables;
using Lykke.Service.IcoApi.Core.Domain.AddressPool;
using Microsoft.WindowsAzure.Storage.Table;
using System;

namespace Lykke.Services.IcoApi.AzureRepositories
{
    internal class AddressPoolHistoryEntity : AzureTableEntity, IAddressPoolHistoryItem
    {
        [IgnoreProperty]
        public int Id { get => Int32.Parse(RowKey.TrimStart(new char[] { '0' })); }

        [IgnoreProperty]
        public DateTime CreatedUtc { get => Timestamp.UtcDateTime; }

        public string Email { get; set; }

        public string EthPublicKey { get; set; }

        public string BtcPublicKey { get; set; }
    }
}
