using System.Threading.Tasks;

namespace Lykke.Service.IcoJob.Services
{
    public interface IInvestorJobService
    {
        Task AssignPayInAddresses(string email);
    }
}
