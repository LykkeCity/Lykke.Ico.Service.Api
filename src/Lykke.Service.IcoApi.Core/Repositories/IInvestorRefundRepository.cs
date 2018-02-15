using Lykke.Service.IcoApi.Core.Domain.Investor;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Lykke.Service.IcoApi.Core.Repositories
{
    public interface IInvestorRefundRepository
    {
        Task<IEnumerable<IInvestorRefund>> GetAllAsync();
        Task<IEnumerable<IInvestorRefund>> GetByEmailAsync(string email);
        Task SaveAsync(string email, InvestorRefundReason reason, string messageJson);
        Task RemoveAsync(string email);
    }
}
