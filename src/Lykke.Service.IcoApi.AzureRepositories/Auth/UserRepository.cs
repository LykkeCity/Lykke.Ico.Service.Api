using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using AzureStorage;
using AzureStorage.Tables;
using Common.Log;
using Lykke.Service.IcoApi.Core.Repositories;
using Lykke.SettingsReader;

namespace Lykke.Service.IcoApi.AzureRepositories.Auth
{
    public class UserRepository : IUserRepository
    {
        private readonly INoSQLTableStorage<UserEntity> _table;
        private static string GetPartitionKey(string username) => username;
        private static string GetRowKey(string password) => password;

        public UserRepository(IReloadingManager<string> connectionStringManager, ILog log)
        {
            _table = AzureTableStorage<UserEntity>.Create(connectionStringManager, "Users", log);
        }

        public async Task<bool> Exists(string username, string password)
        {
            return await _table.RecordExistsAsync(new UserEntity()
            {
                PartitionKey = GetPartitionKey(username),
                RowKey = GetRowKey(password)
            });
        }
    }
}
