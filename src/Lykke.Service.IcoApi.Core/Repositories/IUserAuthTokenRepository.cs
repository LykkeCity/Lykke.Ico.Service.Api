using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Lykke.Service.IcoApi.Core.Repositories
{
    public interface IUserAuthTokenRepository
    {
        Task Insert(string authToken, string username);
        Task<(string username, DateTime issuedUtc)> Get(string authToken);
        Task<string> GetUsername(string authToken);
    }
}
