using Lykke.Service.IcoApi.Core.Services;
using System;
using Lykke.Service.IcoApi.Core.Domain.Ico;
using System.Threading.Tasks;
using Lykke.Service.IcoApi.Core.Repositories;
using System.Collections.Generic;
using Lykke.Ico.Core.Services;
using Lykke.Ico.Core.Contracts.Emails;

namespace Lykke.Service.IcoApi.Services
{
    public class InvestorService : IInvestorService
    {
        private readonly IInvestorRepository _investorRepository;
        private readonly IEmailsQueuePublisher<InvestorConfirmation> _emailsQueuePublisher;

        public InvestorService(IInvestorRepository investorRepository, IEmailsQueuePublisher<InvestorConfirmation> emailsQueuePublisher)
        {
            _investorRepository = investorRepository;
            _emailsQueuePublisher = emailsQueuePublisher;
        }

        public async Task RegisterAsync(string email)
        {
            var investor = await _investorRepository.GetAsync(email);
            if (investor == null)
            {
                investor = Investor.Create(email, "");

                await _investorRepository.AddAsync(investor);

                await _emailsQueuePublisher.SendAsync(new InvestorConfirmation
                {
                    EmailTo = email,
                    ConfirmationToken = investor.ConfirmationToken.ToString(),
                    Attempts = 0
                });

                return;
            }

            if (string.IsNullOrEmpty(investor.VldAddress))
            {
                await _emailsQueuePublisher.SendAsync(new InvestorConfirmation
                {
                    EmailTo = email,
                    ConfirmationToken = investor.ConfirmationToken.ToString(),
                    Attempts = 0
                });

                return;
            }

            //TODO: send investor summary email
        }

        public async Task<Investor> GetAsync(string email)
        {
            return await _investorRepository.GetAsync(email);
        }

        public async Task DeleteAsync(string email)
        {
            await _investorRepository.RemoveAsync(email);
        }

        public async Task<IEnumerable<Investor>> GetAllAsync()
        {
            return await _investorRepository.GetAllAsync();
        }
    }
}
