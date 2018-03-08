using System.ComponentModel.DataAnnotations;

namespace Lykke.Service.IcoApi.Core.Emails
{
    [EmailData("confirmation")]
    public class Confirmation
    {
        public Confirmation() => AuthToken = string.Empty;

        [Display(Description = "")]
        public string AuthToken { get; set; }
    }
}
