using System.Collections.Generic;
using System.Threading.Tasks;
using Lykke.Service.IcoApi.Core.Domain.Investor;

namespace Lykke.Service.IcoApi.Core.Repositories
{
    public interface IInvestorTransactionRefundRepository
    {
        Task<IInvestorTransactionRefund> GetAsync(string email, string uniqueId);
        Task<IEnumerable<IInvestorTransactionRefund>> GetAllAsync();
        Task<IEnumerable<IInvestorTransactionRefund>> GetByEmailAsync(string email);
        Task SaveAsync(string email, string uniqueId, string messageJson);
    }
}
