using Lykke.Ico.Core.Repositories.Investor;
using Lykke.Service.IcoApi.Core.Domain;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

namespace Lykke.Service.IcoApi.Models
{
    public class RegisterInvestorRequest
    {
        [Required]
        [EmailAddress]
        [JsonProperty("email")]
        public string Email { get; set; }
    }

    public class RegisterInvestorResponse
    {
        [JsonProperty("result")]
        public RegisterResult Result { get; set; }
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
        [JsonProperty("tokenAddress")]
        public string TokenAddress { get; set; }

        [JsonProperty("refundEthAddress")]
        public string RefundEthAddress { get; set; }

        [JsonProperty("refundBtcAddress")]
        public string RefundBtcAddress { get; set; }
    }

    public class InvestorResponse
    {
        [JsonProperty("email")]
        string Email { get; set; }

        [JsonProperty("tokenAddress")]
        string TokenAddress { get; set; }

        [JsonProperty("refundEthAddress")]
        string RefundEthAddress { get; set; }

        [JsonProperty("refundBtcAddress")]
        string RefundBtcAddress { get; set; }

        [JsonProperty("payInEthAddress")]
        string PayInEthAddress { get; set; }

        [JsonProperty("payInBtcAddress")]
        string PayInBtcAddress { get; set; }

        public static InvestorResponse Create(IInvestor investor)
        {
            return new InvestorResponse
            {
                Email = investor.Email,
                TokenAddress = investor.TokenAddress,
                RefundEthAddress = investor.RefundEthAddress,
                RefundBtcAddress = investor.RefundBtcAddress,
                PayInEthAddress = investor.PayInEthAddress,
                PayInBtcAddress = investor.PayInBtcAddress
            };
        }
    }
}
