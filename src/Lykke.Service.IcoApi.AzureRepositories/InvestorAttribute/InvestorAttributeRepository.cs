﻿using System;
using System.Threading.Tasks;
using AzureStorage;
using AzureStorage.Tables;
using Lykke.SettingsReader;
using Common.Log;
using Lykke.Service.IcoApi.Core.Repositories;
using Lykke.Service.IcoApi.Core.Domain.Investor;

namespace Lykke.Services.IcoApi.AzureRepositories
{
    public class InvestorAttributeRepository : IInvestorAttributeRepository
    {
        private readonly INoSQLTableStorage<InvestorAttributeEntity> _table;
        private static string GetPartitionKey(InvestorAttributeType type) => Enum.GetName(typeof(InvestorAttributeType), type);
        private static string GetRowKey(string value) => value;

        public InvestorAttributeRepository(IReloadingManager<string> connectionStringManager, ILog log)
        {
            _table = AzureTableStorage<InvestorAttributeEntity>.Create(connectionStringManager, "InvestorAttributes", log);
        }

        public async Task<string> GetInvestorEmailAsync(InvestorAttributeType type, string value)
        {
            var attribute = await _table.GetDataAsync(GetPartitionKey(type), GetRowKey(value));

            return attribute?.Email;
        }

        public async Task SaveAsync(InvestorAttributeType type, string email, string value)
        {
            var entity = InvestorAttributeEntity.Create(email);

            entity.PartitionKey = GetPartitionKey(type);
            entity.RowKey = GetRowKey(value);

            await _table.InsertOrMergeAsync(entity);
        }

        public async Task RemoveAsync(InvestorAttributeType type, string email)
        {
            await _table.DeleteIfExistAsync(GetPartitionKey(type), GetRowKey(email));
        }
    }
}