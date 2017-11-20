using Lykke.Ico.Core.Contracts.Repositories;
using System;
using System.Threading.Tasks;

namespace Lykke.Service.IcoApi.Core.Services
{
    public interface IInvestorService
    {
        Task<IInvestor> GetAsync(string email);

        Task RegisterAsync(string email, string ipAddress);

        Task<bool> ConfirmAsync(Guid confirmationToken, string ipAddress);

        Task UpdateAddressesAsync(string email, string tokenAddress, string refundEthAddress, string refundBtcAddress);

        Task UpdatePayInAddressesAsync(string email, string payInEthPublicKey, string payInBtcPublicKey);

        Task DeleteAsync(string email);

        Task<IInvestorConfirmation> GetConfirmation(Guid confirmationToken);
    }
}
