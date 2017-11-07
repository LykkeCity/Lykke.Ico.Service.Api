using System;
using Common.Log;

namespace Lykke.Service.IcoApi.Client
{
    public class IcoApiClient : IIcoApiClient, IDisposable
    {
        private readonly ILog _log;

        public IcoApiClient(string serviceUrl, ILog log)
        {
            _log = log;
        }

        public void Dispose()
        {
            //if (_service == null)
            //    return;
            //_service.Dispose();
            //_service = null;
        }
    }
}
