﻿using Lykke.Service.IcoApi.Core.Services;
using Lykke.Service.IcoApi.Services.Helpers;

namespace Lykke.Service.IcoApi.Services
{
    public class UrlEncryptionService : IUrlEncryptionService
    {
        private readonly string _key;
        private readonly string _iv;

        public UrlEncryptionService(string key, string iv)
        {
            _key = key;
            _iv = iv;
        }

        public string Encrypt(string message)
        {
            return EncryptionHelper.Encrypt(message, _key, _iv);
        }

        public string Decrypt(string message)
        {
            return EncryptionHelper.Decrypt(message, _key, _iv);
        }
    }
}
