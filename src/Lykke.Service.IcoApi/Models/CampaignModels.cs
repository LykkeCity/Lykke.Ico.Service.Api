using System;

namespace Lykke.Service.IcoApi.Models
{
    public class CampaignResponse
    {
        public DateTime StartDateTimeUtc { get; set; }

        public DateTime EndDateTimeUtc { get; set; }

        public decimal TokensSold { get; set; }

        public decimal TokensTotal { get; set; }

        public int Investors { get; set; }
    }
}
