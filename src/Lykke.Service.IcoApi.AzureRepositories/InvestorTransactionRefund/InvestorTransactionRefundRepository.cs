using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using AzureStorage;
using AzureStorage.Tables;
using Common.Log;
using Lykke.SettingsReader;
using Lykke.Service.IcoApi.Core.Domain.Investor;
using Lykke.Service.IcoApi.Core.Repositories;

namespace Lykke.Services.IcoApi.AzureRepositories
{
    public class InvestorTransactionRefundRepository : IInvestorTransactionRefundRepository
    {
        private readonly INoSQLTableStorage<InvestorTransactionRefundEntity> _table;
        private static string GetPartitionKey(string investorEmail) => investorEmail;
        private static string GetRowKey(string uniqueId) => uniqueId;

        public InvestorTransactionRefundRepository(IReloadingManager<string> connectionStringManager, ILog log)
        {
            _table = AzureTableStorage<InvestorTransactionRefundEntity>.Create(connectionStringManager, "InvestorTransactionRefunds", log);
        }

        public async Task<IInvestorTransactionRefund> GetAsync(string email, string uniqueId)
        {
            return await _table.GetDataAsync(GetPartitionKey(email), GetRowKey(uniqueId));
        }

        public async Task<IEnumerable<IInvestorTransactionRefund>> GetAllAsync()
        {
            var entities = await _table.GetDataAsync();

            return entities.OrderBy(f => f.CreatedUtc);
        }

        public async Task<IEnumerable<IInvestorTransactionRefund>> GetByEmailAsync(string email)
        {
            var entities = await _table.GetDataAsync(GetPartitionKey(email));

            return entities.OrderBy(f => f.CreatedUtc);
        }

        public async Task SaveAsync(string email, string uniqueId, string messageJson)
        {
            await _table.InsertOrReplaceAsync(new InvestorTransactionRefundEntity
            {
                PartitionKey = GetPartitionKey(email),
                RowKey = GetRowKey(uniqueId),
                MessageJson = messageJson
            });
        }

        public async Task RemoveAsync(string email)
        {
            var items = await _table.GetDataAsync(GetPartitionKey(email));

            await _table.DeleteAsync(items);
        }
    }
}
