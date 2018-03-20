using System;
using System.Threading.Tasks;

namespace Lykke.Service.IcoApi.Core.Queues
{
    public interface IQueuePublisher<TMessage>
    {
        string GenerateSasUrl(DateTime? expiryTime = null);
        Task SendAsync(TMessage message);
    }
}
