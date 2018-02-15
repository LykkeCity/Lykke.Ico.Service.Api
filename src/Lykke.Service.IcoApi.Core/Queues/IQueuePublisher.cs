using System.Threading.Tasks;

namespace Lykke.Service.IcoApi.Core.Queues
{
    public interface IQueuePublisher<TMessage>
        where TMessage : IMessage
    {
        Task SendAsync(TMessage message);
    }
}
