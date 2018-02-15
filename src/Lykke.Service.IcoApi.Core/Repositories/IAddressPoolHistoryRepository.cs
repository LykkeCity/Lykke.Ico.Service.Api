using Lykke.Service.IcoApi.Core.Domain.AddressPool;
using System.Threading.Tasks;

namespace Lykke.Service.IcoApi.Core.Repositories
{
    public interface IAddressPoolHistoryRepository
    {
        Task<IAddressPoolHistoryItem> Get(int id);

        Task SaveAsync(IAddressPoolItem addressPoolItem, string email);
    }
}
