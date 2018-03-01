using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AzureStorage;
using AzureStorage.Tables;
using Common.Log;
using Lykke.Service.IcoApi.Core.Repositories;
using Lykke.SettingsReader;

namespace Lykke.Service.IcoApi.AzureRepositories.Auth
{
    public class UserAuthTokenRepository : IUserAuthTokenRepository
    {
        private readonly INoSQLTableStorage<UserAuthTokenEntity> _table;
        private static string GetPartitionKey(string authToken) => authToken;
        private static string GetRowKey() => string.Empty;

        public UserAuthTokenRepository(IReloadingManager<string> connectionStringManager, ILog log)
        {
            _table = AzureTableStorage<UserAuthTokenEntity>.Create(connectionStringManager, "UserAuthTokens", log);
        }

        public async Task Insert(string authToken, string username)
        {
            await _table.InsertAsync(new UserAuthTokenEntity()
            {
                PartitionKey = GetPartitionKey(authToken),
                RowKey = GetRowKey(),
                Username = username
            });
        }

        public async Task<(string username, DateTime issuedUtc)> Get(string authToken)
        {
            var entity = await _table.GetDataAsync(GetPartitionKey(authToken), GetRowKey());

            if (entity != null)
            {
                return (entity.Username, entity.Timestamp.UtcDateTime);
            }
            else
            {
                return (null, default(DateTime));
            }
        }

        public async Task<string> GetUsername(string authToken)
        {
            return (await _table.GetDataAsync(GetPartitionKey(authToken), GetRowKey()))?.Username;
        }
    }
}
