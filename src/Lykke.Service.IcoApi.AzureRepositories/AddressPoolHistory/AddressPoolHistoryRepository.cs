using System.Threading.Tasks;
using AzureStorage;
using AzureStorage.Tables;
using Common.Log;
using Lykke.SettingsReader;
using Lykke.Service.IcoApi.Core.Repositories;
using Lykke.Service.IcoApi.Core.Domain.AddressPool;

namespace Lykke.Services.IcoApi.AzureRepositories
{
    public class AddressPoolHistoryRepository : IAddressPoolHistoryRepository
    {
        private readonly INoSQLTableStorage<AddressPoolHistoryEntity> _table;
        private static string GetPartitionKey() => "";
        private static string GetRowKey(int id) => id.ToString().PadLeft(9, '0');

        public AddressPoolHistoryRepository(IReloadingManager<string> connectionStringManager, ILog log)
        {
            _table = AzureTableStorage<AddressPoolHistoryEntity>.Create(connectionStringManager, "AddressPoolHistory", log);
        }

        public async Task<IAddressPoolHistoryItem> Get(int id)
        {
            return await _table.GetDataAsync(GetPartitionKey(), GetRowKey(id));
        }

        public async Task SaveAsync(IAddressPoolItem addressPoolItem, string email)
        {
            await _table.InsertOrReplaceAsync(new AddressPoolHistoryEntity
            {
                PartitionKey = GetPartitionKey(),
                RowKey = GetRowKey(addressPoolItem.Id),
                Email = email,
                BtcPublicKey = addressPoolItem.BtcPublicKey,
                EthPublicKey = addressPoolItem.EthPublicKey
            });
        }
    }
}
