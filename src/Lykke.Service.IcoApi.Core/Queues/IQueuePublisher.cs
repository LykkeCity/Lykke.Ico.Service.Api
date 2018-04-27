using System;
using System.Threading.Tasks;

namespace Lykke.Service.IcoApi.Core.Queues
{
    public interface IQueuePublisher<TMessage>
    {
        string GenerateSasUrl(DateTimeOffset expiryTime);
        Task SendAsync(TMessage message);
    }
}
