namespace Lykke.Service.IcoApi.Core.Services
{
    public interface IBtcService
    {
        bool ValidateAddress(string address);
        string GetAddressByPublicKey(string key);
        string GetAddressByPublicKey(string key, string btcNetwork);
        string GeneratePublicKey();
        decimal GetBalance(string address);
        string SendToAddress(string address, decimal amount);
    }
}
