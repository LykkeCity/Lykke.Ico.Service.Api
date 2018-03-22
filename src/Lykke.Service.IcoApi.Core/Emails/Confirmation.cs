using Lykke.Service.IcoApi.Core.Domain;

namespace Lykke.Service.IcoApi.Core.Emails
{
    [EmailData(Consts.Emails.Confirmation)]
    public class Confirmation
    {
        public Confirmation() => AuthToken = TokenName = string.Empty;

        public string AuthToken { get; set; }
        public string TokenName { get; set; }
    }
}
