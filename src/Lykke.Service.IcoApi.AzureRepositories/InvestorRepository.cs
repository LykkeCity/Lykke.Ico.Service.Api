using Lykke.Service.IcoApi.Core.Repositories;
using System;
using System.Collections.Generic;
using Lykke.Service.IcoApi.Core.Domain.Ico;
using System.Threading.Tasks;
using AzureStorage;
using Lykke.Service.IcoApi.AzureRepositories.Entities;
using AutoMapper;
using System.Linq;

namespace Lykke.Service.IcoApi.AzureRepositories
{
    public class InvestorRepository : IInvestorRepository
    {
        private readonly INoSQLTableStorage<InvestorEntity> _investorTable;
        private static string GetPartitionKey() => "Investor";
        private static string GetRowKey(string email) => email;

        public InvestorRepository(INoSQLTableStorage<InvestorEntity> investorTable)
        {
            _investorTable = investorTable;
        }

        public async Task AddAsync(IInvestor investor)
        {
            var entity = Mapper.Map<InvestorEntity>(investor);

            entity.PartitionKey = GetPartitionKey();
            entity.RowKey = GetRowKey(investor.Email);

            await _investorTable.InsertAsync(entity);
        }

        public async Task<IEnumerable<IInvestor>> GetAllAsync()
        {
            var entities = await _investorTable.GetDataAsync(GetPartitionKey());

            return entities.Select(Mapper.Map<Investor>);
        }

        public async Task<IInvestor> GetAsync(string email)
        {
            var entity = await _investorTable.GetDataAsync(GetPartitionKey(), GetRowKey(email));

            return Mapper.Map<Investor>(entity);
        }

        public async Task RemoveAsync(string email)
        {
            await _investorTable.DeleteIfExistAsync(GetPartitionKey(), GetRowKey(email));
        }

        public async Task UpdateAsync(IInvestor investor)
        {
            await _investorTable.MergeAsync(GetPartitionKey(), GetRowKey(investor.Email), x =>
            {
                Mapper.Map(investor, x);

                return x;
            });
        }
    }
}
