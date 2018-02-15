using Common;
using Common.Log;
using Lykke.Service.IcoApi.Core.Domain.PrivateInvestor;
using Lykke.Service.IcoApi.Core.Repositories;
using Lykke.Service.IcoApi.Core.Services;
using System;
using System.Threading.Tasks;

namespace Lykke.Service.IcoApi.Services
{
    public class PrivateInvestorService : IPrivateInvestorService
    {
        private readonly ILog _log;
        private readonly IPrivateInvestorRepository _privateInvestorRepository;
        private readonly IPrivateInvestorAttributeRepository _privateInvestorAttributeRepository;

        public PrivateInvestorService(ILog log,
            IPrivateInvestorRepository privateInvestorRepository,
            IPrivateInvestorAttributeRepository privateInvestorAttributeRepository)
        {
            _log = log;
            _privateInvestorRepository = privateInvestorRepository;
            _privateInvestorAttributeRepository = privateInvestorAttributeRepository;
        }

        public async Task<IPrivateInvestor> GetAsync(string email)
        {
            email = email.ToLowCase();

            return await _privateInvestorRepository.GetAsync(email);
        }

        public async Task CreateAsync(string email)
        {
            email = email.ToLowCase();

            await _log.WriteInfoAsync(nameof(InvestorService), nameof(CreateAsync),
                $"email={email}", "Create private investor");

            await _privateInvestorRepository.AddAsync(email);
        }

        public async Task RequestKycAsync(string email)
        {
            email = email.ToLowCase();

            var kycId = Guid.NewGuid().ToString();

            await _log.WriteInfoAsync(nameof(InvestorService), nameof(RequestKycAsync),
                $"email={email}, kycId={kycId}", "Request KYC for private investor");

            await _privateInvestorRepository.SaveKycAsync(email, kycId);
            await _privateInvestorAttributeRepository.SaveAsync(PrivateInvestorAttributeType.KycId, email, kycId);
        }

        public async Task<string> GetEmailByKycId(Guid kycId)
        {
            return await _privateInvestorAttributeRepository.GetInvestorEmailAsync(
                PrivateInvestorAttributeType.KycId,
                kycId.ToString());
        }

        public async Task SaveKycResultAsync(string email, string kycStatus)
        {
            email = email.ToLowCase();

            var kycPassed = kycStatus.ToString().ToUpper() == "OK";

            await _privateInvestorRepository.SaveKycResultAsync(email, kycPassed);
        }
    }
}
