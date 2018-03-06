using System.ComponentModel.DataAnnotations;

namespace Lykke.Service.IcoApi.Core.Emails
{
    public class InvestorConfirmationMessage
    {
        public InvestorConfirmationMessage() => ConfirmationLink = string.Empty;

        [Display(Description = "")]
        public string ConfirmationLink { get; set; }
    }
}
