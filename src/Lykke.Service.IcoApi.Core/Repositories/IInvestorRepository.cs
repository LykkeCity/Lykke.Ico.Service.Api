using Lykke.Service.IcoApi.Core.Domain;
using Lykke.Service.IcoApi.Core.Domain.Investor;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Lykke.Service.IcoApi.Core.Repositories
{
    public interface IInvestorRepository
    {
        Task<IEnumerable<IInvestor>> GetAllAsync();

        Task<IInvestor> GetAsync(string email);

        Task<IInvestor> AddAsync(string email, Guid confirmationToken);

        Task ConfirmAsync(string email);

        Task SaveAddressesAsync(string email, string tokenAddress, string refundEthAddress, string refundBtcAddress);

        Task SaveAddressesAsync(string email, InvestorFillIn item);

        Task IncrementAmount(string email, CurrencyType type, decimal amount, decimal amountUsd, decimal amountSmarcToken, decimal amountLogiToken);

        Task SaveKycAsync(string email, string kycRequestId);

        Task SaveKycResultAsync(string email, bool kycPassed);

        Task RemoveAsync(string email);
    }
}
