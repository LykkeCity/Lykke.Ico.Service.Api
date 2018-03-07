using System.ComponentModel.DataAnnotations;

namespace Lykke.Service.IcoApi.Core.Emails
{
    public class InvestorConfirmationMessage
    {
        public InvestorConfirmationMessage() => AuthToken = string.Empty;

        [Display(Description = "")]
        public string AuthToken { get; set; }
    }
}
