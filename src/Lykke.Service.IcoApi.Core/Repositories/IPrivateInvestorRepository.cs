using Lykke.Service.IcoApi.Core.Domain.PrivateInvestor;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Lykke.Service.IcoApi.Core.Repositories
{
    public interface IPrivateInvestorRepository
    {
        Task<IPrivateInvestor> AddAsync(string email);
        Task<IEnumerable<IPrivateInvestor>> GetAllAsync();
        Task<IPrivateInvestor> GetAsync(string email);
        Task SaveKycAsync(string email, string kycRequestId);
        Task SaveKycResultAsync(string email, bool kycPassed);
        Task RemoveAsync(string email);
    }
}
