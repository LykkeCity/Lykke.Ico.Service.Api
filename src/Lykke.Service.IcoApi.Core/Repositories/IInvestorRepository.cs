using Lykke.Service.IcoApi.Core.Domain.Ico;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Lykke.Service.IcoApi.Core.Repositories
{
    public  interface IInvestorRepository
    {
        Task AddAsync(Investor investor);

        Task<IEnumerable<Investor>> GetAllAsync();

        Task<Investor> GetAsync(string email);

        Task RemoveAsync(string email);

        Task UpdateAsync(Investor investor);
    }
}
