using System.Threading.Tasks;

namespace Lykke.Service.IcoApi.Core.Services
{
    public interface ISupportService
    {
        Task UpdateInvestorAsync(string email, string tokenAddress, string refundEthAddress, string refundBtcAddress);
        Task UpdateMinInvestAmount(decimal minInvestAmountUsd);
    }
}
