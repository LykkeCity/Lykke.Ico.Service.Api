using Lykke.Service.IcoApi.Core.Settings.ServiceSettings;

namespace Lykke.Service.IcoJob.Settings
{
    public class IcoJobSettings
    {
        public AzureQueueSettings AzureQueue { get; set; }
        public DbSettings Db { get; set; }
        public string IcoExRateServiceUrl { get; set; }
        public string IcoCommonServiceUrl { get; set; }
        public string BtcNetwork { get; set; }
    }
}
