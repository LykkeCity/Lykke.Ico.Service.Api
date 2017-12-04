using Lykke.Ico.Core.Repositories.InvestorEmail;
using Lykke.Ico.Core.Repositories.InvestorHistory;
using Lykke.Ico.Core.Repositories.InvestorTransaction;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Lykke.Service.IcoApi.Core.Services
{
    public interface IAdminService
    {
        Task<Dictionary<string, string>> GetCampainInfo();
        Task<IEnumerable<IInvestorHistoryItem>> GetInvestorHistory(string email);
        Task<IEnumerable<IInvestorEmail>> GetInvestorEmails(string email);
        Task DeleteInvestorAsync(string email);
        Task DeleteInvestorHistoryAsync(string email);
        Task<int> ImportPublicKeys(StreamReader reader);
        Task<IEnumerable<IInvestorTransaction>> GetInvestorTransactions(string email);
    }
}
