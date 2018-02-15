using Lykke.Service.IcoApi.Core.Queues.Transactions;
using System.Threading.Tasks;

namespace Lykke.Service.IcoApi.Core.Services
{
    public interface ITransactionService
    {
        Task Process(TransactionMessage message);
    }
}
