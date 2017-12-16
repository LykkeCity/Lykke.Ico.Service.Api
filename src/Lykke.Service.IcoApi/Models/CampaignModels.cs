using System;

namespace Lykke.Service.IcoApi.Models
{
    public class CampaignResponse
    {
        public DateTime PreSaleStartDateTimeUtc { get; set; }

        public DateTime PreSaleEndDateTimeUtc { get; set; }

        public int PreSaleTokensTotal { get; set; }

        public DateTime CrowdSaleStartDateTimeUtc { get; set; }

        public DateTime CrowdSaleEndDateTimeUtc { get; set; }

        public int CrowdSaleTokensTotal { get; set; }

        public decimal TokensSold { get; set; }

        public int Investors { get; set; }
    }
}
