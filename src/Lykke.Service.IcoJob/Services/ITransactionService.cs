using Lykke.Service.IcoApi.Core.Queues.Messages;
using System.Threading.Tasks;

namespace Lykke.Service.IcoJob.Services
{
    public interface ITransactionService
    {
        Task Process(TransactionMessage message);
    }
}
