using System;
using System.Threading.Tasks;
using AzureStorage.Queue;
using Common;
using Lykke.Service.IcoApi.Core.Queues;
using Lykke.SettingsReader;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Queue;

namespace Lykke.Service.IcoApi.Services
{
    public class QueuePublisher<TMessage> : IQueuePublisher<TMessage>
    {
        private readonly IReloadingManager<string> _connectionStringManager;
        private readonly IQueueExt _queue;

        public QueuePublisher(IReloadingManager<string> connectionStringManager, string queueName)
        {
            _connectionStringManager = connectionStringManager;
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

        public string GenerateSasUrl(DateTime? expiryTime = null)
        {
            var storageAccount = CloudStorageAccount.Parse(_connectionStringManager.CurrentValue);

            var policy = new SharedAccessQueuePolicy()
            {
                Permissions = SharedAccessQueuePermissions.Add,
                SharedAccessExpiryTime = expiryTime ?? DateTime.UtcNow.AddYears(1)
            };

            var cloudQueue = storageAccount.CreateCloudQueueClient()
                .GetQueueReference(_queue.Name);

            return 
                cloudQueue.Uri + 
                cloudQueue.GetSharedAccessSignature(policy);
        }
    }
}
