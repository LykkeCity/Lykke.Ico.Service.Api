using System.ComponentModel.DataAnnotations;

namespace Lykke.Service.IcoApi.Core.Emails
{
    [EmailData("confirmation")]
    public class Confirmation
    {
        public Confirmation() => AuthToken = TokenName = string.Empty;

        public string AuthToken { get; set; }
        public string TokenName { get; set; }
    }
}
