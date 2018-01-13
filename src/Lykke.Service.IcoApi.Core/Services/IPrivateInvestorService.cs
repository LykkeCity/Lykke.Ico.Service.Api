using System;
using System.Threading.Tasks;
using Lykke.Ico.Core.Repositories.PrivateInvestor;

namespace Lykke.Service.IcoApi.Core.Services
{
    public interface IPrivateInvestorService
    {
        Task CreateAsync(string email);
        Task<IPrivateInvestor> GetAsync(string email);
        Task<string> GetEmailByKycId(Guid kycId);
        Task RequestKycAsync(string email);
        Task SaveKycResultAsync(string email, string kycStatus);
    }
}
