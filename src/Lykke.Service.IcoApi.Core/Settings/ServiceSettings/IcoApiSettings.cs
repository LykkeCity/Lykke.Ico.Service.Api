namespace Lykke.Service.IcoApi.Core.Settings.ServiceSettings
{
    public class IcoApiSettings
    {
        public DbSettings Db { get; set; }
        public string BtcUrl { get; set; }
        public string BtcNetwork { get; set; }
        public string BtcTrackerUrl { get; set; }
        public string BtcTestSecretKey { get; set; }
        public string EthUrl { get; set; }
        public string EthNetwork { get; set; }
        public string EthTrackerUrl { get; set; }
        public string EthTestSecretKey { get; set; }
        public string IcoExRateServiceUrl { get; set; }
        public string IcoCommonServiceUrl { get; set; }
        public string SiteEmailConfirmationPageUrl { get; set; }
        public string SiteSummaryPageUrl { get; set; }
        public string StripeSecretKey { get; set; }
        public string KycServiceEncriptionKey { get; set; }
        public string KycServiceEncriptionIv { get; set; }
        public string AdminAuthKey { get; set; }
        public bool DisableAdminMethods { get; set; }
        public bool DisableDebugMethods { get; set; }
        public string CaptchaSecret { get; set; }
        public string CampaignId { get; set; }
    }
}
