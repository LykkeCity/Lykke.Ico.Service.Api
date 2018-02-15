using System;

namespace Lykke.Service.IcoApi.Core.Domain.AddressPool
{
    public interface IAddressPoolHistoryItem
    {
        int Id { get; }

        string Email { get; }

        DateTime CreatedUtc { get; }

        string EthPublicKey { get; set; }

        string BtcPublicKey { get; set; }
    }
}
