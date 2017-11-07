using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

namespace Lykke.Service.IcoApi.Models
{
    public class RegisterUserRequest
    {
        [Required]
        [JsonProperty("email")]
        public string Email { get; set; }
    }

    public class ConfirmUserRequest
    {
        [Required]
        [JsonProperty("email")]
        public string Email { get; set; }

        [Required]
        [JsonProperty("verificationCode")]
        public string VerificationCode { get; set; }
    }

    public class ConfirmUserResponse
    {
        [JsonProperty("authToken")]
        public string AuthToken { get; set; }
    }
}
