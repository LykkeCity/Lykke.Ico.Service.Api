using Lykke.Service.IcoApi.Core.Domain.Investor;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Lykke.Service.IcoApi.Core.Repositories
{
    public interface IInvestorHistoryRepository
    {
        Task<IEnumerable<IInvestorHistoryItem>> GetAsync(string email);

        Task SaveAsync(IInvestor investor, InvestorHistoryAction action);

        Task RemoveAsync(string email);
    }
}
