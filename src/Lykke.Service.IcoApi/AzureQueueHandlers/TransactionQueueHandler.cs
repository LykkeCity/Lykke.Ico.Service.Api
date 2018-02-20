using System.Threading.Tasks;
using Common.Log;
using System;
using Common;
using Lykke.Service.IcoApi.Core.Services;
using Lykke.Service.IcoApi.Core.Domain;
using Lykke.Service.IcoApi.Core.Queues.Messages;
using Lykke.JobTriggers.Triggers.Attributes;

namespace Lykke.Job.IcoInvestment.AzureQueueHandlers
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
        
        [QueueTrigger(Consts.Transactions.Queues.Investor)]
        public async Task HandleTransactionMessage(TransactionMessage msg)
        {
            try
            {
                await _transactionService.Process(msg);
            }
            catch (Exception ex)
            {
                await _log.WriteErrorAsync(nameof(HandleTransactionMessage),
                    $"Message: {msg.ToJson()}",
                    ex);

                throw;
            }
        }
    }
}   
