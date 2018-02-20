using System;
using System.Threading.Tasks;
using AzureStorage.Queue;
using System.Linq;
using Common;
using Lykke.SettingsReader;
using System.Text.RegularExpressions;
using System.Diagnostics;
using Lykke.Service.IcoApi.Core.Queues;

namespace Lykke.Service.IcoApi.Services
{
    public class QueuePublisher<TMessage> : IQueuePublisher<TMessage>
    {
        private readonly IQueueExt _queue;

        public QueuePublisher(IReloadingManager<string> connectionStringManager, string queueName)
        {
            _queue = AzureQueueExt.Create(connectionStringManager, queueName);
        }

        public async Task SendAsync(TMessage message)
        {
            if (message == null)
            {
                throw new ArgumentNullException(nameof(message));
            }

            await _queue.PutRawMessageAsync(message.ToJson());
        }            
    }
}
