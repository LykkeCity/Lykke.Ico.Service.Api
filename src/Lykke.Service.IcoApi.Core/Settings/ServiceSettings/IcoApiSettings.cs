namespace Lykke.Service.IcoApi.Core.Settings.ServiceSettings
{
    public class IcoApiSettings
    {
        public AzureQueueSettings AzureQueue { get; set; }
        public DbSettings Db { get; set; }
        public string BtcUrl { get; set; }
        public string BtcNetwork { get; set; }
        public string BtcTestSecretKey { get; set; }
        public string EthUrl { get; set; }
        public string EthNetwork { get; set; }
        public string EthTestSecretKey { get; set; }
        public string IcoExRateServiceUrl { get; set; }
        public string IcoCommonServiceUrl { get; set; }
        public string AdminAuthKey { get; set; }
        public bool DisableAdminMethods { get; set; }
        public bool DisableDebugMethods { get; set; }
    }
}
