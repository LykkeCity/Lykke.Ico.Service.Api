using AzureStorage;
using AzureStorage.Tables;
using Common.Log;
using Lykke.Service.IcoApi.Core.Domain;
using Lykke.Service.IcoApi.Core.Domain.Investor;
using Lykke.Service.IcoApi.Core.Repositories;
using Lykke.SettingsReader;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Lykke.Services.IcoApi.AzureRepositories
{
    public class InvestorRepository : IInvestorRepository
    {
        private readonly INoSQLTableStorage<InvestorEntity> _table;
        private static string GetPartitionKey() => "Investor";
        private static string GetRowKey(string email) => email;

        private readonly IInvestorHistoryRepository _investorHistoryRepository;

        public InvestorRepository(IReloadingManager<string> connectionStringManager, ILog log)
        {
            _table = AzureTableStorage<InvestorEntity>.Create(connectionStringManager, "Investors", log);
            _investorHistoryRepository = new InvestorHistoryRepository(connectionStringManager, log);
        }

        public async Task<IEnumerable<IInvestor>> GetAllAsync()
        {
            return await _table.GetDataAsync(GetPartitionKey());
        }

        public async Task<IInvestor> GetAsync(string email)
        {
            return await _table.GetDataAsync(GetPartitionKey(), GetRowKey(email));
        }

        public async Task<IInvestor> AddAsync(string email, Guid confirmationToken)
        {
            var entity = new InvestorEntity
            {
                PartitionKey = GetPartitionKey(),
                RowKey = GetRowKey(email),
                ConfirmationToken = confirmationToken,
                ConfirmationTokenCreatedUtc = DateTime.UtcNow
            };

            await _table.InsertAsync(entity);
            await _investorHistoryRepository.SaveAsync(entity, InvestorHistoryAction.Add);

            return entity;
        }

        public async Task ConfirmAsync(string email)
        {
            var entity = await _table.MergeAsync(GetPartitionKey(), GetRowKey(email), x =>
            {
                x.ConfirmedUtc = DateTime.UtcNow;

                return x;
            });

            await _investorHistoryRepository.SaveAsync(entity, InvestorHistoryAction.Confirm);
        }

        public async Task SavePayInAddressesAsync(string email, InvestorPayInAddresses item)
        {
            var entity = await _table.MergeAsync(GetPartitionKey(), GetRowKey(email), x =>
            {
                x.PayInAssigned = true;

                x.PayInSmarcEthPublicKey = item.PayInSmarcEthPublicKey;
                x.PayInSmarcEthAddress = item.PayInSmarcEthAddress;
                x.PayInSmarcBtcPublicKey = item.PayInSmarcBtcPublicKey;
                x.PayInSmarcBtcAddress = item.PayInSmarcBtcAddress;

                x.PayInLogiEthPublicKey = item.PayInLogiEthPublicKey;
                x.PayInLogiEthAddress = item.PayInLogiEthAddress;
                x.PayInLogiBtcPublicKey = item.PayInLogiBtcPublicKey;
                x.PayInLogiBtcAddress = item.PayInLogiBtcAddress;

                x.PayInSmarc90Logi10EthPublicKey = item.PayInSmarc90Logi10EthPublicKey;
                x.PayInSmarc90Logi10EthAddress = item.PayInSmarc90Logi10EthAddress;
                x.PayInSmarc90Logi10BtcPublicKey = item.PayInSmarc90Logi10BtcPublicKey;
                x.PayInSmarc90Logi10BtcAddress = item.PayInSmarc90Logi10BtcAddress;

                return x;
            });

            await _investorHistoryRepository.SaveAsync(entity, InvestorHistoryAction.SavePayInAddresses);
        }

        public async Task SaveAddressesAsync(string email, string tokenAddress, string refundEthAddress, string refundBtcAddress)
        {
            var entity = await _table.MergeAsync(GetPartitionKey(), GetRowKey(email), x =>
            {
                x.TokenAddress = tokenAddress;
                x.RefundEthAddress = refundEthAddress;
                x.RefundBtcAddress = refundBtcAddress;

                return x;
            });

            await _investorHistoryRepository.SaveAsync(entity, InvestorHistoryAction.SaveAddresses);
        }

        public async Task SaveKycAsync(string email, string kycRequestId)
        {
            var entity = await _table.MergeAsync(GetPartitionKey(), GetRowKey(email), x =>
            {
                x.KycRequestId = kycRequestId;
                x.KycRequestedUtc = DateTime.UtcNow;

                return x;
            });

            await _investorHistoryRepository.SaveAsync(entity, InvestorHistoryAction.SaveKyc);
        }

        public async Task SaveKycResultAsync(string email, bool kycPassed)
        {
            var entity = await _table.MergeAsync(GetPartitionKey(), GetRowKey(email), x =>
            {
                x.KycPassed = kycPassed;
                x.KycPassedUtc = DateTime.UtcNow;

                return x;
            });

            await _investorHistoryRepository.SaveAsync(entity, InvestorHistoryAction.SaveKycResult);
        }

        public async Task IncrementAmount(string email, CurrencyType type, decimal amount, decimal amountUsd, 
            decimal amountSmarcToken, decimal amountLogiToken)
        {
            var entity = await _table.MergeAsync(GetPartitionKey(), GetRowKey(email), x =>
            {
                switch (type)
                {
                    case CurrencyType.Bitcoin:
                        x.AmountBtc += amount;
                        break;
                    case CurrencyType.Ether:
                        x.AmountEth += amount;
                        break;
                    case CurrencyType.Fiat:
                        x.AmountFiat += amount;
                        break;
                }

                x.AmountUsd += amountUsd;
                x.AmountSmarcToken += amountSmarcToken;
                x.AmountLogiToken += amountLogiToken;

                return x;
            });

            await _investorHistoryRepository.SaveAsync(entity, InvestorHistoryAction.IncrementAmount);
        }

        public async Task RemoveAsync(string email)
        {
            await _table.DeleteIfExistAsync(GetPartitionKey(), GetRowKey(email));
        }
    }
}
