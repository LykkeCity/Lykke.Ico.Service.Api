using System.Threading.Tasks;

namespace Lykke.Service.IcoApi.Core.Services
{
    public interface IShutdownManager
    {
        Task StopAsync();
    }
}