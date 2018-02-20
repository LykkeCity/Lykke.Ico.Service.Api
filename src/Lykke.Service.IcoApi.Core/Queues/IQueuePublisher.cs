using System.Threading.Tasks;

namespace Lykke.Service.IcoApi.Core.Queues
{
    public interface IQueuePublisher<TMessage>
    {
        Task SendAsync(TMessage message);
    }
}
