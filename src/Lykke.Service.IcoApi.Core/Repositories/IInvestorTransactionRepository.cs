using Lykke.Service.IcoApi.Core.Domain.Investor;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Lykke.Service.IcoApi.Core.Repositories
{ 
    public interface IInvestorTransactionRepository
    {
        Task<IEnumerable<IInvestorTransaction>> GetAllAsync();

        Task<IInvestorTransaction> GetAsync(string email, string uniqueId);

        Task<IEnumerable<IInvestorTransaction>> GetByEmailAsync(string email);

        Task SaveAsync(IInvestorTransaction tx);

        Task RemoveAsync(string email);
    }
}
