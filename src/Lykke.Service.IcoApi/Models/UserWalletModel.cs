using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

namespace Lykke.Service.IcoApi.Models
{
    public class UserWalletRequest
    {
        [Required]
        [JsonProperty("rndAddress")]
        public string RndAddress { get; set; }

        [JsonProperty("ethRefundAddress")]
        public string EthRefundAddress { get; set; }

        [JsonProperty("btcRefundAddress")]
        public string BtcRefundAddress { get; set; }
    }

    public class UserWalletResponse
    {
        [JsonProperty("rndAddress")]
        public string RndAddress { get; set; }

        [JsonProperty("ethRefundAddress")]
        public string EthRefundAddress { get; set; }

        [JsonProperty("btcRefundAddress")]
        public string BtcRefundAddress { get; set; }

        [JsonProperty("ethCashinAddress")]
        public string EthCashinAddress { get; set; }

        [JsonProperty("btcCashinAddress")]
        public string BtcCashinAddress { get; set; }
    }
}
