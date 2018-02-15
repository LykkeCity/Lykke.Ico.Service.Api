using Lykke.Service.IcoApi.Core.Domain.PrivateInvestor;
using System.Threading.Tasks;

namespace Lykke.Service.IcoApi.Core.Repositories
{
    public interface IPrivateInvestorAttributeRepository
    {
        Task<string> GetInvestorEmailAsync(PrivateInvestorAttributeType type, string value);
        Task RemoveAsync(PrivateInvestorAttributeType type, string email);
        Task SaveAsync(PrivateInvestorAttributeType type, string email, string value);
    }
}
