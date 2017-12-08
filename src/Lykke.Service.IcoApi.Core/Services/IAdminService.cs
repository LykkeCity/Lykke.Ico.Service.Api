using Lykke.Ico.Core;
using Lykke.Ico.Core.Repositories.CampaignSettings;
using Lykke.Ico.Core.Repositories.InvestorEmail;
using Lykke.Ico.Core.Repositories.InvestorHistory;
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
        Task<ICampaignSettings> GetCampaignSettings();
        Task SaveCampaignSettings(ICampaignSettings settings);
        Task<IEnumerable<IInvestorHistoryItem>> GetInvestorHistory(string email);
        Task<IEnumerable<IInvestorEmail>> GetInvestorEmails(string email);
        Task DeleteInvestorAsync(string email);
        Task DeleteInvestorHistoryAsync(string email);
        Task<int> ImportPublicKeys(StreamReader reader);
        Task<IEnumerable<IInvestorTransaction>> GetInvestorTransactions(string email);
        Task<string> SendTransactionMessageAsync(string email, CurrencyType currency, DateTime createdUtc, 
            string transactionId, decimal amount, decimal fee = 0M);
    }
}
