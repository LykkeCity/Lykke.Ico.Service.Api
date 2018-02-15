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
        where TMessage : IMessage
    {
        private readonly IQueueExt _queue;

        public QueuePublisher(IReloadingManager<string> connectionStringManager)
        {
            var t = typeof(TMessage);
            var queueName = GetQueueName(t);

            _queue = AzureQueueExt.Create(connectionStringManager, queueName);
        }

        private string GetQueueName(Type t)
        {
            var metadata = Attribute.GetCustomAttribute(t, typeof(QueueMessageAttribute), true) as QueueMessageAttribute;

            Debug.Assert(metadata != null && !string.IsNullOrWhiteSpace(metadata.QueueName), 
                $"Consider using [QueueMessage] attribute on {t.FullName} type to define queue name explicitly.");

            if (metadata == null || string.IsNullOrWhiteSpace(metadata.QueueName))
            {
                var replacedMessage = t.Name.Replace("Message", string.Empty);
                var splittedByUppercase = Regex.Split(replacedMessage, @"(?<!^)(?=[A-Z])");
                var lowerCased = splittedByUppercase.Select(x => x.ToLowerInvariant());
                var dashed = string.Join("-", lowerCased);

                return dashed;
            }
            else
            {
                return metadata.QueueName;
            }
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
