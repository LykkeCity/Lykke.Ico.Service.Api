using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Lykke.Service.IcoApi.Core.Services
{
    public interface IAuthService
    {
        Task<string> Login(string username, string password);
        Task<bool> IsValid(string authToken);
    }
}
