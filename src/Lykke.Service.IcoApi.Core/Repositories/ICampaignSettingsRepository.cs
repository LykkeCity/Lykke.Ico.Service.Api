using Lykke.Service.IcoApi.Core.Domain.Campaign;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Lykke.Service.IcoApi.Core.Repositories
{
    public interface ICampaignSettingsRepository
    {
        Task<ICampaignSettings> GetAsync();
        Task<IEnumerable<ICampaignSettingsHistoryItem>> GetHistoryAsync();
        Task SaveAsync(ICampaignSettings settings, string username);
    }
}
