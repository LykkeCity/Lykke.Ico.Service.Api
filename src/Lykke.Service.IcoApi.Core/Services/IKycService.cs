using System.Threading.Tasks;

namespace Lykke.Service.IcoApi.Core.Services
{
    public interface IKycService
    {
        Task<string> GetKycLink(string email, string kycId);
        Task<string> Encrypt(string message);
        Task<string> Decrypt(string message);
    }
}
