﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AzureStorage;
using AzureStorage.Tables;
using Common.Log;
using Lykke.SettingsReader;
using System.Linq;
using Common;
using Lykke.Service.IcoApi.Core.Repositories;
using Lykke.Service.IcoApi.Core.Domain.Investor;

namespace Lykke.Services.IcoApi.AzureRepositories
{
    public class InvestorHistoryRepository : IInvestorHistoryRepository
    {
        private readonly INoSQLTableStorage<InvestorHistoryEntity> _table;
        private static string GetPartitionKey(string email) => email;
        private static string GetRowKey() => DateTime.UtcNow.ToString("o");

        public InvestorHistoryRepository(IReloadingManager<string> connectionStringManager, ILog log)
        {
            _table = AzureTableStorage<InvestorHistoryEntity>.Create(connectionStringManager, "InvestorHistory", log);
        }

        public async Task<IEnumerable<IInvestorHistoryItem>> GetAsync(string email)
        {
            return await _table.GetDataAsync(GetPartitionKey(email));
        }

        public async Task SaveAsync(IInvestor investor, InvestorHistoryAction action)
        {
            await _table.InsertOrReplaceAsync(new InvestorHistoryEntity
            {
                PartitionKey = GetPartitionKey(investor.Email),
                RowKey = GetRowKey(),
                Action = action,
                Json = investor.ToJson()
            });
        }

        public async Task RemoveAsync(string email)
        {
            var items = await _table.GetDataAsync(GetPartitionKey(email));
            if (items.Any())
            {
                await _table.DeleteAsync(items);
            }
        }
    }
}