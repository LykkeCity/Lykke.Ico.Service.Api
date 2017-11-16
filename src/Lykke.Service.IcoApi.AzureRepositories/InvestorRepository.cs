﻿using Lykke.Service.IcoApi.Core.Repositories;
using System;
using System.Collections.Generic;
using Lykke.Service.IcoApi.Core.Domain.Ico;
using System.Threading.Tasks;
using AzureStorage;
using Lykke.Service.IcoApi.AzureRepositories.Entities;
using AutoMapper;
using System.Linq;
using Common.Log;
using Lykke.SettingsReader;
using AzureStorage.Tables;

namespace Lykke.Service.IcoApi.AzureRepositories
{
    public class InvestorRepository : IInvestorRepository
    {
        private readonly INoSQLTableStorage<InvestorEntity> _investorTable;
        private static string GetPartitionKey() => "Investor";
        private static string GetRowKey(string email) => email;

        public InvestorRepository(IReloadingManager<string> connectionStringManager, ILog log)
        {
            _investorTable = AzureTableStorage<InvestorEntity>.Create(connectionStringManager, "Investors", log);
        }

        public async Task AddAsync(Investor investor)
        {
            var entity = Mapper.Map<InvestorEntity>(investor);

            entity.PartitionKey = GetPartitionKey();
            entity.RowKey = GetRowKey(investor.Email);

            await _investorTable.InsertAsync(entity);
        }

        public async Task<IEnumerable<Investor>> GetAllAsync()
        {
            var entities = await _investorTable.GetDataAsync(GetPartitionKey());

            return entities.Select(Mapper.Map<Investor>);
        }

        public async Task<Investor> GetAsync(string email)
        {
            var entity = await _investorTable.GetDataAsync(GetPartitionKey(), GetRowKey(email));

            return Mapper.Map<Investor>(entity);
        }

        public async Task RemoveAsync(string email)
        {
            await _investorTable.DeleteIfExistAsync(GetPartitionKey(), GetRowKey(email));
        }

        public async Task UpdateAsync(Investor investor)
        {
            await _investorTable.MergeAsync(GetPartitionKey(), GetRowKey(investor.Email), x =>
            {
                Mapper.Map(investor, x);

                return x;
            });
        }
    }
}
