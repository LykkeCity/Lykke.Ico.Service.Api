using System.Threading.Tasks;

namespace Lykke.Service.IcoJob.Services
{
    public interface IInvestorService
    {
        Task AssignPayInAddresses(string email);
    }
}