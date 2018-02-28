using Lykke.Ico.Core;
using Lykke.Ico.Core.Repositories.Investor;
using Lykke.Ico.Core.Repositories.InvestorEmail;
using Lykke.Ico.Core.Repositories.InvestorHistory;
using Lykke.Ico.Core.Repositories.InvestorRefund;
using Lykke.Ico.Core.Repositories.InvestorTransaction;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Lykke.Service.IcoApi.Core.Services
{
    public interface IAdminService
    {
        Task<Dictionary<string, string>> GetCampaignInfo();
        Task<IEnumerable<IInvestor>> GetAllInvestors();
        Task<IEnumerable<IInvestorTransaction>> GetAllInvestorTransactions();
        Task<IEnumerable<IInvestorRefund>> GetAllInvestorFailedTransactions();
        Task<IEnumerable<IInvestorHistoryItem>> GetInvestorHistory(string email);
        Task<IEnumerable<IInvestorEmail>> GetInvestorEmails(string email);
        Task DeleteInvestorAsync(string email);
        Task DeleteInvestorAllDataAsync(string email);
        Task<int> ImportPublicKeys(StreamReader reader);
        Task<IEnumerable<IInvestorTransaction>> GetInvestorTransactions(string email);
        Task<IEnumerable<IInvestorRefund>> GetInvestorRefunds(string email);
        Task<IEnumerable<IInvestorRefund>> GetRefunds();
        Task ResendRefunds();
        Task<string> SendTransactionMessageAsync(string email, CurrencyType currency, 
            DateTime? createdUtc, string uniqueId, decimal amount);
        Task<IEnumerable<(int Id, string BtcPublicKey, string EthPublicKey)>> GetPublicKeys(int[] ids);
        Task<IEnumerable<IInvestorTransaction>> GetLatestTransactions();
        Task SendKycReminderEmails(IEnumerable<IInvestor> investors);
        Task UpdateInvestorAsync(string email, string tokenAddress, string refundEthAddress, string refundBtcAddress);
        Task UpdateInvestorKycAsync(IInvestor investor, bool? kycPassed);
        Task<string> Recalculate20MTxs(bool saveChanges);
    }
}
