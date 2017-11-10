using Lykke.Service.IcoApi.Core.Services;
using System;
using Lykke.Service.IcoApi.Core.Domain.Ico;
using System.Threading.Tasks;
using Lykke.Service.IcoApi.Core.Repositories;
using System.Collections.Generic;

namespace Lykke.Service.IcoApi.Services
{
    public class InvestorService : IInvestorService
    {
        private readonly IInvestorRepository _investorRepository;

        public InvestorService(IInvestorRepository investorRepository)
        {
            _investorRepository = investorRepository;
        }

        public async Task RegisterAsync(string email)
        {
            var investor = await _investorRepository.GetAsync(email);
            if (investor == null)
            {
                var token = Guid.NewGuid();

                await _investorRepository.AddAsync(new Investor
                {
                    Email = email,
                    ConfirmationToken = token,
                    CreationDateTime = DateTime.Now
                });

                //TODO: send confirmation email

                return;
            }

            //TODO: send welcome back email
        }

        public async Task<IInvestor> GetAsync(string email)
        {
            return await _investorRepository.GetAsync(email);
        }

        public async Task DeleteAsync(string email)
        {
            await _investorRepository.RemoveAsync(email);
        }

        public async Task<IEnumerable<IInvestor>> GetAllAsync()
        {
            return await _investorRepository.GetAllAsync();
        }
    }
}
