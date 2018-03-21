using Lykke.Service.IcoApi.Core.Domain;

namespace Lykke.Service.IcoApi.Core.Emails
{
    [EmailData(Consts.Emails.KycReminder)]
    public class KycReminder
    {
        public KycReminder() => AuthToken = string.Empty;

        public string AuthToken { get; set; }
    }
}
