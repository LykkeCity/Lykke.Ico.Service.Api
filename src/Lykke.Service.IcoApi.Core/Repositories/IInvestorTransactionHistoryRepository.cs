using System.Threading.Tasks;

namespace Lykke.Service.IcoApi.Core.Repositories
{
    public interface IInvestorTransactionHistoryRepository
    {
        Task SaveAsync(string email, string reason, string messageJson);
    }
}
