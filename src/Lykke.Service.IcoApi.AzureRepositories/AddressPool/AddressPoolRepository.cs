using AzureStorage;
using AzureStorage.Tables;
using Common.Log;
using Lykke.SettingsReader;
using System.Threading.Tasks;
using System.Linq;
using System;
using System.Collections.Generic;
using Lykke.Service.IcoApi.Core.Repositories;
using Lykke.Service.IcoApi.Core.Domain.AddressPool;

namespace Lykke.Services.IcoApi.AzureRepositories
{
    public class AddressPoolRepository : IAddressPoolRepository
    {
        private static readonly Object _lock = new Object();
        private readonly INoSQLTableStorage<AddressPoolEntity> _table;
        private static string GetPartitionKey() => "";
        private static string GetRowKey(int id) => id.ToString().PadLeft(9, '0');

        public AddressPoolRepository(IReloadingManager<string> connectionStringManager, ILog log)
        {
            _table = AzureTableStorage<AddressPoolEntity>.Create(connectionStringManager, "AddressPool", log);
        }

        public async Task<IAddressPoolItem> Get(int id)
        {
            return await _table.GetDataAsync(GetPartitionKey() ,GetRowKey(id));
        }

        public async Task AddBatchAsync(List<IAddressPoolItem> keys)
        {
            var entities = keys.Select(f => new AddressPoolEntity
            {
                BtcPublicKey = f.BtcPublicKey,
                EthPublicKey = f.EthPublicKey,
                PartitionKey = GetPartitionKey(),
                RowKey = GetRowKey(f.Id)
            });

            await _table.InsertOrMergeBatchAsync(entities);
        }
    }
}
