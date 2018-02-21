using System;
using System.Threading.Tasks;
using Lykke.Ico.Core.Repositories.PrivateInvestor;
using System.Collections.Generic;

namespace Lykke.Service.IcoApi.Core.Services
{
    public interface IPrivateInvestorService
    {
        Task CreateAsync(string email);
        Task<IPrivateInvestor> GetAsync(string email);
        Task<IEnumerable<IPrivateInvestor>> GetAllAsync();
        Task<string> GetEmailByKycId(Guid kycId);
        Task RequestKycAsync(string email);
        Task SaveKycResultAsync(string email, string kycStatus);
        Task RemoveAsync(string email, string kycId);
    }
}
