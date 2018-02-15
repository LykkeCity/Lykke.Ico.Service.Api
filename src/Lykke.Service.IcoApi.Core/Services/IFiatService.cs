using Lykke.Service.IcoApi.Core.Domain.Fiat;
using System.Threading.Tasks;

namespace Lykke.Service.IcoApi.Core.Services
{
    public interface IFiatService
    {
        Task<FiatCharge> Charge(string email, string token, int cents);
    }
}
