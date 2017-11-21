namespace Lykke.Service.IcoApi.Core.Services
{
    public interface IBtcService
    {
        bool ValidateAddress(string address);
        string GetAddressByPublicKey(string key);
        string GeneratePublicKey();
    }
}
