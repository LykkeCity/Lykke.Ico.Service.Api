using Lykke.Ico.Core.Repositories.Investor;
using Lykke.Service.IcoApi.Core.Domain;
using System;
using System.Threading.Tasks;

namespace Lykke.Service.IcoApi.Core.Services
{
    public interface IInvestorService
    {
        Task<IInvestor> GetAsync(string email);

        Task<string> GetEmailByKycId(Guid kycId);

        Task<RegisterResult> RegisterAsync(string email, string referralCode);

        Task<bool> ConfirmAsync(Guid confirmationToken);

        Task UpdateAsync(string email, string tokenAddress, string refundEthAddress, string refundBtcAddress);

        Task SaveKycResultAsync(string email, string kycStatus);

        Task SendSummaryEmail(IInvestor investor);
    }
}
