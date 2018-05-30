using System;
using System.Threading.Tasks;
using AzureStorage;
using AzureStorage.Tables;
using Common.Log;
using Lykke.SettingsReader;
using Lykke.Service.IcoApi.Core.Repositories;

namespace Lykke.Services.IcoApi.AzureRepositories
{
    public class InvestorTransactionHistoryRepository : IInvestorTransactionHistoryRepository
    {
        private readonly INoSQLTableStorage<InvestorTransactionHistoryEntity> _table;
        private static string GetPartitionKey(string investorEmail) => investorEmail;
        private static string GetRowKey() => DateTime.UtcNow.ToString("o");

        public InvestorTransactionHistoryRepository(IReloadingManager<string> connectionStringManager, ILog log)
        {
            _table = AzureTableStorage<InvestorTransactionHistoryEntity>.Create(connectionStringManager, "InvestorTransactionHistory", log);
        }

        public async Task SaveAsync(string email, string reason, string messageJson)
        {
            await _table.InsertOrReplaceAsync(new InvestorTransactionHistoryEntity
            {
                PartitionKey = GetPartitionKey(email),
                RowKey = GetRowKey(),
                Reason = reason,
                MessageJson = messageJson
            });
        }
    }
}
