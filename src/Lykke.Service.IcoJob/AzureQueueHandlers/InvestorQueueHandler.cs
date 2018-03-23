using System;
using System.Threading.Tasks;
using Common;
using Common.Log;
using Lykke.Service.IcoApi.Core.Queues.Messages;
using Lykke.Service.IcoJob.Services;

namespace Lykke.Service.IcoJob.AzureQueueHandlers
{
    public class InvestorQueueHandler
    {
        private ILog _log;
        private readonly IInvestorService _investorService;

        public InvestorQueueHandler(ILog log, IInvestorService investorService)
        {
            _log = log;
            _investorService = investorService;
        }

        [TransactionQueueTrigger(5000)]
        public async Task HandleTransactionMessage(InvestorMessage msg)
        {
            try
            {
                await _investorService.AssignPayInAddresses(msg.Email);
            }
            catch (Exception ex)
            {
                await _log.WriteErrorAsync(nameof(HandleTransactionMessage),
                    $"Message: {msg.ToJson()}", ex);

                throw;
            }
        }
    }
}
