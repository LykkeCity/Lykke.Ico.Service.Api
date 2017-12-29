using Lykke.Ico.Core.Helpers;
using Lykke.Service.IcoApi.Core.Services;

namespace Lykke.Service.IcoApi.Services
{
    public class EncryptionService : IEncryptionService
    {
        private readonly string _key;
        private readonly string _iv;

        public EncryptionService(string key, string iv)
        {
            _key = key;
            _iv = iv;
        }

        public string Decrypt(string message)
        {
            return EncryptionHelper.Decrypt(message, _key, _iv);
        }
    }
}
