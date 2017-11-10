using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

namespace Lykke.Service.IcoApi.Models
{
    public class RegisterInvestorRequest
    {
        [Required]
        [JsonProperty("email")]
        public string Email { get; set; }
    }

    public class ConfirmInvestorRequest
    {
        [Required]
        [JsonProperty("email")]
        public string Email { get; set; }

        [Required]
        [JsonProperty("verificationCode")]
        public string VerificationCode { get; set; }
    }

    public class ConfirmInvestorResponse
    {
        [JsonProperty("authToken")]
        public string AuthToken { get; set; }
    }

    public class InvestorRequest
    {
        [Required]
        [JsonProperty("rndAddress")]
        public string RndAddress { get; set; }

        [JsonProperty("ethRefundAddress")]
        public string EthRefundAddress { get; set; }

        [JsonProperty("btcRefundAddress")]
        public string BtcRefundAddress { get; set; }
    }

    public class InvestorResponse
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
