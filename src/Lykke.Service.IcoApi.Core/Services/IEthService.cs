using System.Threading.Tasks;

namespace Lykke.Service.IcoApi.Core.Services
{
    public interface IEthService
    {
        bool ValidateAddress(string address);
        string GetAddressByPublicKey(string key);
        string GeneratePublicKey();
        Task<decimal> GetBalance(string address);
        Task<string> SendToAddress(string address, decimal amount);
    }
}
