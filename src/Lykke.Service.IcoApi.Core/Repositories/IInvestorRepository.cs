using Lykke.Service.IcoApi.Core.Domain.Ico;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Lykke.Service.IcoApi.Core.Repositories
{
    public  interface IInvestorRepository
    {
        Task AddAsync(IInvestor investor);

        Task<IEnumerable<IInvestor>> GetAllAsync();

        Task<IInvestor> GetAsync(string email);

        Task RemoveAsync(string email);

        Task UpdateAsync(IInvestor investor);
    }
}
