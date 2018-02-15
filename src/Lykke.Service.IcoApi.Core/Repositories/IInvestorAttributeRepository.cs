using Lykke.Service.IcoApi.Core.Domain.Investor;
using System.Threading.Tasks;

namespace Lykke.Service.IcoApi.Core.Repositories
{
    public interface IInvestorAttributeRepository
    {
        Task<string> GetInvestorEmailAsync(InvestorAttributeType type, string value);

        Task SaveAsync(InvestorAttributeType type, string email, string value);

        Task RemoveAsync(InvestorAttributeType type, string email);
    }
}
