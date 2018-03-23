using Lykke.Service.IcoApi.Core.Domain.AddressPool;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Lykke.Service.IcoApi.Core.Repositories
{
    public interface IAddressPoolRepository
    {
        Task AddBatchAsync(List<IAddressPoolItem> keys);

        Task<IAddressPoolItem> Get(int id);
    }
}
