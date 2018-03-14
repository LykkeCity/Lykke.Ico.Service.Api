using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Lykke.Service.IcoApi.Core.Services
{
    public interface IAuthService
    {
        /// <summary>
        /// Returns new authentication token if credentials are valid, otherwise returns null.
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        Task<string> Login(string username, string password);

        /// <summary>
        /// Returns username if authentication token is valid, otherwise returns null.
        /// </summary>
        /// <param name="authToken"></param>
        /// <returns></returns>
        Task<string> Authenticate(string authToken);
    }
}
