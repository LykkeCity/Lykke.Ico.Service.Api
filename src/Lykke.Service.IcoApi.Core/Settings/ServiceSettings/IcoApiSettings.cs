namespace Lykke.Service.IcoApi.Core.Settings.ServiceSettings
{
    public class IcoApiSettings
    {
        public DbSettings Db { get; set; }
        public string BtcNetwork { get; set; }
        public string BtcTestSecretKey { get; set; }
        public string EthNetwork { get; set; }
        public string EthTestSecretKey { get; set; }
        public string IcoExRateServiceUrl { get; set; }        
    }
}
