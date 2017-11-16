using Lykke.Service.IcoApi.Core.Services;
using System.Threading.Tasks;
using System.Collections.Generic;
using Lykke.Ico.Core.Repositories.Investor;
using Lykke.Ico.Core.Contracts.Queues;
using Lykke.Ico.Core.Services;
using Common.Log;
using Lykke.Ico.Core.Contracts.Repositories;
using Common;

namespace Lykke.Service.IcoApi.Services
{
    public class InvestorService : IInvestorService
    {
        private readonly IInvestorRepository _investorRepository;
        private readonly IQueuePublisher<InvestorConfirmationMessage> _queuePublisher;
        private readonly ILog _log;

        public InvestorService(IInvestorRepository investorRepository, IQueuePublisher<InvestorConfirmationMessage> queuePublisher, 
            ILog log)
        {
            _investorRepository = investorRepository;
            _queuePublisher = queuePublisher;
            _log = log;
        }

        public async Task RegisterAsync(string email)
        {
            await _log.WriteInfoAsync(nameof(InvestorService), nameof(RegisterAsync), 
                $"Register investor: {email}");

            var investor = await _investorRepository.GetAsync(email);
            if (investor == null)
            {
                var newInvestor = await _investorRepository.AddAsync(email, "");
                await _log.WriteInfoAsync(nameof(InvestorService), nameof(RegisterAsync), 
                    $"New investor: {newInvestor.ToJson()}");

                var message = InvestorConfirmationMessage.Create(email, newInvestor.ConfirmationToken);
                await _log.WriteInfoAsync(nameof(InvestorService), nameof(RegisterAsync), 
                    $"Send InvestorConfirmationMessage: {message.ToJson()}");
                await _queuePublisher.SendAsync(message);

                return;
            }

            await _log.WriteInfoAsync(nameof(InvestorService), nameof(RegisterAsync), 
                $"Exisitng investor: {investor.ToJson()}");

            if (string.IsNullOrEmpty(investor.TokenAddress))
            {
                var message = InvestorConfirmationMessage.Create(email, investor.ConfirmationToken);

                await _log.WriteInfoAsync(nameof(InvestorService), nameof(RegisterAsync), 
                    $"TokenAddress is empty. Send InvestorConfirmationMessage: {message.ToJson()}");
                await _queuePublisher.SendAsync(message);

                return;
            }

            //TODO: send investor summary email
        }

        public async Task<IInvestor> GetAsync(string email)
        {
            return await _investorRepository.GetAsync(email);
        }

        public async Task DeleteAsync(string email)
        {
            await _investorRepository.RemoveAsync(email);
        }
    }
}
