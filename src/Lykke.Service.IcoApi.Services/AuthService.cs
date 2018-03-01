using System;
using System.Threading.Tasks;
using Lykke.Service.IcoApi.Core.Repositories;
using Lykke.Service.IcoApi.Core.Services;

namespace Lykke.Service.IcoApi.Services
{
    public class AuthService : IAuthService
    {
        private IUserRepository _userRepository;
        private IUserAuthTokenRepository _userAuthTokenRepository;

        public AuthService(IUserRepository userRepository, IUserAuthTokenRepository userAuthTokenRepository)
        {
            _userRepository = userRepository;
            _userAuthTokenRepository = userAuthTokenRepository;
        }

        public async Task<string> Login(string username, string password)
        {
            if (!string.IsNullOrEmpty(username) &&
                !string.IsNullOrEmpty(password) &&
                await _userRepository.Exists(username, password))
            {
                var authToken = Guid.NewGuid().ToString();

                await _userAuthTokenRepository.Insert(authToken, username);

                return authToken;
            }

            return null;
        }

        public async Task<bool> IsValid(string authToken)
        {
            return
                !string.IsNullOrEmpty(authToken) &&
                !string.IsNullOrEmpty(await _userAuthTokenRepository.GetUsername(authToken));
        }
    }
}
