using System;
using System.Threading.Tasks;
using Common;
using Common.Log;
using Lykke.Service.IcoApi.Core.Services;
using Lykke.Service.IcoApi.Core.Queues.Messages;
using Lykke.JobTriggers.Triggers.Attributes;
using Lykke.Service.IcoApi.Core.Domain;

namespace Lykke.Service.IcoJob.AzureQueueHandlers
{
    public class TransactionQueueHandler
    {
        private ILog _log;
        private ITransactionService _transactionService;

        public TransactionQueueHandler(ILog log, ITransactionService transactionService)
        {
            _log = log;
            _transactionService = transactionService;
        }
        
        [TransactionQueueTrigger(30000)]
        public async Task HandleTransactionMessage(TransactionMessage msg)
        {
            try
            {
                await _transactionService.Process(msg);
            }
            catch (Exception ex)
            {
                await _log.WriteErrorAsync(nameof(HandleTransactionMessage),
                    $"Message: {msg.ToJson()}", ex);

                throw;
            }
        }
    }

    public class TransactionQueueTriggerAttribute : QueueTriggerAttribute
    {
        public TransactionQueueTriggerAttribute(int maxPoolingIntervalMs = -1)
            : base($"{Consts.CAMPAIGN_ID.ToLower()}-transaction", maxPoolingIntervalMs)
        {

        }
    }
}   
