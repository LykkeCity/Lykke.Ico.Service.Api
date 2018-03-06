using Lykke.Service.IcoApi.Core.Settings.SlackNotifications;

namespace Lykke.Service.IcoJob.Settings
{
    public class AppSettings
    {
        public IcoJobSettings IcoJob { get; set; }
        public SlackNotificationsSettings SlackNotifications { get; set; }
    }
}
