using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Lykke.Service.IcoApi.Core.Repositories
{
    public interface IUserRepository
    {
        Task<bool> Exists(string username, string password);
    }
}
