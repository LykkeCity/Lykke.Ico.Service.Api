namespace Lykke.Service.IcoApi.Models
{
    public class CampaignResponse
    {
        public bool CampaignActive { get; set; }        

        public bool CaptchaEnabled { get; set; }

        public string CampaignId { get; set; }

        public string TokenName { get; set; }
    }
}
