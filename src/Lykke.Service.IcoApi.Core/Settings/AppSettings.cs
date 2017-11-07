using Lykke.Service.IcoApi.Core.Settings.ServiceSettings;
using Lykke.Service.IcoApi.Core.Settings.SlackNotifications;

namespace Lykke.Service.IcoApi.Core.Settings
{
    public class AppSettings
    {
        public IcoApiSettings IcoApiService { get; set; }
        public SlackNotificationsSettings SlackNotifications { get; set; }
    }
}
