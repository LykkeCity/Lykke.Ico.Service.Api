using Lykke.Service.IcoApi.Core.Domain;
using Lykke.Service.IcoApi.Core.Domain.Investor;
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
        Task DeleteInvestorAsync(string email);
        Task DeleteInvestorAllDataAsync(string email);
        Task<int> ImportPublicKeys(StreamReader reader);
        Task<IEnumerable<IInvestorTransaction>> GetInvestorTransactions(string email);
        Task<IEnumerable<IInvestorRefund>> GetInvestorRefunds(string email);
        Task<IEnumerable<IInvestorRefund>> GetRefunds();
        Task<string> SendTransactionMessageAsync(string email, string payInAddress,
            CurrencyType currency, DateTime? createdUtc, string uniqueId, decimal amount);
        Task<IEnumerable<(int Id, string BtcPublicKey, string EthPublicKey)>> GetPublicKeys(int[] ids);
        Task<IEnumerable<IInvestorTransaction>> GetLatestTransactions();
        Task UpdateInvestorAsync(string email, string tokenAddress, string refundEthAddress, string refundBtcAddress);
        string GenerateTransactionQueueSasUrl(DateTime? expiryTime = null);
        Task SendKycReminderEmails(IEnumerable<IInvestor> investors);
    }
}
