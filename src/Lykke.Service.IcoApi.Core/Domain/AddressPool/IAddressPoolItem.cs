namespace Lykke.Service.IcoApi.Core.Domain.AddressPool
{
    public interface IAddressPoolItem
    {
        int Id { get; }
        string EthPublicKey { get; set; }
        string BtcPublicKey { get; set; }
    }
}
