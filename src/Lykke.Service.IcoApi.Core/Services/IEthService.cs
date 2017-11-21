namespace Lykke.Service.IcoApi.Core.Services
{
    public interface IEthService
    {
        bool ValidateAddress(string address);
        string GetAddressByPublicKey(string key);
        string GeneratePublicKey();
    }
}
