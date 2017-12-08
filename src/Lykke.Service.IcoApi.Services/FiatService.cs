using Common;
using Common.Log;
using Lykke.Ico.Core;
using Lykke.Ico.Core.Queues;
using Lykke.Ico.Core.Queues.Transactions;
using Lykke.Service.IcoApi.Core.Services;
using System;
using System.Threading.Tasks;

namespace Lykke.Service.IcoApi.Services
{
    public class FiatService : IFiatService
    {
        private readonly ILog _log;
        private readonly IQueuePublisher<TransactionMessage> _transactionQueuePublisher;

        public FiatService(ILog log,
            IQueuePublisher<TransactionMessage> transactionQueuePublisher)
        {
            _log = log;
            _transactionQueuePublisher = transactionQueuePublisher;
        }

        public async Task SendTxMessageAsync(string email, DateTime createdUtc, string transactionId, 
            decimal amount, decimal fee)
        {
            var message = new TransactionMessage
            {
                Email = email,
                Amount = amount,
                CreatedUtc = createdUtc,
                Currency = CurrencyType.Fiat,
                Fee = fee,
                TransactionId = transactionId,
                UniqueId = transactionId
            };

            await _log.WriteInfoAsync(nameof(FiatService), nameof(SendTxMessageAsync), 
                $"Send TransactionMessage: {message.ToJson()}");

            await _transactionQueuePublisher.SendAsync(message);
        }
    }
}
