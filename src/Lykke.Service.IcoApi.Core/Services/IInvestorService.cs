using Lykke.Ico.Core.Repositories.Investor;
using System;
using System.Threading.Tasks;

namespace Lykke.Service.IcoApi.Core.Services
{
    public interface IInvestorService
    {
        Task<IInvestor> GetAsync(string email);

        Task RegisterAsync(string email);

        Task<bool> ConfirmAsync(Guid confirmationToken);

        Task<IInvestor> UpdateAsync(string email, string tokenAddress, string refundEthAddress, string refundBtcAddress);

        Task DeleteAsync(string email);
    }
}
