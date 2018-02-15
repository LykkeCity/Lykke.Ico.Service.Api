using Lykke.Service.IcoApi.Core.Domain.Campaign;
using System.Threading.Tasks;

namespace Lykke.Service.IcoApi.Core.Repositories
{
    public interface ICampaignSettingsRepository
    {
        Task<ICampaignSettings> GetAsync();
        Task SaveAsync(ICampaignSettings settings);
    }
}
