using Lykke.Ico.Core.Repositories.Investor;
using Lykke.Service.IcoApi.Core.Domain;
using System;
using System.Threading.Tasks;

namespace Lykke.Service.IcoApi.Core.Services
{
    public interface IInvestorService
    {
        Task<IInvestor> GetAsync(string email);

        Task<RegisterResult> RegisterAsync(string email);

        Task<bool> ConfirmAsync(Guid confirmationToken);

        Task UpdateAsync(string email, string tokenAddress, string refundEthAddress, string refundBtcAddress);

        Task DeleteAsync(string email);
    }
}
