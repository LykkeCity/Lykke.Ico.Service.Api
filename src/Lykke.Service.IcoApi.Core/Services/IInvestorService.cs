using Lykke.Service.IcoApi.Core.Domain.Ico;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Lykke.Service.IcoApi.Core.Services
{
    public interface IInvestorService
    {
        Task RegisterAsync(string email);

        Task DeleteAsync(string email);

        Task<IInvestor> GetAsync(string email);

        Task<IEnumerable<IInvestor>> GetAllAsync();
    }
}
